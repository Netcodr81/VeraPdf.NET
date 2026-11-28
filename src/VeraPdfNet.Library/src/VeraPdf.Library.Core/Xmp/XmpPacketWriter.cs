// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Text;
using System.Xml;

namespace VeraPdf.Library.Xmp;

/// <summary>Writes the supported raw XMP tree as RDF/XML.</summary>
public static class XmpPacketWriter
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xml = "http://www.w3.org/XML/1998/namespace";
    private const string Xmp = "adobe:ns:meta/";
    private const string XmpSchema = "http://ns.adobe.com/xap/1.0/";
    private const string PacketHeader = "<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>";
    private const string WritableTrailer = "<?xpacket end=\"w\"?>";
    private const string ReadOnlyTrailer = "<?xpacket end=\"r\"?>";

    /// <summary>
    /// Writes an XMP wrapper and RDF document without closing the output stream.
    /// No packet processing instructions are emitted by default.
    /// </summary>
    public static void Write(XmpPacket packet, Stream output) => Write(packet, output, new XmpPacketWriterOptions());

    /// <summary>Writes a raw RDF/XML packet with the specified wrapper and padding options.</summary>
    public static void Write(XmpPacket packet, Stream output, XmpPacketWriterOptions options)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);
        Encoding encoding = options.Encoding switch
        {
            XmpPacketEncoding.Utf8 => new UTF8Encoding(false),
            XmpPacketEncoding.Utf16BigEndian => new UnicodeEncoding(true, false),
            XmpPacketEncoding.Utf16LittleEndian => new UnicodeEncoding(false, false),
            _ => throw new XmpException("Unsupported packet encoding", XmpError.BadOptions)
        };
        var prefixes = CollectNamespaces(packet, options.Sort);
        if (!options.IncludePacketWrapper)
        {
            WriteXml(packet, output, prefixes, options, encoding);
            return;
        }

        using var body = new MemoryStream();
        WriteXml(packet, body, prefixes, options, encoding);
        var newline = encoding.GetBytes(options.NewLineChars);
        var header = encoding.GetBytes(PacketHeader + options.NewLineChars);
        var trailer = encoding.GetBytes(options.ReadOnlyPacket ? ReadOnlyTrailer : WritableTrailer);
        var minimumLength = header.Length + body.Length + newline.Length + trailer.Length;
        var padding = options.ExactPacketLength is int length ? length - minimumLength : (long)options.PaddingBytes;
        if (options.IncludeThumbnailPadding && packet.GetProperty(XmpSchema, "Thumbnails") is null)
            padding += options.Encoding == XmpPacketEncoding.Utf8 ? 10000 : 20000;
        if (padding < 0)
            throw new XmpException("Can't fit into specified packet size", XmpError.BadSerialize);

        output.Write(header);
        body.Position = 0;
        body.CopyTo(output);
        output.Write(newline);
        var spaces = encoding.GetBytes(new string(' ', 2048));
        while (padding > 0)
        {
            var count = (int)Math.Min(padding, spaces.Length);
            output.Write(spaces, 0, count);
            padding -= count;
        }
        output.Write(trailer);
    }

    private static void ValidateOptions(XmpPacketWriterOptions options)
    {
        if (options.PaddingBytes < 0 || options.ExactPacketLength < 0)
            throw new XmpException("Packet length and padding must not be negative", XmpError.BadOptions);
        if (options.IndentChars is null || options.NewLineChars is null ||
            options.IndentChars.Any(character => character is not (' ' or '\t')) ||
            options.NewLineChars.Any(character => character is not (' ' or '\t' or '\r' or '\n')))
            throw new XmpException("Indentation and newline must contain only XML whitespace", XmpError.BadOptions);
        if (options.Encoding is not (XmpPacketEncoding.Utf8 or XmpPacketEncoding.Utf16BigEndian or XmpPacketEncoding.Utf16LittleEndian))
            throw new XmpException("Unsupported packet encoding", XmpError.BadOptions);
        if (options.Encoding != XmpPacketEncoding.Utf8 &&
            (options.PaddingBytes % 2 != 0 || options.ExactPacketLength is int length && length % 2 != 0))
            throw new XmpException("UTF-16 packet lengths and padding must be even", XmpError.BadOptions);
        if ((!options.IncludePacketWrapper && (options.ReadOnlyPacket || options.PaddingBytes != 0 || options.ExactPacketLength is not null || options.IncludeThumbnailPadding))
            || (options.ReadOnlyPacket && (options.PaddingBytes != 0 || options.ExactPacketLength is not null || options.IncludeThumbnailPadding))
            || (options.ExactPacketLength is not null && (options.PaddingBytes != 0 || options.IncludeThumbnailPadding)))
            throw new XmpException("Inconsistent packet serialization options", XmpError.BadOptions);
    }

    private static void WriteXml(XmpPacket packet, Stream output, IReadOnlyDictionary<string, string> prefixes, XmpPacketWriterOptions options, Encoding encoding)
    {
        using var writer = XmlWriter.Create(output, new XmlWriterSettings
        {
            Encoding = encoding,
            CloseOutput = false,
            NewLineHandling = NewLineHandling.Entitize,
            Indent = options.Indent,
            IndentChars = options.IndentChars,
            NewLineChars = options.NewLineChars,
            OmitXmlDeclaration = true
        });
        if (!options.OmitXmpMetaElement) writer.WriteStartElement("x", "xmpmeta", Xmp);
        writer.WriteStartElement("rdf", "RDF", Rdf);
        foreach (var (uri, prefix) in prefixes)
            writer.WriteAttributeString("xmlns", prefix, null, uri);
        writer.WriteStartElement("rdf", "Description", Rdf);
        if (packet.About is not null) writer.WriteAttributeString("rdf", "about", Rdf, packet.About);
        var properties = options.Sort
            ? packet.Properties.OrderBy(node => node.NamespaceUri, StringComparer.Ordinal)
                .ThenBy(node => QualifiedName(node, prefixes), StringComparer.Ordinal)
            : packet.Properties.AsEnumerable();
        foreach (var node in properties) WriteNode(writer, node, prefixes, false, options.Sort);
        writer.WriteEndElement();
        writer.WriteEndElement();
        if (!options.OmitXmpMetaElement) writer.WriteEndElement();
    }

    private static Dictionary<string, string> CollectNamespaces(XmpPacket packet, bool sort)
    {
        var prefixes = new Dictionary<string, string>(StringComparer.Ordinal);
        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        var namespaceOrder = new List<string>();
        var stack = new Stack<(XmpNode Node, bool IsQualifier)>(
            packet.Properties.Reverse().Select(node => (node, false)));
        while (stack.Count > 0)
        {
            var (node, isQualifier) = stack.Pop();
            if (isQualifier && node.NamespaceUri == Rdf && node.Name == "type")
            {
                var (typeNamespace, _) = GetTypeName(node);
                AddNamespace(typeNamespace);
                continue;
            }
            if (node.NamespaceUri is Rdf or Xml)
                throw new NotSupportedException("RDF and XML named qualifiers cannot be serialized by the raw XMP writer.");
            if (node.NamespaceUri.Length != 0) AddNamespace(node.NamespaceUri);
            for (var i = node.Qualifiers.Count - 1; i >= 0; i--) stack.Push((node.Qualifiers[i], true));
            for (var i = node.Children.Count - 1; i >= 0; i--) stack.Push((node.Children[i], false));
        }
        var orderedNamespaces = sort ? namespaceOrder.OrderBy(uri => uri, StringComparer.Ordinal) : namespaceOrder.AsEnumerable();
        foreach (var uri in orderedNamespaces)
            prefixes.Add(uri, "ns" + (prefixes.Count + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
        return prefixes;

        void AddNamespace(string uri)
        {
            if (namespaces.Add(uri)) namespaceOrder.Add(uri);
        }
    }

    private static string QualifiedName(XmpNode node, IReadOnlyDictionary<string, string> prefixes) =>
        prefixes[node.NamespaceUri] + ":" + node.Name;

    private static IEnumerable<XmpNode> OrderedNodes(IEnumerable<XmpNode> nodes,
        IReadOnlyDictionary<string, string> prefixes, bool sort) =>
        sort ? nodes.OrderBy(node => QualifiedName(node, prefixes), StringComparer.Ordinal) : nodes;

    private static void WriteNode(XmlWriter writer, XmpNode node, IReadOnlyDictionary<string, string> prefixes,
        bool arrayItem, bool sort)
    {
        if (arrayItem) writer.WriteStartElement("rdf", "li", Rdf);
        else writer.WriteStartElement(prefixes[node.NamespaceUri], node.Name, node.NamespaceUri);
        var type = node.Qualifiers.FirstOrDefault(q => q.NamespaceUri == Rdf && q.Name == "type");
        if (type is not null)
        {
            var (typeNamespace, localName) = GetTypeName(type);
            writer.WriteStartElement(prefixes[typeNamespace], localName, typeNamespace);
            writer.WriteStartElement("rdf", "value", Rdf);
            WriteValue(writer, node, prefixes, sort);
            writer.WriteEndElement();
            foreach (var qualifier in OrderedNodes(node.Qualifiers.Where(q => !ReferenceEquals(q, type)), prefixes, sort))
                WriteNode(writer, qualifier, prefixes, false, sort);
            writer.WriteEndElement();
        }
        else if (node.Qualifiers.Count > 0)
        {
            writer.WriteAttributeString("rdf", "parseType", Rdf, "Resource");
            writer.WriteStartElement("rdf", "value", Rdf);
            WriteValue(writer, node, prefixes, sort);
            writer.WriteEndElement();
            foreach (var qualifier in OrderedNodes(node.Qualifiers, prefixes, sort))
                WriteNode(writer, qualifier, prefixes, false, sort);
        }
        else WriteValue(writer, node, prefixes, sort);
        writer.WriteEndElement();
    }

    private static (string NamespaceUri, string LocalName) GetTypeName(XmpNode type)
    {
        var value = type.Value;
        var separator = value?.LastIndexOf(':') ?? -1;
        if (type.Kind != XmpNodeKind.Literal || type.Language is not null || type.Children.Count != 0 ||
            type.Qualifiers.Count != 0 || separator <= 0 || separator == value!.Length - 1)
            throw new NotSupportedException("Unsupported RDF type qualifier.");
        var ns = value[..separator];
        var localName = value[(separator + 1)..];
        try { XmlConvert.VerifyNCName(localName); }
        catch (XmlException exception) { throw new NotSupportedException("Unsupported RDF type qualifier.", exception); }
        if (ns is Rdf or Xml) throw new NotSupportedException("Unsupported RDF type namespace.");
        return (ns, localName);
    }

    private static void WriteValue(XmlWriter writer, XmpNode node, IReadOnlyDictionary<string, string> prefixes, bool sort)
    {
        if (node.Language is not null) writer.WriteAttributeString("xml", "lang", Xml, node.Language);
        switch (node.Kind)
        {
            case XmpNodeKind.Literal:
                writer.WriteString(node.Value ?? "");
                break;
            case XmpNodeKind.Uri:
                writer.WriteAttributeString("rdf", "resource", Rdf, node.Value ?? "");
                break;
            case XmpNodeKind.Structure:
                writer.WriteAttributeString("rdf", "parseType", Rdf, "Resource");
                foreach (var child in OrderedNodes(node.Children, prefixes, sort)) WriteNode(writer, child, prefixes, false, sort);
                break;
            case XmpNodeKind.UnorderedArray:
            case XmpNodeKind.OrderedArray:
            case XmpNodeKind.AlternativeArray:
                var name = node.Kind == XmpNodeKind.UnorderedArray ? "Bag" :
                    node.Kind == XmpNodeKind.OrderedArray ? "Seq" : "Alt";
                writer.WriteStartElement("rdf", name, Rdf);
                foreach (var child in node.Children) WriteNode(writer, child, prefixes, true, sort);
                writer.WriteEndElement();
                break;
            default:
                throw new NotSupportedException($"Unsupported XMP node kind: {node.Kind}.");
        }
    }
}
