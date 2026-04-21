namespace VeraPdf.NET.Model.Contracts.External;

public interface IICCInputProfile
{
    int N { get; }

    string? DeviceClass { get; }

    string? ColorSpace { get; }

    decimal Version { get; }

    bool IsValid { get; }
}
