using VeraPdf.NET.Model.Pd.Contracts;

namespace VeraPdf.NET.Model;

/// <summary>
/// Base class for all PD layer objects.
/// </summary>
public class PDObject : PDFObject
{
    /// <summary>
    /// Ampersand-separated list of entries.
    /// </summary>
    public string? Entries { get; set; }

    /// <summary>
    /// Indirect object number and generation number for the current object.
    /// </summary>
    public string? ObjectKey { get; set; }
}

/// <summary>
/// High-level PDF document structure.
/// </summary>
public class PDDocument : PDObject
{
    /// <summary>
    /// Output intents of the document.
    /// </summary>
    public OutputIntents? OutputIntents { get; set; }

    /// <summary>
    /// Collection of all document pages.
    /// </summary>
    public IReadOnlyList<PDPage> Pages { get; set; } = [];

    /// <summary>
    /// Document-level metadata.
    /// </summary>
    public PDMetadata? Metadata { get; set; }

    /// <summary>
    /// True if the catalog dictionary contains metadata.
    /// </summary>
    public bool ContainsMetadata { get; set; }

    /// <summary>
    /// Interactive form dictionary.
    /// </summary>
    public PDAcroForm? AcroForm { get; set; }

    /// <summary>
    /// Additional actions dictionary for the document.
    /// </summary>
    public PDAdditionalActions? AA { get; set; }

    /// <summary>
    /// Document open action.
    /// </summary>
    public PDAction? OpenAction { get; set; }

    /// <summary>
    /// Document open action destination.
    /// </summary>
    public PDDestination? OpenActionDestination { get; set; }

    /// <summary>
    /// Document outlines.
    /// </summary>
    public IReadOnlyList<PDOutline> Outlines { get; set; } = [];

    /// <summary>
    /// Optional content properties dictionary.
    /// </summary>
    public PDOCProperties? OCProperties { get; set; }

    /// <summary>
    /// Structure tree root dictionary.
    /// </summary>
    public PDStructTreeRoot? StructTreeRoot { get; set; }

    /// <summary>
    /// True if the catalog dictionary contains StructTreeRoot.
    /// </summary>
    public bool ContainsStructTreeRoot { get; set; }

    /// <summary>
    /// True if alternate presentations are present.
    /// </summary>
    public bool ContainsAlternatePresentations { get; set; }

    /// <summary>
    /// Lang entry from the document catalog.
    /// </summary>
    public CosLang? Lang { get; set; }

    /// <summary>
    /// True if the catalog dictionary contains Lang.
    /// </summary>
    public bool ContainsLang { get; set; }

    /// <summary>
    /// Permissions object.
    /// </summary>
    public PDPerms? Perms { get; set; }

    /// <summary>
    /// True if the catalog dictionary contains AA.
    /// </summary>
    public bool ContainsAA { get; set; }

    /// <summary>
    /// Output ICC profile color space.
    /// </summary>
    public string? OutputColorSpace { get; set; }

    /// <summary>
    /// Version key value from catalog.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Most common page orientation.
    /// </summary>
    public string? MostCommonOrientation { get; set; }

    /// <summary>
    /// True if the document contains an XRefStream.
    /// </summary>
    public bool ContainsXRefStream { get; set; }
}

/// <summary>
/// PDF page object.
/// </summary>
public class PDPage : PDObject
{
    /// <summary>
    /// Output intents of the page.
    /// </summary>
    public OutputIntents? OutputIntents { get; set; }

    /// <summary>
    /// Additional actions dictionary for the page.
    /// </summary>
    public PDAdditionalActions? AA { get; set; }

    /// <summary>
    /// Transparency blending space.
    /// </summary>
    public TransparencyColorSpace? TransparencyColorSpace { get; set; }

    /// <summary>
    /// Page content stream.
    /// </summary>
    public PDContentStream? ContentStream { get; set; }

    /// <summary>
    /// Page annotations.
    /// </summary>
    public IReadOnlyList<PDAnnot> Annots { get; set; } = [];

    /// <summary>
    /// True if annotations are present.
    /// </summary>
    public bool ContainsAnnotations { get; set; }

    /// <summary>
    /// Parent transparency blending space.
    /// </summary>
    public TransparencyColorSpace? ParentTransparencyColorSpace { get; set; }

    /// <summary>
    /// Group attributes dictionary.
    /// </summary>
    public PDGroup? Group { get; set; }

    /// <summary>
    /// Page media box.
    /// </summary>
    public CosBBox? MediaBox { get; set; }

    /// <summary>
    /// Page crop box.
    /// </summary>
    public CosBBox? CropBox { get; set; }

    /// <summary>
    /// Page bleed box.
    /// </summary>
    public CosBBox? BleedBox { get; set; }

    /// <summary>
    /// Page trim box.
    /// </summary>
    public CosBBox? TrimBox { get; set; }

    /// <summary>
    /// Page art box.
    /// </summary>
    public CosBBox? ArtBox { get; set; }

    /// <summary>
    /// True if page contains presentation steps.
    /// </summary>
    public bool ContainsPresSteps { get; set; }

    /// <summary>
    /// True if page contains transparency.
    /// </summary>
    public bool ContainsTransparency { get; set; }

    /// <summary>
    /// True if group color space is present.
    /// </summary>
    public bool ContainsGroupCS { get; set; }

    /// <summary>
    /// True if page dictionary contains AA.
    /// </summary>
    public bool ContainsAA { get; set; }

    /// <summary>
    /// Tabs key value.
    /// </summary>
    public string? Tabs { get; set; }

