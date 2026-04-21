namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDICCBasedCMYK : IPDICCBased
{
    int OPM { get; }

    bool OverprintFlag { get; }
}
