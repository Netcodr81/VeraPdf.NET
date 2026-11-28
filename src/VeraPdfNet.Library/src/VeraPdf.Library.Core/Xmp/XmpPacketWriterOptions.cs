// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
namespace VeraPdf.Library.Xmp;

/// <summary>Character encoding used for the serialized XMP packet, without a byte-order mark.</summary>
public enum XmpPacketEncoding
{
    Utf8,
    Utf16BigEndian,
    Utf16LittleEndian
}

/// <summary>Options for the raw RDF/XML packet writer.</summary>
public sealed class XmpPacketWriterOptions
{
    /// <summary>Output encoding. The default is UTF-8.</summary>
    public XmpPacketEncoding Encoding { get; init; }

    /// <summary>Add xpacket processing instructions around the XML document.</summary>
    public bool IncludePacketWrapper { get; init; }

    /// <summary>Mark a wrapped packet read-only. Read-only packets cannot have padding.</summary>
    public bool ReadOnlyPacket { get; init; }

    /// <summary>Write an rdf:RDF root instead of an x:xmpmeta element.</summary>
    public bool OmitXmpMetaElement { get; init; }

    /// <summary>Sort schemas, properties, structure fields, and non-special qualifiers without reordering arrays.</summary>
    public bool Sort { get; init; }

    /// <summary>Indent RDF/XML elements for readability. Disabled by default.</summary>
    public bool Indent { get; init; }

    /// <summary>Whitespace used for each indentation level when indentation is enabled.</summary>
    public string IndentChars { get; init; } = "  ";

    /// <summary>XML whitespace used for formatting line breaks. The default is LF.</summary>
    public string NewLineChars { get; init; } = "\n";

    /// <summary>Number of padding bytes before the packet trailer. UTF-16 requires an even number.</summary>
    public int PaddingBytes { get; init; }

    /// <summary>
    /// Adds room for a thumbnail if the packet has no xmp:Thumbnails property.
    /// Requires a writable wrapper and cannot be combined with an exact packet length.
    /// </summary>
    public bool IncludeThumbnailPadding { get; init; }

    /// <summary>Total packet length in bytes, including processing instructions and padding. UTF-16 requires an even number.</summary>
    public int? ExactPacketLength { get; init; }
}
