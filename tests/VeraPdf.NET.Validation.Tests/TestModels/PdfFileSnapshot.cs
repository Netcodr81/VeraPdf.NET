namespace VeraPdf.NET.Validation.Tests.TestModels;

public sealed class PdfFileSnapshot
{
    public bool HasPdfHeader { get; init; }

    public bool HasEofMarker { get; init; }

    public bool HasCatalogObject { get; init; }

    public int PageMarkerCount { get; init; }

    public int FileSize { get; init; }
}
