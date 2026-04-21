namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDAnnot
{
    string? Subtype { get; }

    bool ContainsAppearances { get; }

    bool ContainsA { get; }

    bool ContainsAA { get; }

    bool ContainsLang { get; }
}
