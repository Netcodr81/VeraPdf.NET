namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDDeviceN : IPDColorSpace
{
    bool AreColorantsPresent { get; }
}
