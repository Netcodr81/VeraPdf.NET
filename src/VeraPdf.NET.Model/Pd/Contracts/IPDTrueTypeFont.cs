namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDTrueTypeFont : IPDSimpleFont
{
    bool DifferencesAreUnicodeCompliant { get; }
}
