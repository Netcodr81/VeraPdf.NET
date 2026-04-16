namespace VeraPdf.NET.Model;

/// <summary>
/// Parent type for all basic PDF COS objects.
/// </summary>
public abstract class CosObject : PDFObject
{
}

/// <summary>
/// Low-level PDF document object.
/// </summary>
public class CosDocument : CosObject
{
    /// <summary>
    /// Number of indirect objects in the document.
    /// </summary>
    public int NrIndirects { get; set; }

    /// <summary>
    /// Version of the document.
    /// </summary>
    public decimal HeaderVersion { get; set; }

    /// <summary>
    /// True if the OCProperties key is present in the document catalog.
    /// </summary>
    public bool IsOptionalContentPresent { get; set; }

    /// <summary>
    /// Size of data after EOF and optional end-of-line marker.
    /// </summary>
    public int PostEOFDataSize { get; set; }

    /// <summary>
    /// Identifier of the first page trailer for linearized documents.
    /// </summary>
    public string? FirstPageID { get; set; }

    /// <summary>
    /// Identifier of the last trailer in the document.
    /// </summary>
    public string? LastID { get; set; }

    /// <summary>
    /// String representation of the first page trailer identifier.
    /// </summary>
    public string? FirstPageIDValue { get; set; }

    /// <summary>
    /// String representation of the last trailer identifier.
    /// </summary>
    public string? LastIDValue { get; set; }

    /// <summary>
    /// True if the document is linearized.
    /// </summary>
    public bool IsLinearized { get; set; }

    /// <summary>
    /// Embedded file specifications.
    /// </summary>
    public IReadOnlyList<CosFileSpecification> EmbeddedFiles { get; set; } = [];

    /// <summary>
    /// Cross-reference table.
    /// </summary>
    public CosXRef? XRef { get; set; }

    /// <summary>
    /// Trailer dictionary.
    /// </summary>
    public CosTrailer? Trailer { get; set; }

    /// <summary>
    /// Indirect objects referred from the xref table.
    /// </summary>
    public IReadOnlyList<CosIndirect> IndirectObjects { get; set; } = [];

    /// <summary>
    /// Offset of the PDF document header.
    /// </summary>
    public int HeaderOffset { get; set; }

    /// <summary>
    /// Content of the PDF header.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// First byte in the comment following the PDF header.
    /// </summary>
    public int HeaderByte1 { get; set; }

    /// <summary>
    /// Second byte in the comment following the PDF header.
    /// </summary>
    public int HeaderByte2 { get; set; }

    /// <summary>
    /// Third byte in the comment following the PDF header.
    /// </summary>
    public int HeaderByte3 { get; set; }

    /// <summary>
    /// Fourth byte in the comment following the PDF header.
    /// </summary>
    public int HeaderByte4 { get; set; }

    /// <summary>
    /// High-level PDF document structure.
    /// </summary>
    public PDDocument? Document { get; set; }

    /// <summary>
    /// Value of the /Marked entry in the MarkInfo dictionary.
    /// </summary>
    public bool Marked { get; set; }

    /// <summary>
    /// String representation of the /Requirements key value.
    /// </summary>
    public string? Requirements { get; set; }

    /// <summary>
    /// NeedsRendering entry in the document catalog.
    /// </summary>
    public bool NeedsRendering { get; set; }

    /// <summary>
    /// True if the EmbeddedFiles entry is present in the names dictionary.
    /// </summary>
    public bool ContainsEmbeddedFiles { get; set; }

    /// <summary>
    /// Value of the /Suspects entry in the MarkInfo dictionary.
    /// </summary>
    public bool Suspects { get; set; }

    /// <summary>
    /// Value of the /DisplayDocTitle key in the ViewerPreferences dictionary.
    /// </summary>
    public bool DisplayDocTitle { get; set; }

    /// <summary>
    /// True if the trailer dictionary contains Info key.
    /// </summary>
    public bool ContainsInfo { get; set; }

    /// <summary>
    /// True if the document catalog contains PieceInfo entry.
    /// </summary>
    public bool ContainsPieceInfo { get; set; }

    /// <summary>
    /// String representation of the MarkInfo dictionary.
    /// </summary>
    public string? MarkInfo { get; set; }

    /// <summary>
    /// String representation of the ViewerPreferences dictionary.
    /// </summary>
    public string? ViewerPreferences { get; set; }
}

/// <summary>
/// PDF indirect object.
/// </summary>
public class CosIndirect : CosObject
{
    /// <summary>
    /// Direct contents of the indirect object.
    /// </summary>
    public CosObject? DirectObject { get; set; }

    /// <summary>
    /// True if spacing around object syntax complies with PDF/A.
    /// </summary>
    public bool SpacingCompliesPDFA { get; set; }
}

/// <summary>
/// PDF null object.
/// </summary>
public class CosNull : CosObject
{
}

