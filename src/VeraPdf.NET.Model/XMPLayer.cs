namespace VeraPdf.NET.Model;

/// <summary>
/// Parent type for all XMP objects.
/// </summary>
public class XMPObject : PDFObject
{
}

/// <summary>
/// XMP package.
/// </summary>
public class XMPPackage : XMPObject
{
    /// <summary>
    /// Collection of all XMP properties.
    /// </summary>
    public IReadOnlyList<XMPProperty> Properties { get; set; } = [];

    /// <summary>
    /// Collection of all extension schema containers defined in this package.
    /// </summary>
    public IReadOnlyList<ExtensionSchemasContainer> ExtensionSchemasContainers { get; set; } = [];

    /// <summary>
    /// True if package serialization is valid.
    /// </summary>
    public bool IsSerializationValid { get; set; }

    /// <summary>
    /// Actual encoding used for XMP package serialization.
    /// </summary>
    public string? ActualEncoding { get; set; }

    /// <summary>
    /// Value of the bytes attribute in package header.
    /// </summary>
    public string? Bytes { get; set; }

    /// <summary>
    /// Value of the encoding attribute in package header.
    /// </summary>
    public string? Encoding { get; set; }
}

/// <summary>
/// Main XMP package.
/// </summary>
public class MainXMPPackage : XMPPackage
{
    /// <summary>
    /// PDF/A identification object.
    /// </summary>
    public PDFAIdentification? PDFAIdentification { get; set; }

    /// <summary>
    /// True if XMP contains PDF/A identification.
    /// </summary>
    public bool ContainsPDFAIdentification { get; set; }

    /// <summary>
    /// PDF/UA identification object.
    /// </summary>
    public PDFUAIdentification? PDFUAIdentification { get; set; }

    /// <summary>
    /// True if XMP contains PDF/UA identification.
    /// </summary>
    public bool ContainsPDFUAIdentification { get; set; }

    /// <summary>
    /// Value of dc:title.
    /// </summary>
    public string? DcTitle { get; set; }

    /// <summary>
    /// Set of declarations.
    /// </summary>
    public ISet<string> Declarations { get; set; } = new HashSet<string>();
}

/// <summary>
/// XMP property.
/// </summary>
public class XMPProperty : XMPObject
{
    /// <summary>
    /// True if property is predefined for PDF/A-1.
    /// </summary>
    public bool IsPredefinedInXMP2004 { get; set; }

    /// <summary>
    /// True if property is predefined for PDF/A-2 or PDF/A-3.
    /// </summary>
    public bool IsPredefinedInXMP2005 { get; set; }

    /// <summary>
    /// True if property is defined in extension schema in current package.
    /// </summary>
    public bool IsDefinedInCurrentPackage { get; set; }

    /// <summary>
    /// True if property is defined in extension schema in main package.
    /// </summary>
    public bool IsDefinedInMainPackage { get; set; }

    /// <summary>
    /// True if property value type is correct.
    /// </summary>
    public bool IsValueTypeCorrect { get; set; }

    /// <summary>
    /// Predefined type from XMP spec or extension schema.
    /// </summary>
    public string? PredefinedType { get; set; }
}

/// <summary>
/// XMPMM History property.
/// </summary>
public class XMPMMHistoryProperty : XMPProperty
{
    /// <summary>
    /// Resource events from xmpMM History property.
    /// </summary>
    public IReadOnlyList<XMPMMHistoryResourceEvent> ResourceEvents { get; set; } = [];
}

/// <summary>
/// Language Alternative property.
/// </summary>
public class XMPLangAlt : XMPProperty
{
    /// <summary>
    /// True if language alternative has only x-default.
    /// </summary>
    public bool XDefault { get; set; }
}

/// <summary>
/// Resource event structure of xmpMM History property.
/// </summary>
public class XMPMMHistoryResourceEvent : XMPObject
{
    /// <summary>
    /// Action value.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Parameters value.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// When value.
    /// </summary>
    public string? When { get; set; }
}

/// <summary>
/// PDF/A identification object.
/// </summary>
public class PDFAIdentification : XMPObject
{
    /// <summary>
    /// Part value.
    /// </summary>
    public int Part { get; set; }

    /// <summary>
    /// Conformance value.
    /// </summary>
    public string? Conformance { get; set; }

    /// <summary>
    /// Rev value.
    /// </summary>
    public string? Rev { get; set; }

    /// <summary>
    /// Namespace prefix for part property.
    /// </summary>
    public string? PartPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for conformance property.
    /// </summary>
    public string? ConformancePrefix { get; set; }

    /// <summary>
    /// Namespace prefix for amd property.
    /// </summary>
    public string? AmdPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for corr property.
    /// </summary>
    public string? CorrPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for rev property.
    /// </summary>
    public string? RevPrefix { get; set; }
}

/// <summary>
/// PDF/UA identification object.
/// </summary>
public class PDFUAIdentification : XMPObject
{
    /// <summary>
    /// Part value.
    /// </summary>
    public int Part { get; set; }

    /// <summary>
    /// Rev value.
    /// </summary>
    public string? Rev { get; set; }

    /// <summary>
    /// Namespace prefix for part property.
    /// </summary>
    public string? PartPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for amd property.
    /// </summary>
    public string? AmdPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for corr property.
    /// </summary>
    public string? CorrPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for rev property.
    /// </summary>
    public string? RevPrefix { get; set; }
}

/// <summary>
/// Parent type for all extension schema definition objects.
/// </summary>
public class ExtensionSchemaObject : XMPObject
{
    /// <summary>
    /// True if object contains undefined fields.
    /// </summary>
    public bool ContainsUndefinedFields { get; set; }