    /// <summary>
    /// Page orientation value.
    /// </summary>
    public string? Orientation { get; set; }

    /// <summary>
    /// Output ICC profile color space.
    /// </summary>
    public string? OutputColorSpace { get; set; }

    /// <summary>
    /// Page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Resource dictionary.
    /// </summary>
    public PDResources? Resources { get; set; }
}

/// <summary>
/// Transparency blending space.
/// </summary>
public class TransparencyColorSpace : PDFObject
{
    /// <summary>
    /// Color space type.
    /// </summary>
    public string? ColorSpaceType { get; set; }
}

/// <summary>
/// Array of output intents.
/// </summary>
public class OutputIntents : PDFObject
{
    /// <summary>
    /// Output intent dictionaries.
    /// </summary>
    public IReadOnlyList<PDOutputIntent> OutputIntentsList { get; set; } = [];

    /// <summary>
    /// True if output profile indirect references are consistent.
    /// </summary>
    public bool SameOutputProfileIndirect { get; set; }

    /// <summary>
    /// Comma-separated DestOutputProfile values.
    /// </summary>
    public string? OutputProfileIndirects { get; set; }
}

/// <summary>
/// Content stream with associated resources.
/// </summary>
public class PDContentStream : PDObject
{
    /// <summary>
    /// Operators in the content stream.
    /// </summary>
    public IReadOnlyList<Operator> Operators { get; set; } = [];

    /// <summary>
    /// Undefined resource names used by content stream.
    /// </summary>
    public string? UndefinedResourceNames { get; set; }

    /// <summary>
    /// Inherited resource names used by content stream.
    /// </summary>
    public string? InheritedResourceNames { get; set; }

    /// <summary>
    /// Resource dictionary.
    /// </summary>
    public PDResources? Resources { get; set; }
}

/// <summary>
/// Semantic content stream.
/// </summary>
public class PDSemanticContentStream : PDContentStream
{
    /// <summary>
    /// Content items in the semantic content stream.
    /// </summary>
    public IReadOnlyList<SEContentItem> Content { get; set; } = [];
}

/// <summary>
/// PDF resource object.
/// </summary>
public class PDResource : PDObject
{
    /// <summary>
    /// True if this resource is inherited.
    /// </summary>
    public bool IsInherited { get; set; }
}

/// <summary>
/// Color space object.
/// </summary>
public class PDColorSpace : PDResource, IPDColorSpace
{
    /// <summary>
    /// Number of components.
    /// </summary>
    public int NrComponents { get; set; }
}

/// <summary>
/// Additional-actions dictionary.
/// </summary>
public class PDAdditionalActions : PDObject
{
    /// <summary>
    /// Additional actions.
    /// </summary>
    public IReadOnlyList<PDAction> Actions { get; set; } = [];

    /// <summary>
    /// Parent object type.
    /// </summary>
    public string? ParentType { get; set; }
}

/// <summary>
/// DeviceGray color space.
/// </summary>
public class PDDeviceGray : PDColorSpace, IPDDeviceGray
{
}

/// <summary>
/// DeviceRGB color space.
/// </summary>
public class PDDeviceRGB : PDColorSpace, IPDDeviceRGB
{
}

/// <summary>
/// DeviceCMYK color space.
/// </summary>
public class PDDeviceCMYK : PDColorSpace, IPDDeviceCMYK
{
}

/// <summary>
/// ICCBased color space.
/// </summary>
public class PDICCBased : PDColorSpace, IPDICCBased
{
    /// <summary>
    /// Embedded ICC profile.
    /// </summary>
    public ICCInputProfile? IccProfile { get; set; }

    /// <summary>
    /// Indirect reference to ICC profile.
    /// </summary>
    public string? ICCProfileIndirect { get; set; }

    /// <summary>
    /// Indirect reference to current transparency ICC profile.
    /// </summary>
    public string? CurrentTransparencyProfileIndirect { get; set; }

    /// <summary>
    /// MD5 of ICC profile.
    /// </summary>
    public string? ICCProfileMD5 { get; set; }

    /// <summary>
    /// MD5 of current transparency ICC profile.
    /// </summary>
    public string? CurrentTransparencyICCProfileMD5 { get; set; }
}

/// <summary>
/// ICCBased CMYK color space.
/// </summary>
public class PDICCBasedCMYK : PDICCBased, IPDICCBasedCMYK
{
    /// <summary>
    /// OPM value.
    /// </summary>
    public int OPM { get; set; }

    /// <summary>
    /// Effective overprint flag.
    /// </summary>
    public bool OverprintFlag { get; set; }
}

/// <summary>
/// Lab color space.
/// </summary>
public class PDLab : PDColorSpace, IPDLab
{
}

/// <summary>
/// CalGray color space.
/// </summary>
public class PDCalGray : PDColorSpace, IPDCalGray
{
}

/// <summary>
/// CalRGB color space.
/// </summary>
public class PDCalRGB : PDColorSpace, IPDCalRGB
{
}

/// <summary>
/// Separation color space.
/// </summary>
public class PDSeparation : PDColorSpace, IPDSeparation
{
    /// <summary>
    /// Tint transform function.
    /// </summary>
    public PDFunction? TintTransform { get; set; }

    /// <summary>
    /// Alternate color space.
    /// </summary>
    public PDColorSpace? Alternate { get; set; }

    /// <summary>
    /// Colorant name object.
    /// </summary>
    public CosUnicodeName? ColorantName { get; set; }

    /// <summary>
    /// Colorant name value.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// True if tint and alternate are consistent.
    /// </summary>
    public bool AreTintAndAlternateConsistent { get; set; }
}

