namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IExtensionSchemasContainer
{
    string? Prefix { get; }

    bool IsValidBag { get; }

    int ExtensionSchemaDefinitionsCount { get; }
}
