namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDFont
{
    string? Type { get; }

    string? Subtype { get; }

    string? FontName { get; }

    bool IsSymbolic { get; }

    bool ContainsFontFile { get; }

    bool IsItalic { get; }
}