/// <summary>
/// DeviceN color space.
/// </summary>
public class PDDeviceN : PDColorSpace, IPDDeviceN
{
    /// <summary>
    /// Tint transform function.
    /// </summary>
    public PDFunction? TintTransform { get; set; }

    /// <summary>
    /// Alternate color space.
    /// </summary>
    public PDColorSpace? Alternate { get; set; }

    /// <summary>
    /// Colorant names.
    /// </summary>
    public IReadOnlyList<CosUnicodeName> ColorantNames { get; set; } = [];

    /// <summary>
    /// Colorants from attributes dictionary.
    /// </summary>
    public IReadOnlyList<PDSeparation> Colorants { get; set; } = [];

    /// <summary>
    /// Process color space from attributes dictionary.
    /// </summary>
    public PDColorSpace? ProcessColor { get; set; }

    /// <summary>
    /// True if all colorants are present.
    /// </summary>
    public bool AreColorantsPresent { get; set; }
}

/// <summary>
/// Indexed color space.
/// </summary>
public class PDIndexed : PDColorSpace, IPDIndexed
{
    /// <summary>
    /// Base color space.
    /// </summary>
    public PDColorSpace? Base { get; set; }
}

/// <summary>
/// Generic pattern color space.
/// </summary>
public class PDPattern : PDColorSpace
{
}

/// <summary>
/// Tiling pattern.
/// </summary>
public class PDTilingPattern : PDPattern
{
    /// <summary>
    /// Pattern content stream.
    /// </summary>
    public PDContentStream? ContentStream { get; set; }
}

/// <summary>
/// Shading pattern.
/// </summary>
public class PDShadingPattern : PDPattern
{
    /// <summary>
    /// Shading object.
    /// </summary>
    public PDShading? Shading { get; set; }
}

/// <summary>
/// PDF font dictionary.
/// </summary>
public class PDFont : PDResource, IPDFont
{
    /// <summary>
    /// Font type value.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Font subtype value.
    /// </summary>
    public string? Subtype { get; set; }

    /// <summary>
    /// Font name.
    /// </summary>
    public string? FontName { get; set; }

    /// <summary>
    /// BaseFont name object.
    /// </summary>
    public CosUnicodeName? BaseFont { get; set; }

    /// <summary>
    /// True if font is symbolic.
    /// </summary>
    public bool IsSymbolic { get; set; }

    /// <summary>
    /// Rendering mode value.
    /// </summary>
    public int RenderingMode { get; set; }

    /// <summary>
    /// Embedded font program.
    /// </summary>
    public FontProgram? FontFile { get; set; }

    /// <summary>
    /// True if font program is present.
    /// </summary>
    public bool ContainsFontFile { get; set; }

    /// <summary>
    /// Embedded font file subtype.
    /// </summary>
    public string? FontFileSubtype { get; set; }

    /// <summary>
    /// True if italic flag is set.
    /// </summary>
    public bool IsItalic { get; set; }
}

/// <summary>
/// Simple font dictionary.
/// </summary>
public class PDSimpleFont : PDFont, IPDSimpleFont
{
    /// <summary>
    /// True if the font is one of 14 standard fonts.
    /// </summary>
    public bool IsStandard { get; set; }

    /// <summary>
    /// FirstChar value.
    /// </summary>
    public int? FirstChar { get; set; }

    /// <summary>
    /// LastChar value.
    /// </summary>
    public int? LastChar { get; set; }

    /// <summary>
    /// Widths array size.
    /// </summary>
    public int? WidthsSize { get; set; }

    /// <summary>
    /// String representation of font encoding.
    /// </summary>
    public string? Encoding { get; set; }

    /// <summary>
    /// True if encoding dictionary contains Differences.
    /// </summary>
    public bool ContainsDifferences { get; set; }
}

/// <summary>
/// TrueType font dictionary.
/// </summary>
public class PDTrueTypeFont : PDSimpleFont, IPDTrueTypeFont
{
    /// <summary>
    /// True if differences are unicode compliant.
    /// </summary>
    public bool DifferencesAreUnicodeCompliant { get; set; }
}

/// <summary>
/// Type1 font dictionary.
/// </summary>
public class PDType1Font : PDSimpleFont, IPDType1Font
{
    /// <summary>
    /// CharSet value.
    /// </summary>
    public string? CharSet { get; set; }

    /// <summary>
    /// True if CharSet lists all glyphs.
    /// </summary>
    public bool CharSetListsAllGlyphs { get; set; }
}

/// <summary>
/// Type3 font dictionary.
/// </summary>
public class PDType3Font : PDSimpleFont, IPDType3Font
{
    /// <summary>
    /// Character strings.
    /// </summary>
    public IReadOnlyList<PDContentStream> CharStrings { get; set; } = [];

    /// <summary>
    /// True if CharStrings array is present.
    /// </summary>
    public bool ContainsCharStrings { get; set; }

    /// <summary>
    /// CharStrings array size.
    /// </summary>
    public int CharStringsCount => CharStrings.Count;
}

/// <summary>
/// CID font dictionary.
/// </summary>
public class PDCIDFont : PDFont, IPDCIDFont
{
    /// <summary>
    /// CIDToGIDMap value.
    /// </summary>
    public string? CIDToGIDMap { get; set; }

    /// <summary>
    /// True if CIDSet is present.
    /// </summary>
    public bool ContainsCIDSet { get; set; }

    /// <summary>
    /// True if CIDSet lists all glyphs.
    /// </summary>
    public bool CidSetListsAllGlyphs { get; set; }
}

