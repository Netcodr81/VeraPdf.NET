using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Writer;
using System.Text;

namespace VeraPdf.Parser.Pd;

public sealed class PdfDocument : IDisposable
{
    private bool _disposed;

    public PdfDocumentReader Reader { get; }
    public PdfCatalog Catalog { get; }

    private PdfDocument(PdfDocumentReader reader)
    {
        Reader = reader;
        Catalog = CreateCatalog(reader);
    }

    public static PdfDocument Open(string path)
    {
        var reader = PdfDocumentReader.Open(path);
        try
        {
            return new PdfDocument(reader);
        }
        catch
        {
            reader.Dispose();
            throw;
        }
    }

    public static PdfDocument Open(Stream stream, bool leaveOpen = false)
    {
        var reader = PdfDocumentReader.Open(stream, leaveOpen);
        try
        {
            return new PdfDocument(reader);
        }
        catch
        {
            reader.Dispose();
            throw;
        }
    }

    private static PdfCatalog CreateCatalog(PdfDocumentReader reader)
    {
        if (!reader.Trailer.TryGetValue("Root", out var root))
        {
            throw new IOException("The PDF trailer has no /Root catalog.");
        }
        var catalogDictionary = reader.ResolveDictionary(root);
        if (catalogDictionary is null)
        {
            throw new IOException("The PDF catalog could not be resolved.");
        }
        return new PdfCatalog(catalogDictionary);
    }

    public IReadOnlyList<PdfPage> Pages => Catalog.GetPages(this);

    public PdfPage GetPage(int index) =>
        Catalog.GetPage(this, index) ??
        throw new ArgumentOutOfRangeException(nameof(index), "The page index is outside the PDF.");

    public int PageCount => Catalog.GetPageCount(this);

    public void SaveAs(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var output = File.Create(path);
        SaveAs(output);
    }

    public void SaveAs(Stream output)
    {
        ArgumentNullException.ThrowIfNull(output);
        ObjectDisposedException.ThrowIf(_disposed, this);

        static void WriteText(Stream stream, string text) =>
            stream.Write(Encoding.ASCII.GetBytes(text));

        WriteText(output, "%PDF-" + Reader.PdfVersion.ToString(
            System.Globalization.CultureInfo.InvariantCulture) + "\n");

        var offsets = new Dictionary<CosKey, long>();
        foreach (var key in Reader.Keys.OrderBy(item => item.ObjectNumber).ThenBy(item => item.Generation))
        {
            if (key.Generation != 0)
            {
                continue;
            }
            var resolved = Reader.Resolve(key);
            if (resolved is not CosIndirectObject indirect)
            {
                continue;
            }
            offsets[key] = output.Position;
            WriteText(output,
                $"{key.ObjectNumber} {key.Generation} obj\n");

            if (indirect.Value is CosStream stream)
            {
                var streamDictionary = new CosDictionary();
                foreach (var entry in stream.Dictionary.Entries)
                {
                    streamDictionary.Set(entry.Key, entry.Value);
                }
                streamDictionary.Set("Length", new CosInteger(stream.RawData.Length));
                WriteText(output, PdfCosWriter.ToPdf(streamDictionary));
                WriteText(output, "\nstream\n");
                output.Write(stream.RawData);
                WriteText(output, "\nendstream");
            }
            else
            {
                WriteText(output, PdfCosWriter.ToPdf(indirect.Value));
            }
            WriteText(output, "\nendobj\n");
        }

        if (offsets.Count == 0)
        {
            throw new IOException("The PDF has no indirect objects to rewrite.");
        }
        var xrefOffset = output.Position;
        var maximumObjectNumber = offsets.Keys.Max(key => key.ObjectNumber);
        var orderedKeys = offsets.Keys.OrderBy(key => key.ObjectNumber).ToArray();
        WriteText(output, $"xref\n0 {maximumObjectNumber + 1}\n");
        WriteText(output, "0000000000 65535 f \n");
        for (var objectNumber = 1L; objectNumber <= maximumObjectNumber; objectNumber++)
        {
            var key = orderedKeys.FirstOrDefault(entry => entry.ObjectNumber == objectNumber);
            if (!offsets.ContainsKey(key))
            {
                WriteText(output, "0000000000 00000 f \n");
            }
            else
            {
                WriteText(output, $"{offsets[key]:0000000000} 00000 n \n");
            }
        }

        CosValue root = Reader.Trailer.TryGetValue("Root", out var trailerRoot)
            ? trailerRoot
            : new CosNull();
        WriteText(output,
            $"trailer\n<< /Size {maximumObjectNumber + 1} /Root " +
            PdfCosWriter.ToPdf(root) + " >>\n");
        WriteText(output, $"startxref\n{xrefOffset}\n%%EOF");
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        Reader.Dispose();
    }
}
