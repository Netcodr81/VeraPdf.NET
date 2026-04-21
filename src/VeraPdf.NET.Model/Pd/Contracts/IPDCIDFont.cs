namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDCIDFont : IPDFont
{
    string? CIDToGIDMap { get; }

    bool ContainsCIDSet { get; }

    bool CidSetListsAllGlyphs { get; }
}