/// <summary>
/// Type0 font dictionary.
/// </summary>
public class PDType0Font : PDFont, IPDType0Font
{
    /// <summary>
    /// Ordering key from CIDSystemInfo.
    /// </summary>
    public string? CIDFontOrdering { get; set; }

    /// <summary>
    /// Ordering key from CMap.
    /// </summary>
    public string? CMapOrdering { get; set; }

    /// <summary>
    /// Registry key from CIDSystemInfo.
    /// </summary>
    public string? CIDFontRegistry { get; set; }

    /// <summary>
    /// Registry key from CMap.
    /// </summary>
    public string? CMapRegistry { get; set; }

    /// <summary>
    /// Supplement key from CIDSystemInfo.
    /// </summary>
    public int? CIDFontSupplement { get; set; }

    /// <summary>
    /// Supplement key from CMap.
    /// </summary>
    public int? CMapSupplement { get; set; }

    /// <summary>
    /// Descendant CIDFont.
    /// </summary>
    public PDCIDFont? DescendantFonts { get; set; }

    /// <summary>
    /// Font CMap.
    /// </summary>
    public PDCMap? Encoding { get; set; }

    /// <summary>
    /// Name of the CMap.
    /// </summary>
    public string? CMapName { get; set; }
}

/// <summary>
/// CMap dictionary or predefined CMap name.
/// </summary>
public class PDCMap : PDObject
{
    /// <summary>
    /// CMap name.
    /// </summary>
    public string? CMapName { get; set; }

    /// <summary>
    /// CMap referenced by /UseCMap.
    /// </summary>
    public PDReferencedCMap? UseCMap { get; set; }

    /// <summary>
    /// Embedded CMap file for non-standard CMap.
    /// </summary>
    public CMapFile? EmbeddedFile { get; set; }

    /// <summary>
    /// True if embedded CMap file is present.
    /// </summary>
    public bool ContainsEmbeddedFile { get; set; }
}

/// <summary>
/// CMap referenced through UseCMap.
/// </summary>
public class PDReferencedCMap : PDCMap
{
}

/// <summary>
/// XObject resource.
/// </summary>
public class PDXObject : PDResource
{
    /// <summary>
    /// XObject subtype.
    /// </summary>
    public string? Subtype { get; set; }

    /// <summary>
    /// True if OPI entry is present.
    /// </summary>
    public bool ContainsOPI { get; set; }

    /// <summary>
    /// True if SMask entry is present.
    /// </summary>
    public bool ContainsSMask { get; set; }
}

/// <summary>
/// Image XObject.
/// </summary>
public class PDXImage : PDXObject
{
    /// <summary>
    /// Image color space.
    /// </summary>
    public PDColorSpace? ImageCS { get; set; }

    /// <summary>
    /// Interpolate value.
    /// </summary>
    public bool Interpolate { get; set; }

    /// <summary>
    /// Alternate images.
    /// </summary>
    public IReadOnlyList<PDXImage> Alternates { get; set; } = [];

    /// <summary>
    /// Rendering intent.
    /// </summary>
    public CosRenderingIntent? Intent { get; set; }

    /// <summary>
    /// Linked JPEG2000 stream.
    /// </summary>
    public JPEG2000? JpxStream { get; set; }

    /// <summary>
    /// Soft mask image.
    /// </summary>
    public PDSMaskImage? SMask { get; set; }

    /// <summary>
    /// Mask image.
    /// </summary>
    public PDMaskImage? Mask { get; set; }

    /// <summary>
    /// True if Alternates key is present.
    /// </summary>
    public bool ContainsAlternates { get; set; }

    /// <summary>
    /// BitsPerComponent value.
    /// </summary>
    public int BitsPerComponent { get; set; }

    /// <summary>
    /// True if this image is a mask.
    /// </summary>
    public bool IsMask { get; set; }
}

/// <summary>
/// SMask image.
/// </summary>
public class PDSMaskImage : PDXImage
{
}

/// <summary>
/// Mask image.
/// </summary>
public class PDMaskImage : PDXImage
{
}

/// <summary>
/// Inline image object.
/// </summary>
public class PDInlineImage : PDXImage
{
    /// <summary>
    /// Inline image filters.
    /// </summary>
    public IReadOnlyList<CosIIFilter> F { get; set; } = [];
}

/// <summary>
/// Form XObject.
/// </summary>
public class PDXForm : PDXObject
{
    /// <summary>
    /// Subtype2 value.
    /// </summary>
    public string? Subtype2 { get; set; }

    /// <summary>
    /// True if PS entry is present.
    /// </summary>
    public bool ContainsPS { get; set; }

    /// <summary>
    /// True if Ref entry is present.
    /// </summary>
    public bool ContainsRef { get; set; }

    /// <summary>
    /// Transparency blending space.
    /// </summary>
    public TransparencyColorSpace? TransparencyColorSpace { get; set; }

    /// <summary>
    /// Form content stream.
    /// </summary>
    public PDContentStream? ContentStream { get; set; }

    /// <summary>
    /// Parent transparency blending space.
    /// </summary>
    public TransparencyColorSpace? ParentTransparencyColorSpace { get; set; }

    /// <summary>
    /// Group attributes dictionary.
    /// </summary>
    public PDGroup? Group { get; set; }

    /// <summary>
    /// True if Form XObject has unique semantic parent.
    /// </summary>
    public bool IsUniqueSemanticParent { get; set; }
}