    /// <summary>
    /// Comma-separated undefined fields.
    /// </summary>
    public string? UndefinedFields { get; set; }
}

/// <summary>
/// Container of extension schemas defined in XMP package.
/// </summary>
public class ExtensionSchemasContainer : XMPObject
{
    /// <summary>
    /// Extension schema definitions.
    /// </summary>
    public IReadOnlyList<ExtensionSchemaDefinition> ExtensionSchemaDefinitions { get; set; } = [];

    /// <summary>
    /// Namespace prefix for the container.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// True if extension schema container type is Bag.
    /// </summary>
    public bool IsValidBag { get; set; }
}

/// <summary>
/// Extension schema definition.
/// </summary>
public class ExtensionSchemaDefinition : ExtensionSchemaObject
{
    /// <summary>
    /// XMP properties defined in this extension schema.
    /// </summary>
    public IReadOnlyList<ExtensionSchemaProperty> ExtensionSchemaProperties { get; set; } = [];

    /// <summary>
    /// XMP value types defined in this extension schema.
    /// </summary>
    public IReadOnlyList<ExtensionSchemaValueType> ExtensionSchemaValueTypes { get; set; } = [];

    /// <summary>
    /// True if namespace URI is valid URI.
    /// </summary>
    public bool IsNamespaceURIValidURI { get; set; }

    /// <summary>
    /// True if preferred prefix is valid Text.
    /// </summary>
    public bool IsPrefixValidText { get; set; }

    /// <summary>
    /// True if property list is valid Seq.
    /// </summary>
    public bool IsPropertyValidSeq { get; set; }

    /// <summary>
    /// True if schema description is valid Text.
    /// </summary>
    public bool IsSchemaValidText { get; set; }

    /// <summary>
    /// True if value type list is valid Seq.
    /// </summary>
    public bool IsValueTypeValidSeq { get; set; }

    /// <summary>
    /// Namespace prefix for namespaceURI property.
    /// </summary>
    public string? NamespaceURIPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for prefix property.
    /// </summary>
    public string? PrefixPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for property list property.
    /// </summary>
    public string? PropertyPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for schema description property.
    /// </summary>
    public string? SchemaPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for value type list property.
    /// </summary>
    public string? ValueTypePrefix { get; set; }
}

/// <summary>
/// Extension schema property definition.
/// </summary>
public class ExtensionSchemaProperty : ExtensionSchemaObject
{
    /// <summary>
    /// Category field value.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// True if category is valid Text.
    /// </summary>
    public bool IsCategoryValidText { get; set; }

    /// <summary>
    /// True if description is valid Text.
    /// </summary>
    public bool IsDescriptionValidText { get; set; }

    /// <summary>
    /// True if name is valid Text.
    /// </summary>
    public bool IsNameValidText { get; set; }

    /// <summary>
    /// True if value type is valid Text.
    /// </summary>
    public bool IsValueTypeValidText { get; set; }

    /// <summary>
    /// True if value type is defined.
    /// </summary>
    public bool IsValueTypeDefined { get; set; }

    /// <summary>
    /// Namespace prefix for category property.
    /// </summary>
    public string? CategoryPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for description property.
    /// </summary>
    public string? DescriptionPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for name property.
    /// </summary>
    public string? NamePrefix { get; set; }

    /// <summary>
    /// Namespace prefix for value type property.
    /// </summary>
    public string? ValueTypePrefix { get; set; }
}

/// <summary>
/// Extension schema value type definition.
/// </summary>
public class ExtensionSchemaValueType : ExtensionSchemaObject
{
    /// <summary>
    /// Fields used in this type definition.
    /// </summary>
    public IReadOnlyList<ExtensionSchemaField> ExtensionSchemaFields { get; set; } = [];

    /// <summary>
    /// True if description is valid Text.
    /// </summary>
    public bool IsDescriptionValidText { get; set; }

    /// <summary>
    /// True if field list is valid Seq.
    /// </summary>
    public bool IsFieldValidSeq { get; set; }

    /// <summary>
    /// True if namespace URI is valid URI.
    /// </summary>
    public bool IsNamespaceURIValidURI { get; set; }

    /// <summary>
    /// True if preferred prefix is valid Text.
    /// </summary>
    public bool IsPrefixValidText { get; set; }

    /// <summary>
    /// True if type is valid Text.
    /// </summary>
    public bool IsTypeValidText { get; set; }

    /// <summary>
    /// Namespace prefix for description property.
    /// </summary>
    public string? DescriptionPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for field property.
    /// </summary>
    public string? FieldPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for namespace URI property.
    /// </summary>
    public string? NamespaceURIPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for prefix property.
    /// </summary>
    public string? PrefixPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for type property.
    /// </summary>
    public string? TypePrefix { get; set; }
}

/// <summary>
/// Extension schema field definition.
/// </summary>
public class ExtensionSchemaField : ExtensionSchemaObject
{
    /// <summary>
    /// True if description is valid Text.
    /// </summary>
    public bool IsDescriptionValidText { get; set; }

    /// <summary>
    /// True if name is valid Text.
    /// </summary>
    public bool IsNameValidText { get; set; }

    /// <summary>
    /// True if value type is valid Text.
    /// </summary>
    public bool IsValueTypeValidText { get; set; }

    /// <summary>
    /// True if value type is defined.
    /// </summary>
    public bool IsValueTypeDefined { get; set; }

    /// <summary>
    /// Namespace prefix for description field.
    /// </summary>
    public string? DescriptionPrefix { get; set; }

    /// <summary>
    /// Namespace prefix for name field.
    /// </summary>
    public string? NamePrefix { get; set; }

    /// <summary>
    /// Namespace prefix for value type field.
    /// </summary>
    public string? ValueTypePrefix { get; set; }
}


