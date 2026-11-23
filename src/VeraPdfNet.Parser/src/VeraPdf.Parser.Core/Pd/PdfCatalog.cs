using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public sealed class PdfCatalog : PdfDictionaryObject
{
    private PdfPageTree? _pageTree;

    public PdfCatalog(CosDictionary dictionary)
        : base(dictionary)
    {
    }

    public PdfMetadata? Metadata(PdfDocument document) =>
        Get("Metadata") is { } value &&
        document.Reader.ResolveReference(value) is CosStream stream
            ? new PdfMetadata(stream)
            : null;

    public PdfPageTree GetPageTree(PdfDocument document)
    {
        if (_pageTree is not null)
        {
            return _pageTree;
        }
        var value = Get("Pages") ??
            throw new IOException("The PDF catalog has no /Pages page tree.");
        var dictionary = document.Reader.ResolveDictionary(value);
        if (dictionary is null)
        {
            throw new IOException("The PDF page tree could not be resolved.");
        }
        _pageTree = new PdfPageTree(dictionary);
        return _pageTree;
    }

    public int GetPageCount(PdfDocument document) =>
        GetPageTree(document).GetPageCount(document);

    public IReadOnlyList<PdfPage> GetPages(PdfDocument document) =>
        GetPageTree(document).GetPages(document);

    public PdfPage? GetPage(PdfDocument document, int index) =>
        GetPageTree(document).GetPage(document, index);

    public PdfAnnotation? AcroForm(PdfDocument document) =>
        GetDictionary("AcroForm", document) is { } acroForm
            ? new PdfAnnotation(acroForm)
            : null;

    public PdfStructure? Structure(PdfDocument document) =>
        GetDictionary("StructTreeRoot", document) is { } structure
            ? new PdfStructure(structure)
            : null;
}