/// <summary>
/// Resource dictionary.
/// </summary>
public class PDResources : PDObject
{
    /// <summary>
    /// Resource names.
    /// </summary>
    public IReadOnlyList<CosUnicodeName> ResourcesNames { get; set; } = [];
}

/// <summary>
/// Group attributes dictionary.
/// </summary>
public class PDGroup : PDObject
{
    /// <summary>
    /// Group subtype.
    /// </summary>
    public string? S { get; set; }

    /// <summary>
    /// Group color space.
    /// </summary>
    public PDColorSpace? ColorSpace { get; set; }
}

/// <summary>
/// ExtGState dictionary.
/// </summary>
public class PDExtGState : PDResource
{
    /// <summary>
    /// TR2 function name value.
    /// </summary>
    public string? TR2NameValue { get; set; }

    /// <summary>
    /// True if TR key is present.
    /// </summary>
    public bool ContainsTR { get; set; }

    /// <summary>
    /// True if TR2 key is present.
    /// </summary>
    public bool ContainsTR2 { get; set; }

    /// <summary>
    /// Halftone dictionary.
    /// </summary>
    public PDHalftone? HT { get; set; }

    /// <summary>
    /// True if HTP key is present.
    /// </summary>
    public bool ContainsHTP { get; set; }

    /// <summary>
    /// True if HTO key is present.
    /// </summary>
    public bool ContainsHTO { get; set; }

    /// <summary>
    /// SMask name value.
    /// </summary>
    public string? SMaskNameValue { get; set; }

    /// <summary>
    /// True if SMask key is present.
    /// </summary>
    public bool ContainsSMask { get; set; }

    /// <summary>
    /// True if BM key is present.
    /// </summary>
    public bool ContainsBM { get; set; }

    /// <summary>
    /// Blend mode name value.
    /// </summary>
    public string? BMNameValue { get; set; }

    /// <summary>
    /// Blend mode object.
    /// </summary>
    public CosBM? BM { get; set; }

    /// <summary>
    /// Fill alpha.
    /// </summary>
    public decimal Ca { get; set; }

    /// <summary>
    /// Stroke alpha.
    /// </summary>
    public decimal CA { get; set; }

    /// <summary>
    /// Rendering intent.
    /// </summary>
    public CosRenderingIntent? RI { get; set; }

    /// <summary>
    /// Custom TR functions.
    /// </summary>
    public IReadOnlyList<PDFunction> CustomFunctions { get; set; } = [];
}

/// <summary>
/// Halftone object.
/// </summary>
public class PDHalftone : PDObject
{
    /// <summary>
    /// Halftone type.
    /// </summary>
    public int HalftoneType { get; set; }

    /// <summary>
    /// Halftone name.
    /// </summary>
    public string? HalftoneName { get; set; }

    /// <summary>
    /// Colorant name from parent type 5 halftone.
    /// </summary>
    public string? ColorantName { get; set; }

    /// <summary>
    /// Transfer function value.
    /// </summary>
    public string? TransferFunction { get; set; }

    /// <summary>
    /// Custom transfer function.
    /// </summary>
    public PDFunction? CustomFunction { get; set; }

    /// <summary>
    /// Nested halftones.
    /// </summary>
    public IReadOnlyList<PDHalftone> Halftones { get; set; } = [];
}

/// <summary>
/// Shading resource.
/// </summary>
public class PDShading : PDResource
{
    /// <summary>
    /// Shading color space.
    /// </summary>
    public PDColorSpace? ColorSpace { get; set; }
}

/// <summary>
/// Annotation object.
/// </summary>
public class PDAnnot : PDObject, IPDAnnot
{
    /// <summary>
    /// Annotation subtype.
    /// </summary>
    public string? Subtype { get; set; }

    /// <summary>
    /// Annotation opacity.
    /// </summary>
    public decimal? CA { get; set; }

    /// <summary>
    /// Annotation flags.
    /// </summary>
    public int? F { get; set; }

    /// <summary>
    /// Appearance dictionary keys string.
    /// </summary>
    public string? AP { get; set; }

    /// <summary>
    /// True if appearances are present.
    /// </summary>
    public bool ContainsAppearances { get; set; }

    /// <summary>
    /// Widget field type value.
    /// </summary>
    public string? FT { get; set; }

    /// <summary>
    /// Type of normal appearance entry.
    /// </summary>
    public string? NType { get; set; }

    /// <summary>
    /// Appearance XForm streams.
    /// </summary>
    public IReadOnlyList<PDXForm> Appearance { get; set; } = [];

    /// <summary>
    /// True if C entry is present.
    /// </summary>
    public bool ContainsC { get; set; }

    /// <summary>
    /// True if IC entry is present.
    /// </summary>
    public bool ContainsIC { get; set; }

    /// <summary>
    /// Annotation action.
    /// </summary>
    public PDAction? A { get; set; }

    /// <summary>
    /// Additional actions dictionary.
    /// </summary>
    public PDAdditionalActions? AA { get; set; }

    /// <summary>
    /// Annotation width.
    /// </summary>
    public decimal Width { get; set; }

    /// <summary>
    /// Annotation height.
    /// </summary>
    public decimal Height { get; set; }

    /// <summary>
    /// True if A entry is present.
    /// </summary>
    public bool ContainsA { get; set; }

    /// <summary>
    /// True if AA entry is present.
    /// </summary>
    public bool ContainsAA { get; set; }

    /// <summary>
    /// Struct parent type.
    /// </summary>
    public string? StructParentType { get; set; }

    /// <summary>
    /// Struct parent standard type.
    /// </summary>
    public string? StructParentStandardType { get; set; }

