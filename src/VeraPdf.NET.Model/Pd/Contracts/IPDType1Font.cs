namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDType1Font : IPDSimpleFont
{
    string? CharSet { get; }

    bool CharSetListsAllGlyphs { get; }
}
