// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Text;

namespace VeraPdf.Library.Xmp;

public enum XmpNodeKind
{
    Literal,
    Uri,
    Structure,
    UnorderedArray,
    OrderedArray,
    AlternativeArray
}

/// <summary>A raw XMP property or array item. Array items have an empty name and namespace.</summary>
public sealed class XmpNode
{
    internal XmpNode(string namespaceUri, string name, XmpNodeKind kind, string? value,
        string? language, IEnumerable<XmpNode> children, IEnumerable<XmpNode>? qualifiers = null,
        string? prefix = null)
    {
        NamespaceUri = namespaceUri;
        Name = name;
        Prefix = prefix;
        Kind = kind;
        Value = value;
        Language = language;
        Children = Array.AsReadOnly(children.ToArray());
        Qualifiers = Array.AsReadOnly((qualifiers ?? []).ToArray());
    }

    public string NamespaceUri { get; }
    public string Name { get; }
    internal string? Prefix { get; }
    public XmpNodeKind Kind { get; }
    public string? Value { get; }
    public string? Language { get; }
    public IReadOnlyList<XmpNode> Children { get; }
    /// <summary>Non-language qualifiers, addressed by namespace URI and local name.</summary>
    public IReadOnlyList<XmpNode> Qualifiers { get; }
}