    /// <summary>
    /// Struct parent object key.
    /// </summary>
    public string? StructParentObjectKey { get; set; }

    /// <summary>
    /// Lang entry value.
    /// </summary>
    public CosLang? Lang { get; set; }

    /// <summary>
    /// True if Lang entry is present.
    /// </summary>
    public bool ContainsLang { get; set; }

    /// <summary>
    /// Contents value.
    /// </summary>
    public string? Contents { get; set; }

    /// <summary>
    /// Alt value.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// True if rectangle is outside CropBox.
    /// </summary>
    public bool IsOutsideCropBox { get; set; }

    /// <summary>
    /// True if annotation is within Artifact structure.
    /// </summary>
    public bool IsArtifact { get; set; }

    /// <summary>
    /// Blend mode.
    /// </summary>
    public CosBM? BM { get; set; }
}

/// <summary>
/// Markup annotation.
/// </summary>
public class PDMarkupAnnot : PDAnnot
{
    /// <summary>
    /// RC entry representation.
    /// </summary>
    public string? RC { get; set; }

    /// <summary>
    /// True if RC entry is present.
    /// </summary>
    public bool ContainsRC { get; set; }
}

/// <summary>
/// 3D annotation.
/// </summary>
public class PD3DAnnot : PDAnnot
{
    /// <summary>
    /// Linked 3D stream dictionary.
    /// </summary>
    public PD3DStream? Stream3D { get; set; }
}

/// <summary>
/// File attachment annotation.
/// </summary>
public class PDFileAttachmentAnnot : PDMarkupAnnot
{
    /// <summary>
    /// True if FS entry is present.
    /// </summary>
    public bool ContainsFS { get; set; }

    /// <summary>
    /// AFRelationship value.
    /// </summary>
    public string? AFRelationship { get; set; }
}

/// <summary>
/// Ink annotation.
/// </summary>
public class PDInkAnnot : PDMarkupAnnot
{
}

/// <summary>
/// Link annotation.
/// </summary>
public class PDLinkAnnot : PDAnnot
{
    /// <summary>
    /// Destination of link annotation.
    /// </summary>
    public PDDestination? Dest { get; set; }

    /// <summary>
    /// Different target annotation object key.
    /// </summary>
    public string? DifferentTargetAnnotObjectKey { get; set; }
}

/// <summary>
/// Movie annotation.
/// </summary>
public class PDMovieAnnot : PDAnnot
{
}

/// <summary>
/// Popup annotation.
/// </summary>
public class PDPopupAnnot : PDAnnot
{
}

/// <summary>
/// Printer's mark annotation.
/// </summary>
public class PDPrinterMarkAnnot : PDAnnot
{
}

/// <summary>
/// Rich media annotation.
/// </summary>
public class PDRichMediaAnnot : PDAnnot
{
}

/// <summary>
/// Rubber stamp annotation.
/// </summary>
public class PDRubberStampAnnot : PDMarkupAnnot
{
    /// <summary>
    /// Name entry value.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Screen annotation.
/// </summary>
public class PDScreenAnnot : PDAnnot
{
}

/// <summary>
/// Sound annotation.
/// </summary>
public class PDSoundAnnot : PDAnnot
{
}

/// <summary>
/// Trap network annotation.
/// </summary>
public class PDTrapNetAnnot : PDAnnot
{
}

/// <summary>
/// Watermark annotation.
/// </summary>
public class PDWatermarkAnnot : PDAnnot
{
}

/// <summary>
/// Widget annotation.
/// </summary>
public class PDWidgetAnnot : PDAnnot
{
    /// <summary>
    /// TU value.
    /// </summary>
    public string? TU { get; set; }

    /// <summary>
    /// True if parent struct element contains Lbl child.
    /// </summary>
    public bool ContainsLbl { get; set; }

    /// <summary>
    /// True if widget belongs to a form field.
    /// </summary>
    public bool IsFieldWidget { get; set; }
}

/// <summary>
/// Destination object.
/// </summary>
public class PDDestination : PDObject
{
    /// <summary>
    /// True if destination is structural.
    /// </summary>
    public bool IsStructDestination { get; set; }
}

/// <summary>
/// 3D stream dictionary.
/// </summary>
public class PD3DStream : PDObject
{
    /// <summary>
    /// Subtype value.
    /// </summary>
    public string? Subtype { get; set; }

    /// <summary>
    /// Color space.
    /// </summary>
    public PDColorSpace? ColorSpace { get; set; }
}

/// <summary>
/// Interactive form dictionary.
/// </summary>
public class PDAcroForm : PDObject, IPDAcroForm
{
    /// <summary>
    /// NeedAppearances flag.
    /// </summary>
    public bool NeedAppearances { get; set; }

    /// <summary>
    /// True if XFA key is present.
    /// </summary>
    public bool ContainsXFA { get; set; }

    /// <summary>
    /// Root interactive form fields.
    /// </summary>
    public IReadOnlyList<PDFormField> FormFields { get; set; } = [];

    /// <summary>
    /// XFA dynamicRender value.
    /// </summary>
    public string? DynamicRender { get; set; }

    /// <summary>
    /// Number of root form fields.
    /// </summary>
    public int FormFieldsCount => FormFields.Count;
}

/// <summary>
/// Interactive form field.
/// </summary>
public class PDFormField : PDObject, IPDFormField
{
    /// <summary>
    /// Field type.
    /// </summary>
    public string? FT { get; set; }

    /// <summary>
    /// Additional actions dictionary.
    /// </summary>
    public PDAdditionalActions? AA { get; set; }

