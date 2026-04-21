namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDSeparation : IPDColorSpace
{
    string? Name { get; }

    bool AreTintAndAlternateConsistent { get; }
}
