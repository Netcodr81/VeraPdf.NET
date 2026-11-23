using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public sealed class PdfPage : PdfDictionaryObject
{
    private readonly CosDictionary? _parent;

    public PdfPage(CosDictionary dictionary, CosDictionary? parent)
        : base(dictionary)
    {
        _parent = parent;
    }

    public double[]? GetMediaBox(PdfDocument document)
    {
        var box = GetInheritedBox(document, "MediaBox");
        return box is null ? null : ClipToMediaBox(box);
    }

    public double[]? GetCropBox(PdfDocument document)
    {
        var media = GetMediaBox(document);
        var box = GetInheritedBox(document, "CropBox");
        if (box is null)
        {
            return media;
        }
        return Clip(box, media);
    }

    public double[]? GetBleedBox(PdfDocument document)
    {
        var box = GetInheritedBox(document, "BleedBox");
        return box is null ? GetCropBox(document) : ClipToMediaBox(box);
    }

    public double[]? GetTrimBox(PdfDocument document)
    {
        var box = GetInheritedBox(document, "TrimBox");
        return box is null ? GetCropBox(document) : ClipToMediaBox(box);
    }

    public double[]? GetArtBox(PdfDocument document)
    {
        var box = GetInheritedBox(document, "ArtBox");
        return box is null ? GetCropBox(document) : ClipToMediaBox(box);
    }

    private double[]? ClipToMediaBox(double[] box)
    {
        var media = GetMediaBoxNoClip();
        return Clip(box, media);
    }

    private double[]? GetMediaBoxNoClip() =>
        GetInheritedBoxNoDocument(null, "MediaBox");

    private double[]? GetInheritedBox(PdfDocument document, string name)
    {
        var page = Dictionary;
        var visited = new HashSet<CosDictionary>(ReferenceEqualityComparer.Instance);
        CosDictionary? parent = _parent;
        while (visited.Add(page))
        {
            if (page.TryGetValue(name, out var value))
            {
                var resolved = document.Reader.ResolveReference(value);
                if (resolved is CosArray array)
                {
                    return ReadBox(array);
                }
            }
            if (parent is null)
            {
                break;
            }
            page = parent;
            parent = page.TryGetValue("Parent", out var parentValue)
                ? document.Reader.ResolveDictionary(parentValue)
                : null;
        }
        return null;
    }

    private double[]? GetInheritedBoxNoDocument(PdfDocument? document, string name)
    {
        if (Dictionary.TryGetValue(name, out var value) && value is CosArray array)
        {
            return ReadBox(array);
        }
        return null;
    }

    private static double[]? ReadBox(CosArray array)
    {
        if (array.Values.Count < 4)
        {
            return null;
        }
        var box = new double[4];
        for (var i = 0; i < 4; i++)
        {
            var value = array.Values[i] switch
            {
                CosInteger integer => integer.Value,
                CosReal real => real.Value,
                _ => double.NaN
            };
            if (double.IsNaN(value))
            {
                return null;
            }
            box[i] = value;
        }
        return box;
    }

    private double[]? Clip(double[] box, double[]? boundary)
    {
        if (boundary is null)
        {
            return box;
        }
        return
        [
            Math.Max(box[0], boundary[0]),
            Math.Max(box[1], boundary[1]),
            Math.Min(box[2], boundary[2]),
            Math.Min(box[3], boundary[3])
        ];
    }

    public PdfResources? GetResources(PdfDocument document)
    {
        var page = Dictionary;
        var visited = new HashSet<CosDictionary>(ReferenceEqualityComparer.Instance);
        CosDictionary? parent = _parent;
        while (visited.Add(page))
        {
            if (page.TryGetValue("Resources", out var value))
            {
                var resources = document.Reader.ResolveDictionary(value);
                return resources is null ? null : new PdfResources(resources);
            }
            if (parent is null)
            {
                break;
            }
            page = parent;
            parent = page.TryGetValue("Parent", out var parentValue)
                ? document.Reader.ResolveDictionary(parentValue)
                : null;
        }
        return null;
    }

    public IReadOnlyList<PdfContentStream> GetContents(PdfDocument document)
    {
        if (!Dictionary.TryGetValue("Contents", out var value))
        {
            return [];
        }
        var resolved = document.Reader.ResolveReference(value);
        if (resolved is CosStream stream)
        {
            return [new PdfContentStream(stream)];
        }
        if (resolved is CosArray array)
        {
            var result = new List<PdfContentStream>();
            foreach (var item in array.Values)
            {
                var itemResolved = document.Reader.ResolveReference(item);
                if (itemResolved is CosStream itemStream)
                {
                    result.Add(new PdfContentStream(itemStream));
                }
            }
            return result;
        }
        return [];
    }

    public IReadOnlyList<PdfAnnotation> GetAnnotations(PdfDocument document)
    {
        if (!Dictionary.TryGetValue("Annots", out var value))
        {
            return [];
        }
        var resolved = document.Reader.ResolveReference(value);
        if (resolved is not CosArray array)
        {
            return [];
        }
        var annotations = new List<PdfAnnotation>();
        foreach (var item in array.Values)
        {
            var annotation = document.Reader.ResolveDictionary(item);
            if (annotation is not null)
            {
                annotations.Add(new PdfAnnotation(annotation));
            }
        }
        return annotations;
    }
}