    /// <summary>
    /// True if AA entry is present.
    /// </summary>
    public bool ContainsAA { get; set; }

    /// <summary>
    /// Lang value.
    /// </summary>
    public CosLang? Lang { get; set; }

    /// <summary>
    /// True if Lang entry is present.
    /// </summary>
    public bool ContainsLang { get; set; }

    /// <summary>
    /// TU value.
    /// </summary>
    public string? TU { get; set; }

    /// <summary>
    /// Ff value.
    /// </summary>
    public int? Ff { get; set; }

    /// <summary>
    /// Child fields.
    /// </summary>
    public IReadOnlyList<PDFormField> Kids { get; set; } = [];

    /// <summary>
    /// Number of child fields.
    /// </summary>
    public int KidsCount => Kids.Count;
}

/// <summary>
/// Text form field.
/// </summary>
public class PDTextField : PDFormField
{
    /// <summary>
    /// V entry representation.
    /// </summary>
    public string? V { get; set; }

    /// <summary>
    /// RV entry representation.
    /// </summary>
    public string? RV { get; set; }

    /// <summary>
    /// True if RV entry is present.
    /// </summary>
    public bool ContainsRV { get; set; }
}

/// <summary>
/// Signature form field.
/// </summary>
public class PDSignatureField : PDFormField
{
    /// <summary>
    /// Signature dictionary.
    /// </summary>
    public PDSignature? V { get; set; }
}

/// <summary>
/// Signature dictionary object.
/// </summary>
public class PDSignature : PDObject
{
    /// <summary>
    /// True if ByteRange covers entire document except signature value.
    /// </summary>
    public bool DoesByteRangeCoverEntireDocument { get; set; }

    /// <summary>
    /// PKCS#7 signature contents.
    /// </summary>
    public PKCSDataObject? Contents { get; set; }

    /// <summary>
    /// Signature reference dictionaries.
    /// </summary>
    public IReadOnlyList<PDSigRef> Reference { get; set; } = [];
}

/// <summary>
/// PDF action object.
/// </summary>
public class PDAction : PDObject, IPDAction
{
    /// <summary>
    /// Action subtype.
    /// </summary>
    public string? S { get; set; }

    /// <summary>
    /// Next action.
    /// </summary>
    public PDAction? Next { get; set; }
}

/// <summary>
/// Rendition action object.
/// </summary>
public class PDRenditionAction : PDAction
{
    /// <summary>
    /// Media clip dictionary.
    /// </summary>
    public PDMediaClip? C { get; set; }
}

/// <summary>
/// Encryption dictionary.
/// </summary>
public class PDEncryption : PDObject
{
    /// <summary>
    /// P entry value.
    /// </summary>
    public int P { get; set; }
}

/// <summary>
/// Media clip dictionary.
/// </summary>
public class PDMediaClip : PDObject
{
    /// <summary>
    /// CT value.
    /// </summary>
    public string? CT { get; set; }

    /// <summary>
    /// Alt entry representation.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// True if Alt entry structure is correct.
    /// </summary>
    public bool HasCorrectAlt { get; set; }
}

/// <summary>
/// Named action object.
/// </summary>
public class PDNamedAction : PDAction
{
    /// <summary>
    /// Action name.
    /// </summary>
    public string? N { get; set; }
}

/// <summary>
/// GoTo action object.
/// </summary>
public class PDGoToAction : PDAction
{
    /// <summary>
    /// True if this action contains structural destination.
    /// </summary>
    public bool ContainsStructDestination { get; set; }
}

/// <summary>
/// GoToRemote action object.
/// </summary>
public class PDGoToRemoteAction : PDAction
{
}

/// <summary>
/// XMP metadata stream.
/// </summary>
public class PDMetadata : PDObject, IPDMetadata
{
    /// <summary>
    /// XMP package.
    /// </summary>
    public XMPPackage? XMPPackage { get; set; }

    /// <summary>
    /// Stream containing this metadata.
    /// </summary>
    public CosStream? Stream { get; set; }

    /// <summary>
    /// Filter key value in metadata stream dictionary.
    /// </summary>
    public string? Filter { get; set; }

    /// <summary>
    /// True if this is catalog metadata.
    /// </summary>
    public bool IsCatalogMetadata { get; set; }
}

/// <summary>
/// Output intent dictionary.
/// </summary>
public class PDOutputIntent : PDObject, IPDOutputIntent
{
    /// <summary>
    /// Indirect reference to destination output profile.
    /// </summary>
    public string? DestOutputProfileIndirect { get; set; }

    /// <summary>
    /// Destination output profile.
    /// </summary>
    public ICCOutputProfile? DestProfile { get; set; }

    /// <summary>
    /// True if DestOutputProfileRef key is present.
    /// </summary>
    public bool ContainsDestOutputProfileRef { get; set; }

    /// <summary>
    /// OutputConditionIdentifier value.
    /// </summary>
    public string? OutputConditionIdentifier { get; set; }

    /// <summary>
    /// Output intent subtype value.
    /// </summary>
    public string? S { get; set; }

    /// <summary>
    /// MD5 of destination ICC profile.
    /// </summary>
    public string? ICCProfileMD5 { get; set; }
}

/// <summary>
/// Optional content properties dictionary.
/// </summary>
public class PDOCProperties : PDObject
{
    /// <summary>
    /// Default optional content config.
    /// </summary>
    public PDOCConfig? D { get; set; }

    /// <summary>
    /// Alternate optional content configs.
    /// </summary>
    public IReadOnlyList<PDOCConfig> Configs { get; set; } = [];