/// <summary>
/// PDF boolean object.
/// </summary>
public class CosBool : CosObject
{
    /// <summary>
    /// Boolean value of this object.
    /// </summary>
    public bool Value { get; set; }
}

/// <summary>
/// Generic PDF number object.
/// </summary>
public class CosNumber : CosObject
{
    /// <summary>
    /// String representation of this object.
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// Truncated integer value.
    /// </summary>
    public int IntValue { get; set; }

    /// <summary>
    /// Original decimal value.
    /// </summary>
    public decimal RealValue { get; set; }
}

/// <summary>
/// PDF real number object.
/// </summary>
public class CosReal : CosNumber
{
}

/// <summary>
/// PDF integer object.
/// </summary>
public class CosInteger : CosNumber
{
}

/// <summary>
/// PDF name object.
/// </summary>
public class CosName : CosObject
{
    /// <summary>
    /// Byte sequence representation after escape processing.
    /// </summary>
    public string? InternalRepresentation { get; set; }
}

/// <summary>
/// Blend mode name object.
/// </summary>
public class CosBM : CosName
{
}

/// <summary>
/// ActualText string object.
/// </summary>
public class CosActualText : CosString
{
}

/// <summary>
/// Alt string object.
/// </summary>
public class CosAlt : CosString
{
}

/// <summary>
/// PDF name representing a UTF-8 value.
/// </summary>
public class CosUnicodeName : CosName
{
    /// <summary>
    /// Indicates whether the name is valid UTF-8.
    /// </summary>
    public bool IsValidUtf8 { get; set; }

    /// <summary>
    /// Unicode value of the name object.
    /// </summary>
    public string? UnicodeValue { get; set; }
}

/// <summary>
/// PDF name representing rendering intent.
/// </summary>
public class CosRenderingIntent : CosName
{
}

/// <summary>
/// PDF name representing a single stream filter.
/// </summary>
public class CosFilter : CosName
{
    /// <summary>
    /// String representation of the decode parameters behavior.
    /// </summary>
    public string? DecodeParms { get; set; }
}

/// <summary>
/// PDF name representing a single inline image filter.
/// </summary>
public class CosIIFilter : CosName
{
}

/// <summary>
/// PDF string object.
/// </summary>
public class CosString : CosObject
{
    /// <summary>
    /// Internal byte presentation of the decoded string.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// True if the string is stored in hexadecimal format.
    /// </summary>
    public bool IsHex { get; set; }

    /// <summary>
    /// True if all non-whitespace hex characters are valid hex digits.
    /// </summary>
    public bool ContainsOnlyHex { get; set; }

    /// <summary>
    /// Number of hex digits for hexadecimal strings.
    /// </summary>
    public int HexCount { get; set; }

    /// <summary>
    /// True if the string contains private-use-area codes.
    /// </summary>
    public bool ContainsPUA { get; set; }
}

/// <summary>
/// PDF text string object.
/// </summary>
public class CosTextString : CosString
{
    /// <summary>
    /// Unicode representation of the string.
    /// </summary>
    public string? UnicodeValue { get; set; }
}

/// <summary>
/// Language text string object.
/// </summary>
public class CosLang : CosTextString
{
}

/// <summary>
/// PDF dictionary object.
/// </summary>
public class CosDict : CosObject
{
    /// <summary>
    /// Number of key/value pairs in the dictionary.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Dictionary keys.
    /// </summary>
    public IReadOnlyList<CosName> Keys { get; set; } = [];

    /// <summary>
    /// Dictionary values.
    /// </summary>
    public IReadOnlyList<CosObject> Values { get; set; } = [];

    /// <summary>
    /// XMP metadata if present.
    /// </summary>
    public PDMetadata? Metadata { get; set; }

    /// <summary>
    /// Ampersand-separated list of dictionary keys.
    /// </summary>
    public string? KeysString { get; set; }
}

/// <summary>
/// PDF stream object.
/// </summary>
public class CosStream : CosDict
{
    /// <summary>
    /// Value of the Length key in the stream dictionary.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Real length of the stream.
    /// </summary>
    public int RealLength { get; set; }

    /// <summary>
    /// Value of the F key.
    /// </summary>
    public string? F { get; set; }

    /// <summary>
    /// Value of the FFilter key.
    /// </summary>
    public string? FFilter { get; set; }

    /// <summary>
    /// Value of the FDecodeParms key.
    /// </summary>
    public string? FDecodeParms { get; set; }

    /// <summary>
    /// True if spacing around stream keyword complies with PDF/A.
    /// </summary>
    public bool StreamKeywordCRLFCompliant { get; set; }

    /// <summary>
    /// True if spacing around endstream keyword complies with PDF/A.
    /// </summary>
    public bool EndstreamKeywordEOLCompliant { get; set; }

