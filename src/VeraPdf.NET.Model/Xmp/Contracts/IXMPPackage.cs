namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IXMPPackage
{
    bool IsSerializationValid { get; }

    string? ActualEncoding { get; }

    string? Bytes { get; }

    string? Encoding { get; }

    int PropertiesCount { get; }

    int ExtensionSchemasContainersCount { get; }
}