    /// <summary>
    /// True if Configs entry exists and contains configs.
    /// </summary>
    public bool ContainsConfigs { get; set; }
}

/// <summary>
/// Optional content configuration dictionary.
/// </summary>
public class PDOCConfig : PDObject
{
    /// <summary>
    /// Configuration name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// True if config name is duplicated.
    /// </summary>
    public bool HasDuplicateName { get; set; }

    /// <summary>
    /// AS key representation.
    /// </summary>
    public string? AS { get; set; }

    /// <summary>
    /// OCGs not referenced in Order array.
    /// </summary>
    public string? OCGsNotContainedInOrder { get; set; }
}

/// <summary>
/// Outline dictionary.
/// </summary>
public class PDOutline : PDObject
{
    /// <summary>
    /// Associated action.
    /// </summary>
    public PDAction? A { get; set; }

    /// <summary>
    /// Outline title.
    /// </summary>
    public CosTextString? Title { get; set; }

    /// <summary>
    /// Destination.
    /// </summary>
    public PDDestination? Dest { get; set; }
}

/// <summary>
/// Structure tree node dictionary.
/// </summary>
public class PDStructTreeNode : PDObject
{
    /// <summary>
    /// Immediate children in structure hierarchy.
    /// </summary>
    public IReadOnlyList<PDStructElem> K { get; set; } = [];

    /// <summary>
    /// Ampersand-separated child standard types.
    /// </summary>
    public string? KidsStandardTypes { get; set; }

    /// <summary>
    /// True if node contains content items.
    /// </summary>
    public bool HasContentItems { get; set; }
}

/// <summary>
/// Structure tree root dictionary.
/// </summary>
public class PDStructTreeRoot : PDStructTreeNode, IPDStructTreeRoot
{
    /// <summary>
    /// Role map names.
    /// </summary>
    public IReadOnlyList<CosUnicodeName> RoleMapNames { get; set; } = [];

    /// <summary>
    /// Namespace URI of first child standard type.
    /// </summary>
    public string? FirstChildStandardTypeNamespaceURL { get; set; }

    /// <summary>
    /// Number of role map names.
    /// </summary>
    public int RoleMapNamesCount => RoleMapNames.Count;
}

/// <summary>
/// Structure element dictionary.
/// </summary>
public class PDStructElem : PDStructTreeNode, IPDStructElem
{
    /// <summary>
    /// Parent standard type.
    /// </summary>
    public string? ParentStandardType { get; set; }

    /// <summary>
    /// Structure type name.
    /// </summary>
    public CosUnicodeName? S { get; set; }

    /// <summary>
    /// Namespace and tag value.
    /// </summary>
    public string? NamespaceAndTag { get; set; }

    /// <summary>
    /// Raw structure type value.
    /// </summary>
    public string? ValueS { get; set; }

    /// <summary>
    /// Role-mapped standard type.
    /// </summary>
    public string? StandardType { get; set; }

    /// <summary>
    /// Lang entry value.
    /// </summary>
    public CosLang? Lang { get; set; }

    /// <summary>
    /// True if Lang entry is present.
    /// </summary>
    public bool ContainsLang { get; set; }

    /// <summary>
    /// Parent Lang value.
    /// </summary>
    public string? ParentLang { get; set; }

    /// <summary>
    /// Remapped standard type value.
    /// </summary>
    public string? RemappedStandardType { get; set; }

    /// <summary>
    /// Alt value.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// ActualText value.
    /// </summary>
    public string? ActualText { get; set; }

    /// <summary>
    /// E value.
    /// </summary>
    public string? E { get; set; }

    /// <summary>
    /// True if circular mapping exists.
    /// </summary>
    public bool CircularMappingExist { get; set; }

    /// <summary>
    /// ActualText object.
    /// </summary>
    public CosActualText? ActualTextValue { get; set; }

    /// <summary>
    /// Alt object.
    /// </summary>
    public CosAlt? AltValue { get; set; }

    /// <summary>
    /// Role-map remap within same namespace.
    /// </summary>
    public string? RoleMapToSameNamespaceTag { get; set; }

    /// <summary>
    /// True if P entry is present.
    /// </summary>
    public bool ContainsParent { get; set; }

    /// <summary>
    /// True if struct element is grouping.
    /// </summary>
    public bool IsGrouping { get; set; }
}

/// <summary>
/// Permissions dictionary.
/// </summary>
public class PDPerms : PDObject
{
}

/// <summary>
/// Signature reference dictionary.
/// </summary>
public class PDSigRef : PDObject
{
    /// <summary>
    /// True if permissions contain DocMDP.
    /// </summary>
    public bool PermsContainDocMDP { get; set; }
}

/// <summary>
/// PDF function object.
/// </summary>
public class PDFunction : PDObject
{
    /// <summary>
    /// Function type.
    /// </summary>
    public int FunctionType { get; set; }
}

/// <summary>
/// Type0 function.
/// </summary>
public class PDType0Function : PDFunction
{
}

/// <summary>
/// Type2 function.
/// </summary>
public class PDType2Function : PDFunction
{
}

/// <summary>
/// Type3 function.
/// </summary>
public class PDType3Function : PDFunction
{
    /// <summary>
    /// Nested functions.
    /// </summary>
    public IReadOnlyList<PDFunction> Functions { get; set; } = [];
}

/// <summary>
/// Type4 function.
/// </summary>
public class PDType4Function : PDFunction
{
    /// <summary>
    /// Operators in type4 function.
    /// </summary>
    public IReadOnlyList<CosObject> Operators { get; set; } = [];
}
