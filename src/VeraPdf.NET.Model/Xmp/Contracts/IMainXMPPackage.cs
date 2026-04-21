namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IMainXMPPackage : IXMPPackage
{
    bool ContainsPDFAIdentification { get; }

    bool ContainsPDFUAIdentification { get; }

    string? DcTitle { get; }

    int DeclarationsCount { get; }
}
