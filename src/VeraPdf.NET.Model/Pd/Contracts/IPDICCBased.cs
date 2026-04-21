namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDICCBased : IPDColorSpace
{
    string? ICCProfileMD5 { get; }
}