    /// <summary>
    /// Stream filters.
    /// </summary>
    public IReadOnlyList<CosFilter> Filters { get; set; } = [];
}

/// <summary>
/// File specification dictionary.
/// </summary>
public class CosFileSpecification : CosDict
{
    /// <summary>
    /// Embedded file stream.
    /// </summary>
    public EmbeddedFile? EF { get; set; }

    /// <summary>
    /// True if the EF key is present.
    /// </summary>
    public bool ContainsEF { get; set; }

    /// <summary>
    /// True if the Desc key is present.
    /// </summary>
    public bool ContainsDesc { get; set; }

    /// <summary>
    /// File specification string (F key).
    /// </summary>
    public string? F { get; set; }

    /// <summary>
    /// Unicode file specification (UF key).
    /// </summary>
    public string? UF { get; set; }

    /// <summary>
    /// AFRelationship key value.
    /// </summary>
    public string? AFRelationship { get; set; }

    /// <summary>
    /// True if this file specification is associated via /AF.
    /// </summary>
    public bool IsAssociatedFile { get; set; }

    /// <summary>
    /// True if this file specification is present in EmbeddedFiles name tree.
    /// </summary>
    public bool PresentInEmbeddedFiles { get; set; }
}

/// <summary>
/// Trailer dictionary.
/// </summary>
public class CosTrailer : CosDict
{
    /// <summary>
    /// True if the document is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Encryption dictionary.
    /// </summary>
    public PDEncryption? Encrypt { get; set; }

    /// <summary>
    /// Document information dictionary.
    /// </summary>
    public CosInfo? Info { get; set; }
}

/// <summary>
/// Information dictionary.
/// </summary>
public class CosInfo : CosDict
{
    /// <summary>
    /// ModDate entry value.
    /// </summary>
    public string? ModDate { get; set; }

    /// <summary>
    /// XMP modify date value.
    /// </summary>
    public string? XMPModifyDate { get; set; }

    /// <summary>
    /// True if ModDate and XMP modify date match.
    /// </summary>
    public bool DoModDatesMatch { get; set; }

    /// <summary>
    /// CreationDate entry value.
    /// </summary>
    public string? CreationDate { get; set; }

    /// <summary>
    /// XMP create date value.
    /// </summary>
    public string? XMPCreateDate { get; set; }

    /// <summary>
    /// True if CreationDate and XMP create date match.
    /// </summary>
    public bool DoCreationDatesMatch { get; set; }

    /// <summary>
    /// Title entry value without trailing zero.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// XMP title value.
    /// </summary>
    public string? XMPTitle { get; set; }

    /// <summary>
    /// Author entry value without trailing zero.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Comma-separated XMP creator array.
    /// </summary>
    public string? XMPCreator { get; set; }

    /// <summary>
    /// Size of XMP creator array.
    /// </summary>
    public int XMPCreatorSize { get; set; }

    /// <summary>
    /// Subject entry value without trailing zero.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// XMP description value.
    /// </summary>
    public string? XMPDescription { get; set; }

    /// <summary>
    /// Producer entry value without trailing zero.
    /// </summary>
    public string? Producer { get; set; }

    /// <summary>
    /// XMP producer value.
    /// </summary>
    public string? XMPProducer { get; set; }

    /// <summary>
    /// Creator entry value without trailing zero.
    /// </summary>
    public string? Creator { get; set; }

    /// <summary>
    /// XMP creator tool value.
    /// </summary>
    public string? XMPCreatorTool { get; set; }

    /// <summary>
    /// Keywords entry value without trailing zero.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// XMP keywords value.
    /// </summary>
    public string? XMPKeywords { get; set; }
}

/// <summary>
/// PDF array object.
/// </summary>
public class CosArray : CosObject
{
    /// <summary>
    /// Number of elements in the array.
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// Elements contained in the array.
    /// </summary>
    public IReadOnlyList<CosObject> Elements { get; set; } = [];
}

/// <summary>
/// Bounding box array.
/// </summary>
public class CosBBox : CosArray
{
    /// <summary>
    /// Top value.
    /// </summary>
    public decimal Top { get; set; }

    /// <summary>
    /// Bottom value.
    /// </summary>
    public decimal Bottom { get; set; }

    /// <summary>
    /// Left value.
    /// </summary>
    public decimal Left { get; set; }

    /// <summary>
    /// Right value.
    /// </summary>
    public decimal Right { get; set; }
}

/// <summary>
/// Cross-reference table object.
/// </summary>
public class CosXRef : CosObject
{
    /// <summary>
    /// True if subsection headers are space-separated per PDF/A.
    /// </summary>
    public bool SubsectionHeaderSpaceSeparated { get; set; }

    /// <summary>
    /// True if xref keyword EOL markers comply with PDF/A.
    /// </summary>
    public bool XrefEOLMarkersComplyPDFA { get; set; }
}