namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDType0Font : IPDFont
{
    string? CIDFontOrdering { get; }

    string? CMapOrdering { get; }

    string? CIDFontRegistry { get; }

    string? CMapRegistry { get; }

    int? CIDFontSupplement { get; }

    int? CMapSupplement { get; }

    string? CMapName { get; }
}