/// <summary>
/// Reads a deliberately limited, unnormalized subset of XMP RDF. Unsupported RDF
/// constructs throw rather than returning an incomplete metadata tree.
/// </summary>
public sealed class XmpPacket
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Xml = XNamespace.Xml;
    private static readonly XNamespace Xmp = "adobe:ns:meta/";
    private const string DeprecatedDc = "http://purl.org/dc/1.1/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";
    private readonly IReadOnlyDictionary<XName, XmpNode> properties;

    private XmpPacket(string? about, Dictionary<XName, XmpNode> properties, string? packetHeader = null)
    {
        About = about;
        PacketHeader = packetHeader;
        this.properties = new System.Collections.ObjectModel.ReadOnlyDictionary<XName, XmpNode>(properties);
        Properties = Array.AsReadOnly(properties.Values.ToArray());
    }

    public string? About { get; }
    /// <summary>The last xpacket processing-instruction data seen before the discovered RDF root, if any.</summary>
    public string? PacketHeader { get; }
    public IReadOnlyList<XmpNode> Properties { get; }

    /// <summary>Creates an empty packet without an RDF subject or packet header.</summary>
    public static XmpPacket Create() => new(null, []);

    /// <summary>Looks up an exact top-level namespace URI and local name, without alias normalization.</summary>
    public XmpNode? GetProperty(string namespaceUri, string localName)
    {
        ArgumentException.ThrowIfNullOrEmpty(namespaceUri);
        ArgumentException.ThrowIfNullOrEmpty(localName);
        return properties.GetValueOrDefault(XName.Get(localName, namespaceUri));
    }

    /// <summary>
    /// Finds a node by an expanded XMP path, resolving registered aliases against this raw tree.
    /// A missing node returns null; this read-only operation never creates alias targets.
    /// Register custom namespaces in <paramref name="registry"/> before using them in a path.
    /// </summary>
    public XmpNode? GetPropertyByPath(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var rootName = segments[1].Name;
        var colon = rootName.IndexOf(':');
        var node = GetProperty(segments[0].Name, rootName[(colon + 1)..]);
        for (var index = 2; index < segments.Count && node is not null; index++)
            node = FollowPathStep(registry, node, segments[index]);
        return node;
    }

    /// <summary>
    /// Returns a packet with a simple string property updated or created at a top-level
    /// name, structure field, or qualifier. Intermediate nodes must already exist.
    /// Existing composite nodes cannot be replaced by a string.
    /// </summary>
    public XmpPacket WithPropertyString(XmpSchemaRegistry registry, string schemaUri, string path, string value) =>
        WithSimpleProperty(registry, schemaUri, path, value, false);

    /// <summary>
    /// Returns a packet with a simple property stored as an RDF URI resource. A
    /// literal target becomes a URI; existing qualifiers are preserved. Missing
    /// intermediate nodes are not created.
    /// </summary>
    public XmpPacket WithPropertyUri(XmpSchemaRegistry registry, string schemaUri, string path, string value) =>
        WithSimpleProperty(registry, schemaUri, path, value, true);

    private XmpPacket WithSimpleProperty(XmpSchemaRegistry registry, string schemaUri, string path, string value, bool uri)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(path) || value is null)
            throw new XmpException("Schema, property name and value are required", XmpError.BadParameter);
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var rootName = segments[1].Name;
        var rootKey = XName.Get(rootName[(rootName.IndexOf(':') + 1)..], segments[0].Name);
        properties.TryGetValue(rootKey, out var current);
        var ancestors = new List<(XmpNode Parent, XmpNode Child, bool Qualifier)>();
        XmpNode? parent = null;
        var missingIndex = current is null ? 1 : -1;
        for (var index = 2; index < segments.Count && current is not null; index++)
        {
            parent = current;
            var next = FollowPathStep(registry, current, segments[index]);
            if (next is null)
            {
                missingIndex = index;
                current = null;
                break;
            }
            if (segments[index].Kind == XmpPathSegmentKind.Qualifier &&
                segments[index].Name == "?xml:lang")
            {
                if (index != segments.Count - 1)
                    throw new XmpException("Cannot traverse a language qualifier", XmpError.BadPath);
                current = next;
                break;
            }
            ancestors.Add((current, next, segments[index].Kind == XmpPathSegmentKind.Qualifier));
            current = next;
        }

        if (missingIndex >= 0 && missingIndex != segments.Count - 1)
            throw new XmpException("Cannot create missing intermediate nodes", XmpError.BadPath);
        var last = segments[^1];
        var cleanedValue = new string(value.Select(c => IsControl(c) ? ' ' : c).ToArray());
        XmpNode replacement;
        if (last.Kind == XmpPathSegmentKind.Qualifier && last.Name == "?xml:lang")
        {
            if (uri) throw new XmpException("Language qualifiers cannot be URI resources", XmpError.BadPath);
            if (parent is null) throw new XmpException("Cannot create language qualifier", XmpError.BadPath);
            var language = XmpPathFactory.NormalizeLanguage(cleanedValue);
            if (parent.Language == language) return this;
            replacement = new XmpNode(parent.NamespaceUri, parent.Name, parent.Kind, parent.Value,
                language, parent.Children, parent.Qualifiers);
        }
        else if (current is not null)
        {
            if (IsComposite(current))
                throw new XmpException("Property must be simple", XmpError.BadPath);
            var kind = uri ? XmpNodeKind.Uri : current.Kind;
            if (current.Value == cleanedValue && current.Kind == kind) return this;
            replacement = new XmpNode(current.NamespaceUri, current.Name, kind, cleanedValue,
                current.Language, current.Children, current.Qualifiers);
        }
        else
        {
            if (last.Kind is not (XmpPathSegmentKind.Field or XmpPathSegmentKind.Qualifier))
                throw new XmpException("Cannot create property at this path", XmpError.BadPath);
            if (parent is not null && last.Kind == XmpPathSegmentKind.Field && parent.Kind != XmpNodeKind.Structure)
                throw new XmpException("Named children only allowed for structures", XmpError.BadPath);
            var name = last.Kind == XmpPathSegmentKind.Qualifier ? last.Name[1..] : last.Name;
            var colon = name.IndexOf(':');
            var ns = missingIndex == 1 ? segments[0].Name : registry.GetNamespaceUri(name[..colon]);
            if (ns is null) throw new XmpException("Unregistered namespace URI", XmpError.BadSchema);
            var newNode = new XmpNode(ns, name[(colon + 1)..], uri ? XmpNodeKind.Uri : XmpNodeKind.Literal,
                cleanedValue, null, []);
            if (parent is null) replacement = newNode;
            else
                replacement = new XmpNode(parent.NamespaceUri, parent.Name, parent.Kind, parent.Value,
                    parent.Language,
                    last.Kind == XmpPathSegmentKind.Field ? parent.Children.Append(newNode) : parent.Children,
                    last.Kind == XmpPathSegmentKind.Qualifier ? parent.Qualifiers.Append(newNode) : parent.Qualifiers);
        }

        for (var index = ancestors.Count - 1; index >= 0; index--)
        {
            var (ancestor, child, qualifier) = ancestors[index];
            var updated = (qualifier ? ancestor.Qualifiers : ancestor.Children)
                .Select(node => ReferenceEquals(node, child) ? replacement : node).ToArray();
            replacement = new XmpNode(ancestor.NamespaceUri, ancestor.Name, ancestor.Kind, ancestor.Value,
                ancestor.Language, qualifier ? ancestor.Children : updated, qualifier ? updated : ancestor.Qualifiers);
        }
        var propertiesCopy = new Dictionary<XName, XmpNode>(properties) { [rootKey] = replacement };
        return new XmpPacket(About, propertiesCopy, PacketHeader);
    }

    /// <summary>Returns a packet with a Boolean value stored using XMP's canonical spelling.</summary>
    public XmpPacket WithPropertyBoolean(XmpSchemaRegistry registry, string schemaUri, string path, bool value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.FromBoolean(value));

    /// <summary>Returns a packet with an invariant-culture 32-bit integer value.</summary>
    public XmpPacket WithPropertyInteger(XmpSchemaRegistry registry, string schemaUri, string path, int value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.FromInteger(value));

    /// <summary>Returns a packet with an invariant-culture 64-bit integer value.</summary>
    public XmpPacket WithPropertyLong(XmpSchemaRegistry registry, string schemaUri, string path, long value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.FromLong(value));

    /// <summary>Returns a packet with an invariant shortest-round-trip double value.</summary>
    public XmpPacket WithPropertyDouble(XmpSchemaRegistry registry, string schemaUri, string path, double value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.FromDouble(value));

    /// <summary>Returns a packet with an XMP date rendered by <see cref="XmpScalarConverter.FromDate"/>.</summary>
    public XmpPacket WithPropertyDate(XmpSchemaRegistry registry, string schemaUri, string path, XmpDateTime value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.FromDate(value));

    /// <summary>Returns a packet with a Base64-encoded byte array.</summary>
    public XmpPacket WithPropertyBase64(XmpSchemaRegistry registry, string schemaUri, string path, byte[] value) =>
        WithPropertyString(registry, schemaUri, path, XmpScalarConverter.EncodeBase64(value));

    /// <summary>
    /// Creates a structure or array at a missing top-level name or field of an
    /// existing structure. An existing node of the same kind is unchanged.
    /// </summary>
    public XmpPacket WithCompositeProperty(XmpSchemaRegistry registry, string schemaUri, string path, XmpNodeKind kind)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(path))
            throw new XmpException("Schema and property name are required", XmpError.BadParameter);
        if (kind is not (XmpNodeKind.Structure or XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            throw new XmpException("A structure or array kind is required", XmpError.BadOptions);
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var rootName = segments[1].Name;
        var root = GetProperty(segments[0].Name, rootName[(rootName.IndexOf(':') + 1)..]);
        var current = root;
        XmpNode? parent = null;
        var missingIndex = root is null ? 1 : -1;
        for (var index = 2; index < segments.Count && current is not null; index++)
        {
            parent = current;
            current = FollowPathStep(registry, current, segments[index]);
            if (current is null) missingIndex = index;
        }
        if (current is not null)
        {
            if (current.Kind != kind)
                throw new XmpException("Existing property has a different kind", XmpError.BadPath);
            return this;
        }
        return CreateMissingComposite(registry, segments, root, parent, missingIndex, kind, []);
    }

    /// <summary>
    /// Returns a packet without the node at the specified path. Missing nodes leave the packet
    /// unchanged; invalid paths throw. Other nodes and the original packet are not modified.
    /// </summary>
    public XmpPacket WithoutProperty(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(path))
            throw new XmpException("Schema and property name are required", XmpError.BadParameter);
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var rootName = segments[1].Name;
        var rootKey = XName.Get(rootName[(rootName.IndexOf(':') + 1)..], segments[0].Name);
        if (!properties.TryGetValue(rootKey, out var current)) return this;

        var ancestors = new List<(XmpNode Parent, XmpNode Child, bool Qualifier)>();
        XmpNode? replacement = null;
        for (var index = 2; index < segments.Count; index++)
        {
            var step = segments[index];
            var next = FollowPathStep(registry, current, step);
            if (next is null) return this;
            if (step.Kind == XmpPathSegmentKind.Qualifier && step.Name == "?xml:lang" && index == segments.Count - 1)
            {
                replacement = new XmpNode(current.NamespaceUri, current.Name, current.Kind, current.Value,
                    null, current.Children, current.Qualifiers);
                break;
            }
            ancestors.Add((current, next, step.Kind == XmpPathSegmentKind.Qualifier));
            current = next;
        }

        for (var index = ancestors.Count - 1; index >= 0; index--)
        {
            var (parent, child, qualifier) = ancestors[index];
            var nodes = qualifier ? parent.Qualifiers : parent.Children;
            var updated = nodes.SelectMany(node => !ReferenceEquals(node, child)
                ? new[] { node } : replacement is null ? [] : new[] { replacement }).ToArray();
            replacement = new XmpNode(parent.NamespaceUri, parent.Name, parent.Kind, parent.Value, parent.Language,
                qualifier ? parent.Children : updated, qualifier ? updated : parent.Qualifiers);
        }
        var propertiesCopy = new Dictionary<XName, XmpNode>(properties);
        if (replacement is null) propertiesCopy.Remove(rootKey);
        else propertiesCopy[rootKey] = replacement;
        return new XmpPacket(About, propertiesCopy, PacketHeader);
    }

    /// <summary>
    /// Removes an indicated property, the properties of a schema, or properties
    /// across all schemas. Internal properties are retained unless requested.
    /// Schema-wide alias removal removes each alias's entire actual property.
    /// </summary>
    public XmpPacket WithoutProperties(XmpSchemaRegistry registry, string? schemaUri = null, string? path = null,
        bool includeInternal = false, bool includeAliases = false)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (!string.IsNullOrEmpty(path))
        {
            if (string.IsNullOrEmpty(schemaUri))
                throw new XmpException("Property name requires schema namespace", XmpError.BadParameter);
            var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
            var root = segments[1].Name;
            if (!includeInternal && IsInternalProperty(segments[0].Name, root[(root.IndexOf(':') + 1)..]))
                return this;
            return WithoutProperty(registry, schemaUri, path);
        }

        var propertiesCopy = new Dictionary<XName, XmpNode>(properties);
        foreach (var (key, node) in properties)
        {
            if ((string.IsNullOrEmpty(schemaUri) || node.NamespaceUri == schemaUri) &&
                (includeInternal || !IsInternalProperty(node.NamespaceUri, node.Name)))
                propertiesCopy.Remove(key);
        }
        var result = propertiesCopy.Count == properties.Count
            ? this : new XmpPacket(About, propertiesCopy, PacketHeader);
        if (!string.IsNullOrEmpty(schemaUri) && includeAliases)
        {
            foreach (var alias in registry.FindAliases(schemaUri))
                result = result.WithoutProperty(registry, alias.NamespaceUri, alias.PropertyName);
        }
        return result;
    }

    private static bool IsInternalProperty(string schema, string name) => schema switch
    {
        "http://purl.org/dc/elements/1.1/" => name is "format" or "language",
        "http://ns.adobe.com/xap/1.0/" =>
            name is "BaseURL" or "CreatorTool" or "Format" or "Locale" or "MetadataDate" or "ModifyDate",
        "http://ns.adobe.com/pdf/1.3/" => name is "BaseURL" or "Creator" or "ModDate" or "PDFVersion" or "Producer",
        "http://ns.adobe.com/tiff/1.0/" => name is not ("ImageDescription" or "Artist" or "Copyright"),
        "http://ns.adobe.com/exif/1.0/" => name != "UserComment",
        "http://ns.adobe.com/photoshop/1.0/" => name == "ICCProfile",
        "http://ns.adobe.com/camera-raw-settings/1.0/" => name is "Version" or "RawFileName" or "ToneCurveName",
        "http://ns.adobe.com/exif/1.0/aux/" or "http://ns.adobe.com/StockPhoto/1.0/" or
        "http://ns.adobe.com/xap/1.0/mm/" or "http://ns.adobe.com/xap/1.0/t/" or
        "http://ns.adobe.com/xap/1.0/t/pg/" or "http://ns.adobe.com/xap/1.0/g/" or
        "http://ns.adobe.com/xap/1.0/g/img/" or "http://ns.adobe.com/xap/1.0/sType/Font#" => true,
        _ => false
    };

    /// <summary>Reads the string value at a path, or null for a missing property.</summary>
    public string? GetPropertyString(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        var node = GetPropertyForValue(registry, schemaUri, path, false);
        if (node is null) return null;
        return node.Value ?? "";
    }

    public bool? GetPropertyBoolean(XmpSchemaRegistry registry, string schemaUri, string path) =>
        ConvertValue(registry, schemaUri, path, XmpScalarConverter.ToBoolean);

    public int? GetPropertyInteger(XmpSchemaRegistry registry, string schemaUri, string path) =>
        ConvertValue(registry, schemaUri, path, XmpScalarConverter.ToInteger);

    public long? GetPropertyLong(XmpSchemaRegistry registry, string schemaUri, string path) =>
        ConvertValue(registry, schemaUri, path, XmpScalarConverter.ToLong);

    public double? GetPropertyDouble(XmpSchemaRegistry registry, string schemaUri, string path) =>
        ConvertValue(registry, schemaUri, path, XmpScalarConverter.ToDouble);

    public XmpDateTime? GetPropertyDate(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        var node = GetPropertyForValue(registry, schemaUri, path, true);
        return node is null ? null : XmpScalarConverter.ToDate(node.Value);
    }

    public byte[]? GetPropertyBase64(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        var node = GetPropertyForValue(registry, schemaUri, path, true);
        return node is null ? null : XmpScalarConverter.DecodeBase64(node.Value);
    }

    private T? ConvertValue<T>(XmpSchemaRegistry registry, string schemaUri, string path, Func<string?, T> convert)
        where T : struct
    {
        var node = GetPropertyForValue(registry, schemaUri, path, true);
        return node is null ? null : convert(node.Value);
    }

    private XmpNode? GetPropertyForValue(XmpSchemaRegistry registry, string schemaUri, string path, bool requireSimple)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri))
            throw new XmpException("Empty schema namespace URI", XmpError.BadParameter);
        if (string.IsNullOrEmpty(path))
            throw new XmpException("Empty property name", XmpError.BadParameter);
        var node = GetPropertyByPath(registry, schemaUri, path);
        if (node is not null && requireSimple && IsComposite(node))
            throw new XmpException("Property must be simple when a value type is requested", XmpError.BadPath);
        return node;
    }

    private static bool IsComposite(XmpNode node) =>
        node.Kind is XmpNodeKind.Structure or XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray;

    /// <summary>Counts items in an array path, returning zero for a missing property.</summary>
    public int CountArrayItems(XmpSchemaRegistry registry, string schemaUri, string arrayPath)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        var array = GetPropertyByPath(registry, schemaUri, arrayPath);
        if (array is null) return 0;
        if (array.Kind is not (XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            throw new XmpException("The named property is not an array", XmpError.BadPath);
        return array.Children.Count;
    }

    /// <summary>Gets a one-based array item, or the last item when index is -1; missing items return null.</summary>
    public XmpNode? GetArrayItem(XmpSchemaRegistry registry, string schemaUri, string arrayPath, int index)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        return GetPropertyByPath(registry, schemaUri, XmpPathFactory.ComposeArrayItemPath(arrayPath, index));
    }

    /// <summary>
    /// Returns a packet with an existing array item replaced by a new literal item.
    /// Indexes are one-based; -1 selects the last item. The old item's fields and
    /// qualifiers are discarded, including any language qualifier.
    /// </summary>
    public XmpPacket WithArrayItemString(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, string value) => UpdateSimpleArrayItem(registry, schemaUri, arrayPath, index, value, XmpNodeKind.Literal, false);

    /// <summary>
    /// Returns a packet with a new literal inserted before a one-based index. An index
    /// of count + 1 or -1 appends; the array must already exist.
    /// </summary>
    public XmpPacket WithInsertedArrayItemString(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, string value) => UpdateSimpleArrayItem(registry, schemaUri, arrayPath, index, value, XmpNodeKind.Literal, true);

    /// <summary>
    /// Replaces an existing item with a URI item, discarding the old item and its
    /// qualifiers. Index -1 selects the last item.
    /// </summary>
    public XmpPacket WithArrayItemUri(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, string value) => UpdateSimpleArrayItem(registry, schemaUri, arrayPath, index, value, XmpNodeKind.Uri, false);

    /// <summary>
    /// Inserts a URI item before a one-based index. Index -1 or count + 1
    /// appends; the containing array must already exist.
    /// </summary>
    public XmpPacket WithInsertedArrayItemUri(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, string value) => UpdateSimpleArrayItem(registry, schemaUri, arrayPath, index, value, XmpNodeKind.Uri, true);

    /// <summary>
    /// Replaces an existing item with an empty structure or array, discarding the
    /// old item and its qualifiers. Index -1 selects the last item.
    /// </summary>
    public XmpPacket WithArrayItemComposite(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, XmpNodeKind itemKind) =>
        UpdateArrayItemComposite(registry, schemaUri, arrayPath, index, itemKind, false);

    /// <summary>
    /// Inserts an empty structure or array before a one-based index. Index -1 or
    /// count + 1 appends; the containing array must already exist.
    /// </summary>
    public XmpPacket WithInsertedArrayItemComposite(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, XmpNodeKind itemKind) =>
        UpdateArrayItemComposite(registry, schemaUri, arrayPath, index, itemKind, true);

    /// <summary>
    /// Appends an empty structure or array. A missing top-level array or field of
    /// an existing structure can be created by specifying its array kind.
    /// </summary>
    public XmpPacket WithAppendedArrayItemComposite(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        XmpNodeKind itemKind, XmpNodeKind? arrayKind = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        var item = CreateCompositeArrayItem(itemKind);
        return AppendArrayItem(registry, schemaUri, arrayPath, item, arrayKind);
    }

    /// <summary>
    /// Appends a literal item, optionally creating a top-level array or a field of an
    /// existing structure. Creating an array requires an explicit array kind.
    /// </summary>
    public XmpPacket WithAppendedArrayItemString(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        string value, XmpNodeKind? arrayKind = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        return AppendArrayItem(registry, schemaUri, arrayPath, CreateSimpleArrayItem(value, XmpNodeKind.Literal), arrayKind);
    }

    /// <summary>
    /// Appends a URI item, optionally creating a top-level array or a field of an
    /// existing structure. Creating an array requires an explicit array kind.
    /// </summary>
    public XmpPacket WithAppendedArrayItemUri(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        string value, XmpNodeKind? arrayKind = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        return AppendArrayItem(registry, schemaUri, arrayPath, CreateSimpleArrayItem(value, XmpNodeKind.Uri), arrayKind);
    }

    private XmpPacket AppendArrayItem(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        XmpNode item, XmpNodeKind? arrayKind)
    {
        if (arrayKind is not (null or XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            throw new XmpException("Only array forms are supported", XmpError.BadOptions);
        var segments = XmpPath.Parse(registry, schemaUri, arrayPath).Segments;
        var rootName = segments[1].Name;
        var root = GetProperty(segments[0].Name, rootName[(rootName.IndexOf(':') + 1)..]);
        var current = root;
        XmpNode? parent = null;
        var missingIndex = root is null ? 1 : -1;
        for (var index = 2; index < segments.Count && current is not null; index++)
        {
            parent = current;
            current = FollowPathStep(registry, current, segments[index]);
            if (current is null) missingIndex = index;
        }
        if (current is not null)
        {
            if (!IsArray(current)) throw new XmpException("The named property is not an array", XmpError.BadPath);
            if (arrayKind is not null && arrayKind != current.Kind)
                throw new XmpException("Mismatch of specified and existing array form", XmpError.BadOptions);
            return UpdateArrayItem(registry, schemaUri, arrayPath, -1, item, true);
        }
        if (arrayKind is null)
            throw new XmpException("Explicit array kind required to create new array", XmpError.BadOptions);
        return CreateMissingComposite(registry, segments, root, parent, missingIndex, arrayKind.Value, [item]);
    }

    private XmpPacket UpdateSimpleArrayItem(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, string value, XmpNodeKind kind, bool insert)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        return UpdateArrayItem(registry, schemaUri, arrayPath, index, CreateSimpleArrayItem(value, kind), insert);
    }

    private static XmpNode CreateSimpleArrayItem(string value, XmpNodeKind kind)
    {
        if (value is null) throw new XmpException("Array item value is required", XmpError.BadParameter);
        return new XmpNode("", "", kind,
            new string(value.Select(c => IsControl(c) ? ' ' : c).ToArray()), null, []);
    }

    private XmpPacket UpdateArrayItemComposite(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, XmpNodeKind itemKind, bool insert)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, arrayPath);
        return UpdateArrayItem(registry, schemaUri, arrayPath, index, CreateCompositeArrayItem(itemKind), insert);
    }

    private static XmpNode CreateCompositeArrayItem(XmpNodeKind itemKind)
    {
        if (itemKind is not (XmpNodeKind.Structure or XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            throw new XmpException("A structure or array item kind is required", XmpError.BadOptions);
        return new XmpNode("", "", itemKind, null, null, []);
    }

    private XmpPacket UpdateArrayItem(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        int index, XmpNode item, bool insert)
    {
        var segments = XmpPath.Parse(registry, schemaUri, arrayPath).Segments;
        var rootName = segments[1].Name;
        var rootKey = XName.Get(rootName[(rootName.IndexOf(':') + 1)..], segments[0].Name);
        if (!properties.TryGetValue(rootKey, out var root))
            throw new XmpException("Specified array does not exist", XmpError.BadPath);
        XmpNode? array = root;
        for (var i = 2; i < segments.Count && array is not null; i++)
            array = FollowPathStep(registry, array, segments[i]);
        if (array is null) throw new XmpException("Specified array does not exist", XmpError.BadPath);
        if (!IsArray(array)) throw new XmpException("The named property is not an array", XmpError.BadPath);

        var maximum = array.Children.Count + (insert ? 1 : 0);
        if (index == -1) index = maximum;
        if (index < 1 || index > maximum)
            throw new XmpException("Array index out of bounds", XmpError.BadIndex);
        var children = array.Children.ToList();
        if (insert) children.Insert(index - 1, item);
        else children[index - 1] = item;
        var replacement = new XmpNode(array.NamespaceUri, array.Name, array.Kind, array.Value,
            array.Language, children, array.Qualifiers);
        var updated = new Dictionary<XName, XmpNode>(properties)
        {
            [rootKey] = ReplaceArrayNode(root, array, replacement, false)
        };
        return new XmpPacket(About, updated, PacketHeader);
    }

    /// <summary>Finds a structure field by namespace and simple field name, or null if absent.</summary>
    public XmpNode? GetStructField(XmpSchemaRegistry registry, string schemaUri, string structPath,
        string fieldNamespace, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(registry);
        RequireArrayPath(schemaUri, structPath);
        return GetPropertyByPath(registry, schemaUri,
            structPath + XmpPathFactory.ComposeStructFieldPath(registry, fieldNamespace, fieldName));
    }

    /// <summary>Finds a property qualifier by namespace and simple qualifier name, or null if absent.</summary>
    public XmpNode? GetQualifier(XmpSchemaRegistry registry, string schemaUri, string propertyPath,
        string qualifierNamespace, string qualifierName)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri))
            throw new XmpException("Empty schema namespace URI", XmpError.BadParameter);
        if (string.IsNullOrEmpty(propertyPath))
            throw new XmpException("Empty property name", XmpError.BadParameter);
        return GetPropertyByPath(registry, schemaUri,
            propertyPath + XmpPathFactory.ComposeQualifierPath(registry, qualifierNamespace, qualifierName));
    }

    /// <summary>
    /// Traverses an existing property subtree in document order. The root is included unless
    /// <see cref="XmpIterationOptions.JustChildren"/> is set. Qualifiers follow children.
    /// </summary>
    public IEnumerable<XmpIterationEntry> IterateProperty(XmpSchemaRegistry registry, string schemaUri, string path,
        XmpIterationOptions options = XmpIterationOptions.None)
    {
        var (root, initialPath) = GetIterationRoot(registry, schemaUri, path, options);
        return root is null ? [] : IteratePropertyCore(registry, root, initialPath, options);
    }

    /// <summary>
    /// Creates a property-scoped cursor with controls for skipping the current subtree or remaining siblings.
    /// </summary>
    public XmpPropertyIterator CreatePropertyIterator(XmpSchemaRegistry registry, string schemaUri, string path,
        XmpIterationOptions options = XmpIterationOptions.None)
    {
        var (root, initialPath) = GetIterationRoot(registry, schemaUri, path, options);
        return new XmpPropertyIterator(registry, root, initialPath, options);
    }

    private (XmpNode? Root, string Path) GetIterationRoot(XmpSchemaRegistry registry, string schemaUri, string path,
        XmpIterationOptions options)
    {
        ValidateIterationOptions(options);
        var root = GetPropertyForValue(registry, schemaUri, path, false);
        if (root is null) return (null, "");
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var parentPath = new StringBuilder();
        for (var i = 1; i < segments.Count - 1; i++)
        {
            if (i > 1 && segments[i].Kind is XmpPathSegmentKind.Field or XmpPathSegmentKind.Qualifier)
                parentPath.Append('/');
            parentPath.Append(segments[i].Name);
        }
        var last = segments[^1];
        var leaf = last.Kind is XmpPathSegmentKind.ArrayIndex or XmpPathSegmentKind.ArrayLast
            or XmpPathSegmentKind.FieldSelector or XmpPathSegmentKind.QualifierSelector
            ? "[1]" : last.Name.TrimStart('?');
        var initialPath = AccumulateIterationPath(parentPath.ToString(), leaf, options);
        return (root, initialPath);
    }

    /// <summary>
    /// Traverses the raw packet or one schema. Schema entries have no node or path.
    /// Pass null for <paramref name="schemaUri"/> to traverse all schemas.
    /// </summary>
    public IEnumerable<XmpMetadataIterationEntry> Iterate(XmpSchemaRegistry registry, string? schemaUri = null,
        XmpIterationOptions options = XmpIterationOptions.None)
    {
        ValidateMetadataIteration(registry, schemaUri, options);
        return IterateSchemasCore(registry, schemaUri, options);
    }

    /// <summary>Creates a packet- or schema-scoped cursor with subtree and sibling skip controls.</summary>
    public XmpMetadataIterator CreateIterator(XmpSchemaRegistry registry, string? schemaUri = null,
        XmpIterationOptions options = XmpIterationOptions.None)
    {
        ValidateMetadataIteration(registry, schemaUri, options);
        return new XmpMetadataIterator(Properties, registry, schemaUri, options);
    }

    private static void ValidateMetadataIteration(XmpSchemaRegistry registry, string? schemaUri,
        XmpIterationOptions options)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ValidateIterationOptions(options);
        if (schemaUri is not null && schemaUri.Length == 0)
            throw new XmpException("Empty schema namespace URI", XmpError.BadParameter);
    }

    private IEnumerable<XmpMetadataIterationEntry> IterateSchemasCore(XmpSchemaRegistry registry, string? schemaUri,
        XmpIterationOptions options)
    {
        using var iterator = new XmpMetadataIterator(Properties, registry, schemaUri, options);
        while (iterator.MoveNext()) yield return iterator.Current;
    }

    private static void ValidateIterationOptions(XmpIterationOptions options)
    {
        const XmpIterationOptions valid = XmpIterationOptions.JustChildren | XmpIterationOptions.JustLeafNodes
            | XmpIterationOptions.JustLeafName | XmpIterationOptions.OmitQualifiers;
        if ((options & ~valid) != 0) throw new ArgumentOutOfRangeException(nameof(options));
    }

    private static IEnumerable<XmpIterationEntry> IteratePropertyCore(XmpSchemaRegistry registry, XmpNode root,
        string rootPath, XmpIterationOptions options)
    {
        using var iterator = new XmpPropertyIterator(registry, root, rootPath, options);
        while (iterator.MoveNext()) yield return iterator.Current;
    }

    internal static string IterationName(XmpSchemaRegistry registry, XmpNode node)
    {
        var prefix = registry.GetNamespacePrefix(node.NamespaceUri);
        if (prefix is null)
            throw new XmpException("Unregistered namespace URI in iterated node", XmpError.BadSchema);
        return prefix + node.Name;
    }

    internal static string AccumulateIterationPath(string parent, string segment, XmpIterationOptions options)
    {
        if (parent.Length == 0 || (options & XmpIterationOptions.JustLeafName) != 0) return segment;
        return parent + (segment.StartsWith('[') ? "" : "/") + segment;
    }

    internal static bool IsArray(XmpNode node) =>
        node.Kind is XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray;

    private static void RequireArrayPath(string schemaUri, string arrayPath)
    {
        if (string.IsNullOrEmpty(schemaUri))
            throw new XmpException("Empty schema namespace URI", XmpError.BadParameter);
        if (string.IsNullOrEmpty(arrayPath))
            throw new XmpException("Empty array name", XmpError.BadParameter);
    }

    internal XmpPacket WithSeparatedArrayItems(XmpSchemaRegistry registry, string schemaUri, string path,
        IReadOnlyList<string> values, XmpNodeKind? requestedKind)
    {
        var segments = XmpPath.Parse(registry, schemaUri, path).Segments;
        var rootName = segments[1].Name;
        var root = GetProperty(segments[0].Name, rootName[(rootName.IndexOf(':') + 1)..]);
        var current = root;
        XmpNode? parent = null;
        var missingIndex = root is null ? 1 : -1;
        for (var index = 2; index < segments.Count && current is not null; index++)
        {
            parent = current;
            current = FollowPathStep(registry, current, segments[index]);
            if (current is null) missingIndex = index;
        }

        XmpNode replacement;
        if (current is not null)
        {
            if (current.Kind is not (XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray))
                throw new XmpException("Named property must be non-alternate array", XmpError.BadPath);
            if (requestedKind is not null && requestedKind != current.Kind)
                throw new XmpException("Mismatch of specified and existing array form", XmpError.BadPath);
            var children = current.Children.ToList();
            var existing = new HashSet<string>(children.Where(child => child.Value is not null)
                .Select(child => child.Value!), StringComparer.Ordinal);
            foreach (var value in values)
                if (existing.Add(value)) children.Add(new XmpNode("", "", XmpNodeKind.Literal, value, null, []));
            if (children.Count == current.Children.Count) return this;
            replacement = new XmpNode(current.NamespaceUri, current.Name, current.Kind, current.Value,
                current.Language, children, current.Qualifiers);
        }
        else
        {
            return CreateMissingComposite(registry, segments, root, parent, missingIndex,
                requestedKind ?? XmpNodeKind.UnorderedArray,
                values.Select(value => new XmpNode("", "", XmpNodeKind.Literal, value, null, [])));
        }

        var updated = new Dictionary<XName, XmpNode>(properties);
        updated[XName.Get(root!.Name, root.NamespaceUri)] = ReplaceArrayNode(root, current!, replacement, false);
        return new XmpPacket(About, updated, PacketHeader);
    }

    private XmpPacket CreateMissingComposite(XmpSchemaRegistry registry, IReadOnlyList<XmpPathSegment> segments,
        XmpNode? root, XmpNode? parent, int missingIndex, XmpNodeKind kind, IEnumerable<XmpNode> children)
    {
        var last = segments[^1];
        if (last.Kind != XmpPathSegmentKind.Field || missingIndex != segments.Count - 1 ||
            (parent is not null && parent.Kind != XmpNodeKind.Structure))
            throw new XmpException("Cannot create composite property at this path", XmpError.BadPath);
        var colon = last.Name.IndexOf(':');
        var ns = segments.Count == 2 ? segments[0].Name : registry.GetNamespaceUri(last.Name[..colon]);
        if (ns is null) throw new XmpException("Unregistered namespace URI", XmpError.BadSchema);
        var array = new XmpNode(ns, last.Name[(colon + 1)..], kind, null, null, children);
        var updated = new Dictionary<XName, XmpNode>(properties);
        if (root is null) updated.Add(XName.Get(array.Name, array.NamespaceUri), array);
        else updated[XName.Get(root.Name, root.NamespaceUri)] = ReplaceArrayNode(root, parent!, array, true);
        return new XmpPacket(About, updated, PacketHeader);
    }

    private static XmpNode ReplaceArrayNode(XmpNode node, XmpNode target, XmpNode replacement, bool addChild)
    {
        if (ReferenceEquals(node, target))
            return addChild
                ? new XmpNode(node.NamespaceUri, node.Name, node.Kind, node.Value, node.Language,
                    node.Children.Append(replacement), node.Qualifiers)
                : replacement;
        var children = node.Children.Select(child => ReplaceArrayNode(child, target, replacement, addChild)).ToArray();
        var qualifiers = node.Qualifiers.Select(child => ReplaceArrayNode(child, target, replacement, addChild)).ToArray();
        if (children.SequenceEqual(node.Children) && qualifiers.SequenceEqual(node.Qualifiers)) return node;
        return new XmpNode(node.NamespaceUri, node.Name, node.Kind, node.Value, node.Language, children, qualifiers);
    }

    /// <summary>
    /// Returns a packet with localized text updated using exact, generic and
    /// x-default matching. A missing top-level array or field of an existing
    /// structure is created; other missing intermediate nodes are not created.
    /// </summary>
    public XmpPacket WithLocalizedText(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        string? genericLanguage, string specificLanguage, string value)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(arrayPath) ||
            string.IsNullOrEmpty(specificLanguage) || value is null)
            throw new XmpException("Schema, array name, specific language and value are required", XmpError.BadParameter);
        var generic = genericLanguage is null ? null : XmpPathFactory.NormalizeLanguage(genericLanguage);
        var specific = XmpPathFactory.NormalizeLanguage(specificLanguage);
        var cleanedValue = new string(value.Select(c => IsControl(c) ? ' ' : c).ToArray());
        var segments = XmpPath.Parse(registry, schemaUri, arrayPath).Segments;
        var rootName = segments[1].Name;
        var root = GetProperty(segments[0].Name, rootName[(rootName.IndexOf(':') + 1)..]);
        var array = root;
        XmpNode? parent = null;
        var missingIndex = root is null ? 1 : -1;
        for (var index = 2; index < segments.Count && array is not null; index++)
        {
            parent = array;
            array = FollowPathStep(registry, array, segments[index]);
            if (array is null) missingIndex = index;
        }
        if (array is not null && array.Kind != XmpNodeKind.AlternativeArray)
            throw new XmpException("Localized text array is not alt-text", XmpError.BadPath);

        var items = array?.Children.ToList() ?? [];
        if (items.Any(item => IsComposite(item) || item.Language is null))
            throw new XmpException("Alt-text array items must be simple and have a language", XmpError.BadPath);
        var defaultIndex = items.FindIndex(item => item.Language == "x-default");
        if (defaultIndex > 0)
        {
            var defaultItem = items[defaultIndex];
            items.RemoveAt(defaultIndex);
            items.Insert(0, defaultItem);
            defaultIndex = 0;
        }
        var specificIndex = items.FindIndex(item => item.Language == specific);
        var genericMatches = generic is null ? [] : items.Select((item, index) => (item, index))
            .Where(entry => entry.item.Language!.StartsWith(generic, StringComparison.Ordinal))
            .Select(entry => entry.index).ToArray();

        void SetValue(int index)
        {
            var item = items[index];
            items[index] = new XmpNode(item.NamespaceUri, item.Name, item.Kind, cleanedValue,
                item.Language, item.Children, item.Qualifiers);
        }
        void AddItem(string language)
        {
            var item = new XmpNode("", "", XmpNodeKind.Literal, cleanedValue, language, []);
            if (language == "x-default") items.Insert(0, item);
            else items.Add(item);
        }

        if (items.Count == 0)
        {
            AddItem("x-default");
            if (specific != "x-default") AddItem(specific);
        }
        else if (specificIndex >= 0)
        {
            var oldValue = items[specificIndex].Value;
            if (specific == "x-default")
            {
                for (var index = 0; index < items.Count; index++)
                    if (index != specificIndex && items[index].Value == oldValue) SetValue(index);
            }
            else if (defaultIndex >= 0 && defaultIndex != specificIndex && items[defaultIndex].Value == oldValue)
                SetValue(defaultIndex);
            SetValue(specificIndex);
        }
        else if (genericMatches.Length == 1)
        {
            var matched = genericMatches[0];
            if (defaultIndex >= 0 && defaultIndex != matched && items[defaultIndex].Value == items[matched].Value)
                SetValue(defaultIndex);
            SetValue(matched);
        }
        else
        {
            if (genericMatches.Length == 0 && defaultIndex >= 0 && items.Count == 1)
                SetValue(defaultIndex);
            AddItem(specific);
        }
        if (!items.Any(item => item.Language == "x-default") && items.Count == 1)
            AddItem("x-default");

        if (array is null)
            return CreateMissingComposite(registry, segments, root, parent, missingIndex, XmpNodeKind.AlternativeArray, items);
        var replacement = new XmpNode(array.NamespaceUri, array.Name, array.Kind, array.Value,
            array.Language, items, array.Qualifiers);
        var updated = new Dictionary<XName, XmpNode>(properties)
        {
            [XName.Get(root!.Name, root.NamespaceUri)] = ReplaceArrayNode(root, array, replacement, false)
        };
        return new XmpPacket(About, updated, PacketHeader);
    }

    /// <summary>
    /// Selects localized text using exact, generic, x-default, then first-item matching.
    /// Returns null if the property is absent. The parsed tree remains unchanged.
    /// </summary>
    public XmpNode? GetLocalizedText(XmpSchemaRegistry registry, string schemaUri, string arrayPath,
        string? genericLanguage, string specificLanguage)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(arrayPath) || string.IsNullOrEmpty(specificLanguage))
            throw new XmpException("Schema, array name and specific language are required", XmpError.BadParameter);
        var generic = genericLanguage is null ? null : XmpPathFactory.NormalizeLanguage(genericLanguage);
        var specific = XmpPathFactory.NormalizeLanguage(specificLanguage);
        var array = GetPropertyByPath(registry, schemaUri, arrayPath);
        if (array is null) return null;
        if (array.Kind != XmpNodeKind.AlternativeArray || !array.Children.Any(child => child.Language is not null))
            throw new XmpException("Localized text array is not alt-text", XmpError.BadPath);

        var items = array.Children.ToList();
        var defaultIndex = items.FindIndex(1, item => item.Language == "x-default");
        if (defaultIndex >= 1)
        {
            var defaultItem = items[defaultIndex];
            items.RemoveAt(defaultIndex);
            items.Insert(0, defaultItem);
            if (defaultIndex == 1)
            {
                var previous = items[1];
                items[1] = new XmpNode(previous.NamespaceUri, previous.Name, previous.Kind, defaultItem.Value,
                    previous.Language, previous.Children, previous.Qualifiers);
            }
        }

        XmpNode? genericMatch = null;
        XmpNode? defaultMatch = null;
        foreach (var item in items)
        {
            if (item.Kind is not (XmpNodeKind.Literal or XmpNodeKind.Uri))
                throw new XmpException("Alt-text array item is not simple", XmpError.BadPath);
            if (item.Language is null)
                throw new XmpException("Alt-text array item has no language qualifier", XmpError.BadPath);
            if (item.Language == specific) return item;
            if (generic is not null && item.Language.StartsWith(generic, StringComparison.Ordinal))
                genericMatch ??= item;
            else if (item.Language == "x-default")
                defaultMatch = item;
        }
        return genericMatch ?? defaultMatch ?? items[0];
    }

    private static XmpNode? FollowPathStep(XmpSchemaRegistry registry, XmpNode parent, XmpPathSegment step)
    {
        if (step.Kind == XmpPathSegmentKind.Qualifier)
            return FindQualifier(registry, parent, step.Name[1..]);
        if (step.Kind == XmpPathSegmentKind.Field)
        {
            if (parent.Kind != XmpNodeKind.Structure)
                throw new XmpException("Named children only allowed for structures", XmpError.BadPath);
            return FindNamedNode(registry, parent.Children, step.Name);
        }
        if (parent.Kind is not (XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray or XmpNodeKind.AlternativeArray))
            throw new XmpException("Indexing applied to non-array", XmpError.BadPath);
        if (step.Kind == XmpPathSegmentKind.ArrayLast)
            return parent.Children.LastOrDefault();
        if (step.Kind == XmpPathSegmentKind.ArrayIndex)
        {
            if (!int.TryParse(step.Name.AsSpan(1, step.Name.Length - 2), NumberStyles.None,
                    CultureInfo.InvariantCulture, out var itemIndex) || itemIndex < 1)
                throw new XmpException("Invalid array index", XmpError.BadPath);
            return itemIndex <= parent.Children.Count ? parent.Children[itemIndex - 1] : null;
        }

        var equals = step.Name.IndexOf('=');
        var qualifierSelector = step.Kind == XmpPathSegmentKind.QualifierSelector;
        var name = step.Name[(qualifierSelector ? 2 : 1)..equals];
        var quote = step.Name[equals + 1];
        var value = step.Name[(equals + 2)..^2].Replace(new string(quote, 2), quote.ToString(), StringComparison.Ordinal);
        if (qualifierSelector && name == "xml:lang")
            value = XmpPathFactory.NormalizeLanguage(value);
        // The reference skips the last array item for non-language qualifier selectors.
        var count = qualifierSelector && name != "xml:lang" ? parent.Children.Count - 1 : parent.Children.Count;
        for (var i = 0; i < count; i++)
        {
            var item = parent.Children[i];
            XmpNode? selected;
            if (qualifierSelector)
                selected = FindQualifier(registry, item, name);
            else
            {
                if (item.Kind != XmpNodeKind.Structure)
                    throw new XmpException("Field selector must be used on array of structures", XmpError.BadPath);
                selected = FindNamedNode(registry, item.Children, name);
            }
            if (selected?.Value == value) return item;
        }
        return null;
    }

    private static XmpNode? FindQualifier(XmpSchemaRegistry registry, XmpNode parent, string name)
    {
        if (name == "xml:lang")
            return parent.Language is null ? null : new XmpNode(Xml.NamespaceName, "lang", XmpNodeKind.Literal, parent.Language, null, []);
        return FindNamedNode(registry, parent.Qualifiers, name);
    }

    private static XmpNode? FindNamedNode(XmpSchemaRegistry registry, IReadOnlyList<XmpNode> nodes, string name)
    {
        var colon = name.IndexOf(':');
        if (colon <= 0) return null;
        var uri = registry.GetNamespaceUri(name[..colon]);
        return uri is null ? null : nodes.FirstOrDefault(node => node.NamespaceUri == uri && node.Name == name[(colon + 1)..]);
    }

    /// <summary>Reads an XML packet from a stream without closing it.</summary>
    public static XmpPacket Parse(Stream input)
        => ParseXml(input, false, false);

    /// <summary>
    /// Searches an XML document for an RDF root using the reference's discovery rules.
    /// Returns empty metadata when no root is found; normalization and XML recovery are not performed.
    /// </summary>
    public static XmpPacket ParseDiscovered(Stream input, bool requireXmpMeta = false)
        => ParseXml(input, true, requireXmpMeta);

    /// <summary>
    /// Searches an XML stream, optionally retrying malformed XML with the selected recovery.
    /// Recovery buffers the remaining input; the caller retains ownership of the stream.
    /// </summary>
    public static XmpPacket ParseDiscovered(Stream input, XmpRecoveryOptions recovery, bool requireXmpMeta = false)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(recovery);
        if (!recovery.FixControlCharacters && !recovery.AcceptLatin1)
            return ParseXml(input, true, requireXmpMeta);
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        var bytes = buffer.ToArray();
        XDocument document;
        try
        {
            document = ReadDocument(bytes);
        }
        catch (XmlException)
        {
            var encoding = DetectEncoding(bytes);
            if (recovery.AcceptLatin1 && encoding == Encoding.UTF8)
                bytes = RepairLatin1(bytes);
            try
            {
                if (recovery.FixControlCharacters)
                {
                    using var source = new MemoryStream(bytes, false);
                    using var text = new StreamReader(source, encoding, true);
                    var contents = text.ReadToEnd();
                    // Java's UTF-8 InputStreamReader leaves the BOM in its character stream.
                    if (encoding == Encoding.UTF8 && bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble))
                        contents = "\ufeff" + contents;
                    document = ReadDocument(RepairControls(contents));
                }
                else
                    document = ReadDocument(bytes);
            }
            catch (XmlException exception)
            {
                throw new XmpException("XML parsing failure", XmpError.BadXml, exception);
            }
        }
        return DiscoverDocument(document, requireXmpMeta);
    }

    /// <summary>
    /// Searches an XML string for an RDF root. When requested, retries malformed XML after
    /// replacing disallowed ASCII controls and control-character references, as in xmp-core.
    /// </summary>
    public static XmpPacket ParseDiscovered(string input, bool requireXmpMeta = false, bool fixControlCharacters = false)
    {
        ArgumentNullException.ThrowIfNull(input);
        XDocument document;
        try
        {
            document = ReadDocument(input);
        }
        catch (XmlException exception)
        {
            if (!fixControlCharacters)
                throw new XmpException("XML parsing failure", XmpError.BadXml, exception);
            try
            {
                document = ReadDocument(RepairControls(input));
            }
            catch (XmlException retryException)
            {
                throw new XmpException("XML parsing failure", XmpError.BadXml, retryException);
            }
        }
        return DiscoverDocument(document, requireXmpMeta);
    }

    private static XDocument ReadDocument(string input)
    {
        using var source = new StringReader(input);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    }

    private static XDocument ReadDocument(byte[] input)
    {
        using var source = new MemoryStream(input, false);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length < 2) return Encoding.UTF8;
        if (bytes[0] == 0)
        {
            if (bytes.Length < 4 || bytes[1] != 0) return Encoding.BigEndianUnicode;
            return new UTF32Encoding(true, true);
        }
        if (bytes[0] < 0x80)
        {
            if (bytes[1] != 0) return Encoding.UTF8;
            return bytes.Length < 4 || bytes[2] != 0 ? Encoding.Unicode : Encoding.UTF32;
        }
        if (bytes[0] == 0xef) return Encoding.UTF8;
        if (bytes[0] == 0xfe) return Encoding.BigEndianUnicode;
        return bytes.Length < 4 || bytes[2] != 0 ? Encoding.Unicode : Encoding.UTF32;
    }

    private static byte[] RepairLatin1(byte[] input)
    {
        using var output = new MemoryStream();
        for (var index = 0; index < input.Length;)
        {
            var first = input[index];
            if (first < 0x7f)
            {
                output.WriteByte(first);
                index++;
                continue;
            }
            if (first >= 0xc0)
            {
                var leading = 0;
                for (var bit = 0x80; (first & bit) != 0; bit >>= 1) leading++;
                var length = 1;
                while (length < leading && index + length < input.Length &&
                       (input[index + length] & 0xc0) == 0x80) length++;
                if (length == leading)
                {
                    output.Write(input, index, length);
                    index += length;
                    continue;
                }
            }
            var character = first is >= 0x80 and <= 0x9f ? Windows1252[first - 0x80] : (char)first;
            var encoded = Encoding.UTF8.GetBytes([character]);
            output.Write(encoded);
            index++;
        }
        return output.ToArray();
    }

    private const string Windows1252 = "\u20ac \u201a\u0192\u201e\u2026\u2020\u2021\u02c6\u2030\u0160\u2039\u0152 \u017d " +
                                       " \u2018\u2019\u201c\u201d\u2022\u2013\u2014\u02dc\u2122\u0161\u203a\u0153 \u017e\u0178";

    private static XmpPacket ParseXml(Stream input, bool discover, bool requireXmpMeta)
    {
        ArgumentNullException.ThrowIfNull(input);
        try
        {
            using var reader = XmlReader.Create(input, CreateReaderSettings());
            var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            return discover ? DiscoverDocument(document, requireXmpMeta) : ParseDocument(document);
        }
        catch (XmlException exception)
        {
            throw new XmpException("XML parsing failure", XmpError.BadXml, exception);
        }
    }

    private static XmlReaderSettings CreateReaderSettings() => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        CloseInput = false
    };

    private static XmpPacket DiscoverDocument(XDocument document, bool requireXmpMeta)
    {
        string? header = null;
        var rdf = FindRdf(document, requireXmpMeta, ref header);
        return rdf is null ? new XmpPacket(null, []) : ParseRdf(rdf, header);
    }

    private static string RepairControls(string input)
    {
        var result = new StringBuilder(input.Length);
        for (var index = 0; index < input.Length; index++)
        {
            var character = input[index];
            if (character == '&' && index + 2 < input.Length && input[index + 1] == '#')
            {
                var position = index + 2;
                var hex = input[position] == 'x';
                if (hex) position++;
                var digits = 0;
                var code = 0;
                while (position < input.Length)
                {
                    var c = input[position];
                    var digit = c is >= '0' and <= '9' ? c - '0'
                        : hex && c is >= 'a' and <= 'f' ? c - 'a' + 10
                        : hex && c is >= 'A' and <= 'F' ? c - 'A' + 10 : -1;
                    if (digit < 0 || ++digits > (hex ? 4 : 5)) break;
                    code = code * (hex ? 16 : 10) + digit;
                    position++;
                }
                if ((hex || digits > 0) && position < input.Length && input[position] == ';' && IsControl((char)code))
                {
                    result.Append(' ');
                    index = position;
                    continue;
                }
            }
            result.Append(IsControl(character) ? ' ' : character);
        }
        return result.ToString();
    }

    private static bool IsControl(char value) =>
        (value <= '\u001f' || value == '\u007f') && value is not ('\t' or '\n' or '\r');

    private static XElement? FindRdf(XContainer root, bool requireXmpMeta, ref string? header)
    {
        var frames = new Stack<(IEnumerator<XNode> Nodes, bool RequireXmpMeta)>();
        frames.Push((root.Nodes().GetEnumerator(), requireXmpMeta));
        try
        {
            while (frames.Count > 0)
            {
                var frame = frames.Peek();
                if (!frame.Nodes.MoveNext())
                {
                    frames.Pop().Nodes.Dispose();
                    continue;
                }
                var node = frame.Nodes.Current;
                if (node is XProcessingInstruction instruction && instruction.Target == "xpacket")
                    header = instruction.Data;
                else if (node is XElement element)
                {
                    if (element.Name == Xmp + "xmpmeta" || element.Name == Xmp + "xapmeta")
                    {
                        frames.Pop().Nodes.Dispose();
                        frames.Push((element.Nodes().GetEnumerator(), false));
                    }
                    else if (!frame.RequireXmpMeta && element.Name == Rdf + "RDF")
                        return element;
                    else
                        frames.Push((element.Nodes().GetEnumerator(), frame.RequireXmpMeta));
                }
            }
            return null;
        }
        finally
        {
            while (frames.Count > 0) frames.Pop().Nodes.Dispose();
        }
    }

    private static XmpPacket ParseDocument(XDocument document)
    {
        var root = document.Root;
        if (root is null) throw BadRdf("Missing document element.");
        XElement rdf;
        if (root.Name == Rdf + "RDF") rdf = root;
        else if (root.Name == Xmp + "xmpmeta" || root.Name == Xmp + "xapmeta")
        {
            var children = root.Elements().ToArray();
            if (children.Length != 1 || children[0].Name != Rdf + "RDF")
                throw Unsupported("An XMP wrapper must contain exactly one rdf:RDF element.");
            EnsureWhitespace(root);
            rdf = children[0];
        }
        else throw Unsupported("Only an rdf:RDF root or x:xmpmeta wrapper is supported.");

        return ParseRdf(rdf, null);
    }

    private static XmpPacket ParseRdf(XElement rdf, string? packetHeader)
    {
        CheckAttributes(rdf);
        EnsureWhitespace(rdf);
        var properties = new Dictionary<XName, XmpNode>();
        string? about = null;
        foreach (var description in rdf.Elements())
        {
            if (description.Name != Rdf + "Description")
                throw Unsupported("Only top-level rdf:Description nodes are supported.");
            ValidateNodeIdentity(description);
            var currentAbout = (string?)description.Attribute(Rdf + "about");
            if (!string.IsNullOrEmpty(currentAbout))
            {
                if (!string.IsNullOrEmpty(about) && about != currentAbout)
                    throw new XmpException("Mismatched top level rdf:about values", XmpError.BadXmp);
                about = currentAbout;
            }
            else if (about is null) about = currentAbout;
            EnsureWhitespace(description);
            foreach (var attribute in description.Attributes().Where(a => !a.IsNamespaceDeclaration))
            {
                if (IsNodeIdentity(attribute.Name)) continue;
                if (attribute.Name.NamespaceName.Length == 0 || attribute.Name.Namespace == Rdf || attribute.Name.Namespace == Xml)
                    throw Unsupported($"Unsupported description attribute '{attribute.Name}'.");
                AddUnique(properties, CanonicalName(attribute.Name), AttributeNode(attribute));
            }
            foreach (var element in description.Elements())
            {
                RequirePropertyName(element.Name);
                AddUnique(properties, CanonicalName(element.Name), ParseProperty(element, false));
            }
        }
        return new XmpPacket(about, properties, packetHeader);
    }

    private static XmpNode ParseProperty(XElement element, bool arrayItem)
    {
        if (arrayItem && element.Name != Rdf + "li")
            throw BadRdf("Array children must be rdf:li elements.");
        string? language = null;
        string? uri = null;
        string? value = null;
        string? parseType = null;
        var attributes = new Dictionary<XName, XmpNode>();
        var nodeId = false;
        var datatype = false;
        foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            if (attribute.Name == Xml + "lang") language = XmpPathFactory.NormalizeLanguage(attribute.Value);
            else if (attribute.Name == Rdf + "resource") uri = attribute.Value;
            else if (attribute.Name == Rdf + "parseType") parseType = attribute.Value;
            else if (attribute.Name == Rdf + "value") value = attribute.Value;
            else if (attribute.Name == Rdf + "ID") { }
            else if (attribute.Name == Rdf + "nodeID") nodeId = true;
            else if (attribute.Name == Rdf + "datatype") datatype = true;
            else if (attribute.Name.NamespaceName.Length != 0 && attribute.Name.Namespace != Rdf && attribute.Name.Namespace != Xml)
                AddUnique(attributes, CanonicalName(attribute.Name), AttributeNode(attribute));
            else throw Unsupported($"Unsupported property attribute '{attribute.Name}'.");
        }
        var name = arrayItem ? "" : element.Name.LocalName;
        var ns = arrayItem ? "" : CanonicalName(element.Name).NamespaceName;
        var prefix = arrayItem ? null : element.GetPrefixOfNamespace(element.Name.Namespace);
        if (uri is not null && value is not null)
            throw new XmpException("Empty property element can't have both rdf:value and rdf:resource", XmpError.BadXmp);
        if (uri is not null && nodeId) throw BadRdf("Empty property element can't have both rdf:resource and rdf:nodeID");
        if (parseType is not null)
        {
            if (parseType != "Resource") throw new XmpException("Unsupported rdf:parseType", XmpError.BadXmp);
            if (uri is not null || value is not null || nodeId || datatype || attributes.Count != 0)
                throw BadRdf("Invalid attribute for ParseTypeResource property element");
            EnsureWhitespace(element);
            return ParseStructure(element, ns, name, language, true, null, prefix);
        }
        if (uri is not null || value is not null || nodeId || attributes.Count != 0)
        {
            if (datatype || element.Nodes().Any())
                throw BadRdf("Nested content not allowed with rdf:resource or property attributes");
            if (uri is not null || value is not null)
                return new(ns, name, uri is null ? XmpNodeKind.Literal : XmpNodeKind.Uri, uri ?? value,
                    language, [], attributes.Values, prefix);
            if (attributes.Count != 0)
                return new(ns, name, XmpNodeKind.Structure, null, language, attributes.Values,
                    prefix: prefix);
            return new(ns, name, XmpNodeKind.Literal, "", language, [], prefix: prefix);
        }
        var childElements = element.Elements().ToArray();
        if (childElements.Length == 0)
        {
            if (element.Nodes().Any(node => node is not XText || node is XCData))
                throw Unsupported("Only text content is supported in literal properties.");
            return new(ns, name, XmpNodeKind.Literal, element.Value, language, [], prefix: prefix);
        }
        EnsureWhitespace(element);
        if (datatype) throw BadRdf("Invalid attribute for resource property element");
        if (childElements.Length != 1) throw BadRdf("A resource property must have one child.");
        var container = childElements[0];
        EnsureWhitespace(container);
        var kind = container.Name == Rdf + "Bag" ? XmpNodeKind.UnorderedArray
            : container.Name == Rdf + "Seq" ? XmpNodeKind.OrderedArray
            : container.Name == Rdf + "Alt" ? XmpNodeKind.AlternativeArray
            : container.Name == Rdf + "Description" || (container.Name.NamespaceName.Length != 0 && container.Name.Namespace != Rdf)
                ? XmpNodeKind.Structure
            : throw Unsupported($"Unsupported RDF resource node '{container.Name}'.");
        if (kind == XmpNodeKind.Structure)
        {
            var typeName = container.Name != Rdf + "Description" && container.Name.LocalName != "Description"
                ? container.Name.NamespaceName + ":" + container.Name.LocalName : null;
            return ParseStructure(container, ns, name, language, false, typeName, prefix);
        }
        ValidateNodeIdentity(container);
        foreach (var attribute in container.Attributes().Where(a => !a.IsNamespaceDeclaration))
            if (!IsNodeIdentity(attribute.Name))
                throw Unsupported($"Unsupported attribute '{attribute.Name}'.");
        var children = container.Elements().Select(item => ParseProperty(item, true)).ToArray();
        return new(ns, name, kind, null, language, children, prefix: prefix);
    }

    private static XmpNode ParseStructure(XElement element, string ns, string name, string? language,
        bool parseTypeResource, string? typeName, string? prefix)
    {
        var fields = new Dictionary<XName, XmpNode>();
        if (!parseTypeResource) ValidateNodeIdentity(element);
        foreach (var attribute in element.Attributes().Where(a => !a.IsNamespaceDeclaration))
        {
            if (parseTypeResource && (attribute.Name == Rdf + "parseType" || attribute.Name == Xml + "lang")) continue;
            if (IsNodeIdentity(attribute.Name))
            {
                if (parseTypeResource && attribute.Name != Rdf + "ID") throw BadRdf("Invalid structure attribute");
                continue;
            }
            RequirePropertyName(attribute.Name);
            AddUnique(fields, CanonicalName(attribute.Name), AttributeNode(attribute));
        }
        foreach (var field in element.Elements())
        {
            if (field.Name != Rdf + "value") RequirePropertyName(field.Name);
            AddUnique(fields, CanonicalName(field.Name), ParseProperty(field, false));
        }
        var qualifiers = new Dictionary<XName, XmpNode>();
        if (typeName is not null)
            AddUnique(qualifiers, Rdf + "type", new XmpNode(Rdf.NamespaceName, "type", XmpNodeKind.Literal, typeName, null, []));
        if (!fields.TryGetValue(Rdf + "value", out var valueNode))
            return new(ns, name, XmpNodeKind.Structure, null, language, fields.Values,
                qualifiers.Values, prefix);

        if (language is not null && valueNode.Language is not null)
            throw new XmpException("Redundant xml:lang for rdf:value element", XmpError.BadXmp);
        foreach (var qualifier in valueNode.Qualifiers)
            AddUnique(qualifiers, XName.Get(qualifier.Name, qualifier.NamespaceUri), qualifier);
        foreach (var field in fields.Where(field => field.Key != Rdf + "value"))
            AddUnique(qualifiers, field.Key, field.Value);
        return new(ns, name, valueNode.Kind, valueNode.Value, language ?? valueNode.Language,
            valueNode.Children, qualifiers.Values, prefix);
    }

    private static XmpNode AttributeNode(XAttribute attribute) =>
        new(CanonicalName(attribute.Name).NamespaceName, attribute.Name.LocalName, XmpNodeKind.Literal, attribute.Value, null, []);

    private static bool IsNodeIdentity(XName name) =>
        name == Rdf + "about" || name == Rdf + "ID" || name == Rdf + "nodeID";

    private static void ValidateNodeIdentity(XElement element)
    {
        if (element.Attributes().Count(attribute => IsNodeIdentity(attribute.Name)) > 1)
            throw BadRdf("Mutually exclusive about, ID, nodeID attributes");
    }

    private static XName CanonicalName(XName name) =>
        name.NamespaceName == DeprecatedDc ? XName.Get(name.LocalName, Dc) : name;
    private static void AddUnique(Dictionary<XName, XmpNode> nodes, XName name, XmpNode node)
    {
        if (!nodes.TryAdd(name, node)) throw new XmpException($"Duplicate property '{name}'.", XmpError.BadXmp);
    }

    private static void RequirePropertyName(XName name)
    {
        if (name.NamespaceName.Length == 0 || name.Namespace == Rdf || name.Namespace == Xml)
            throw Unsupported($"Unsupported property name '{name}'.");
    }

    private static void CheckAttributes(XElement element)
    {
        var attribute = element.Attributes().FirstOrDefault(a => !a.IsNamespaceDeclaration);
        if (attribute is not null) throw Unsupported($"Unsupported attribute '{attribute.Name}'.");
    }

    private static void EnsureWhitespace(XElement element)
    {
        foreach (var node in element.Nodes())
            if (node is not XElement && (node is not XText text || node is XCData || !string.IsNullOrWhiteSpace(text.Value)))
                throw Unsupported("Mixed content and non-text XML nodes are not supported.");
    }

    private static XmpException BadRdf(string message) => new(message, XmpError.BadRdf);
    private static NotSupportedException Unsupported(string message) => new(message);
}
