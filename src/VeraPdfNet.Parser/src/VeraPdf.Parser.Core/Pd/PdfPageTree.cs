using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public sealed class PdfPageTree
{
    private readonly CosDictionary _dictionary;
    private IReadOnlyList<PdfPage>? _pages;

    public PdfPageTree(CosDictionary dictionary)
    {
        _dictionary = dictionary;
    }

    public CosDictionary Dictionary => _dictionary;

    public int GetPageCount(PdfDocument document)
    {
        if (_dictionary.TryGetValue("Count", out var count) &&
            document.Reader.ResolveReference(count) is CosInteger integer)
        {
            return checked((int)integer.Value);
        }

        return BuildPages(document).Count;
    }

    public IReadOnlyList<PdfPage> GetPages(PdfDocument document)
    {
        _pages ??= BuildPages(document);
        return _pages;
    }

    private IReadOnlyList<PdfPage> BuildPages(PdfDocument document)
    {
        var pages = new List<PdfPage>();
        var visited = new HashSet<CosDictionary>(ReferenceEqualityComparer.Instance);
        BuildPages(document, _dictionary, null, pages, visited);
        return pages;
    }

    private static void BuildPages(PdfDocument document, CosDictionary node,
        CosDictionary? parent, List<PdfPage> pages, HashSet<CosDictionary> visited)
    {
        if (!visited.Add(node))
        {
            throw new IOException("The PDF page tree contains a cycle.");
        }
        if (!node.TryGetValue("Kids", out var kidsValue))
        {
            if (node.TryGetValue("Type", out var typeValue) &&
                typeValue is CosName type &&
                type.Text == "Page")
            {
                pages.Add(new PdfPage(node, parent));
            }
            return;
        }

        var kids = document.Reader.ResolveReference(kidsValue);
        if (kids is not CosArray kidsArray)
        {
            throw new IOException("PDF page tree /Kids must be an array.");
        }
        foreach (var kid in kidsArray.Values)
        {
            var child = document.Reader.ResolveDictionary(kid);
            if (child is null)
            {
                throw new IOException("A PDF page-tree child could not be resolved.");
            }
            if (child.TryGetValue("Type", out var typeValue) &&
                typeValue is CosName type &&
                type.Text == "Page")
            {
                pages.Add(new PdfPage(child, node));
            }
            else
            {
                BuildPages(document, child, node, pages, visited);
            }
        }
    }

    public PdfPage? GetPage(PdfDocument document, int index)
    {
        if (index < 0)
        {
            return null;
        }
        var pages = GetPages(document);
        return index < pages.Count ? pages[index] : null;
    }
}
