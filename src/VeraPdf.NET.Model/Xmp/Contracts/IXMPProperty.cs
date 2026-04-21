namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IXMPProperty
{
    bool IsPredefinedInXMP2004 { get; }

    bool IsPredefinedInXMP2005 { get; }

    bool IsDefinedInCurrentPackage { get; }

    bool IsDefinedInMainPackage { get; }

    bool IsValueTypeCorrect { get; }

    string? PredefinedType { get; }
}
