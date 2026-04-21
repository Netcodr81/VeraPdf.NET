namespace VeraPdf.NET.Model.Contracts.Xmp;

public interface IExtensionSchemaDefinition
{
    bool IsNamespaceURIValidURI { get; }

    bool IsPrefixValidText { get; }

    bool IsPropertyValidSeq { get; }

    bool IsSchemaValidText { get; }

    bool IsValueTypeValidSeq { get; }

    int ExtensionSchemaPropertiesCount { get; }

    int ExtensionSchemaValueTypesCount { get; }
}
