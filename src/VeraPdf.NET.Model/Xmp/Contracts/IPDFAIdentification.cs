namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IPDFAIdentification
{
    int Part { get; }

    string? Conformance { get; }

    string? Rev { get; }
}
