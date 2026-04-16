namespace VeraPdf.NET.Model;

/// <summary>
/// Base class for all operators in the content stream.
/// </summary>
public class Operator : PDFObject
{
}

/// <summary>
/// General graphics state operators.
/// </summary>
public class OpGeneralGS : Operator
{
}

/// <summary>
/// Set line width operator.
/// </summary>
public class OpWLineWidth : OpGeneralGS
{
    /// <summary>
    /// Line width.
    /// </summary>
    public CosNumber? LineWidth { get; set; }
}

/// <summary>
/// Set line cap operator.
/// </summary>
public class OpJLineCap : OpGeneralGS
{
    /// <summary>
    /// Line cap.
    /// </summary>
    public CosInteger? LineCap { get; set; }
}

/// <summary>
/// Set line join operator.
/// </summary>
public class OpJLineJoin : OpGeneralGS
{
    /// <summary>
    /// Line join.
    /// </summary>
    public CosInteger? LineJoin { get; set; }
}

/// <summary>
/// Miter limit operator.
/// </summary>
public class OpMMiterLimit : OpGeneralGS
{
    /// <summary>
    /// Miter limit.
    /// </summary>
    public CosNumber? MiterLimit { get; set; }
}

/// <summary>
/// Set dash pattern operator.
/// </summary>
public class OpD : OpGeneralGS
{
    /// <summary>
    /// Dash array.
    /// </summary>
    public CosArray? DashArray { get; set; }

    /// <summary>
    /// Dash phase.
    /// </summary>
    public CosNumber? DashPhase { get; set; }
}

/// <summary>
/// Rendering intent operator.
/// </summary>
public class OpRi : OpGeneralGS
{
    /// <summary>
    /// Rendering intent.
    /// </summary>
    public CosRenderingIntent? RenderingIntent { get; set; }
}

/// <summary>
/// Flatness operator.
/// </summary>
public class OpI : OpGeneralGS
{
    /// <summary>
    /// Flatness.
    /// </summary>
    public CosNumber? Flatness { get; set; }
}

/// <summary>
/// Extended graphics state operator.
/// </summary>
public class OpGs : OpGeneralGS
{
    /// <summary>
    /// ExtGState dictionary.
    /// </summary>
    public PDExtGState? ExtGState { get; set; }
}

/// <summary>
/// Special graphics state operators.
/// </summary>
public class OpSpecialGS : Operator
{
}

/// <summary>
/// gsave operator.
/// </summary>
public class OpQGsave : OpSpecialGS
{
    /// <summary>
    /// Number of nested g/Q pairs.
    /// </summary>
    public int NestingLevel { get; set; }
}

/// <summary>
/// grestore operator.
/// </summary>
public class OpQGrestore : OpSpecialGS
{
}

/// <summary>
/// Current transformation matrix operator.
/// </summary>
public class OpCm : OpSpecialGS
{
    /// <summary>
    /// Coefficients of the concatenation matrix.
    /// </summary>
    public IReadOnlyList<CosNumber> Matrix { get; set; } = [];
}

/// <summary>
/// Path construction operators.
/// </summary>
public class OpPathConstruction : Operator
{
}

/// <summary>
/// moveto operator.
/// </summary>
public class OpMMoveto : OpPathConstruction
{
    /// <summary>
    /// Operator arguments.
    /// </summary>
    public IReadOnlyList<CosNumber> Point { get; set; } = [];
}

/// <summary>
/// lineto operator.
/// </summary>
public class OpL : OpPathConstruction
{
    /// <summary>
    /// Operator arguments.
    /// </summary>
    public IReadOnlyList<CosNumber> Point { get; set; } = [];
}

/// <summary>
/// curveto operator.
/// </summary>
public class OpC : OpPathConstruction
{
    /// <summary>
    /// Operator arguments.
    /// </summary>
    public IReadOnlyList<CosNumber> ControlPoints { get; set; } = [];
}

/// <summary>
/// Special curveto operator.
/// </summary>
public class OpV : OpPathConstruction
{
    /// <summary>
    /// Operator arguments.
    /// </summary>
    public IReadOnlyList<CosNumber> ControlPoints { get; set; } = [];
}

/// <summary>
/// Special curveto operator.
/// </summary>
public class OpY : OpPathConstruction
{
    /// <summary>
    /// Operator arguments.
    /// </summary>
    public IReadOnlyList<CosNumber> ControlPoints { get; set; } = [];
}

/// <summary>
/// Close subpath operator.
/// </summary>
public class OpH : OpPathConstruction
{
}

/// <summary>
/// rect operator.
/// </summary>
public class OpRe : OpPathConstruction
{
    /// <summary>
    /// Rectangle box.
    /// </summary>
    public IReadOnlyList<CosNumber> RectBox { get; set; } = [];
}

/// <summary>
/// Path paint operators.
/// </summary>
public class OpPathPaint : Operator
{
}

/// <summary>
/// Path close and stroke operator.
/// </summary>
public class OpSCloseStroke : OpPathPaint
{
    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path stroke operator.
/// </summary>
public class OpSStroke : OpPathPaint
{
    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path fill operator.
/// </summary>
public class OpFFill : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }
}

/// <summary>
/// Obsolete path fill operator.
/// </summary>
public class Op_F_fill_obsolete : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }
}

/// <summary>
/// Path fill (even-odd rule) operator.
/// </summary>
public class Op_FStar : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }
}

/// <summary>
/// Path fill and stroke operator.
/// </summary>
public class Op_B_fill_stroke : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }

    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path fill (even-odd rule) and stroke operator.
/// </summary>
public class Op_BStar_eofill_stroke : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }

    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path close, fill and stroke operator.
/// </summary>
public class Op_b_closepath_fill_stroke : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }

    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path close, fill (even-odd rule) and stroke operator.
/// </summary>
public class Op_bstar_closepath_eofill_stroke : OpPathPaint
{
    /// <summary>
    /// Fill color space.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }

    /// <summary>
    /// Stroke color space.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Path painting no-op operator.
/// </summary>
public class Op_n : OpPathPaint
{
}

/// <summary>
/// Clip operators.
/// </summary>
public class OpClip : Operator
{
}

/// <summary>
/// Clip (non-zero rule) operator.
/// </summary>
public class Op_W_clip : OpClip
{
}

/// <summary>
/// Clip (even-odd rule) operator.
/// </summary>
public class Op_WStar : OpClip
{
}

/// <summary>
/// Text object operators.
/// </summary>
public class OpTextObject : Operator
{
}

/// <summary>
/// End text object operator.
/// </summary>
public class Op_ET : OpTextObject
{
}

/// <summary>
/// Begin text object operator.
/// </summary>
public class Op_BT : OpTextObject
{
}

/// <summary>
/// Text state operators.
/// </summary>
public class OpTextState : Operator
{
}

/// <summary>
/// Horizontal text scaling operator.
/// </summary>
public class Op_Tz : OpTextState
{
    /// <summary>
    /// Horizontal text scaling.
    /// </summary>
    public CosNumber? Scale { get; set; }
}

/// <summary>
/// Text rendering mode operator.
/// </summary>
public class Op_Tr : OpTextState
{
    /// <summary>
    /// Rendering mode.
    /// </summary>
    public int RenderingMode { get; set; }
}

/// <summary>
/// Character spacing operator.
/// </summary>
public class Op_Tc : OpTextState
{
    /// <summary>
    /// Character spacing.
    /// </summary>
    public CosNumber? CharSpace { get; set; }
}

/// <summary>
/// Text font operator.
/// </summary>
public class Op_Tf : OpTextState
{
    /// <summary>
    /// Font size.
    /// </summary>
    public CosNumber? Size { get; set; }

    /// <summary>
    /// Text font name.
    /// </summary>
    public CosName? FontName { get; set; }
}

/// <summary>
/// Text leading operator.
/// </summary>
public class Op_Tl : OpTextState
{
    /// <summary>
    /// Text leading.
    /// </summary>
    public CosNumber? Leading { get; set; }
}

/// <summary>
/// Text rise operator.
/// </summary>
public class Op_Ts : OpTextState
{
    /// <summary>
    /// Text rise.
    /// </summary>
    public CosNumber? Rise { get; set; }
}

/// <summary>
/// Word spacing operator.
/// </summary>
public class Op_Tw : OpTextState
{
    /// <summary>
    /// Word spacing.
    /// </summary>
    public CosNumber? WordSpace { get; set; }
}

/// <summary>
/// Text position operators.
/// </summary>
public class OpTextPosition : Operator
{
}

/// <summary>
/// Text position offset operator.
/// </summary>
public class Op_Td : OpTextPosition
{
    /// <summary>
    /// Horizontal offset.
    /// </summary>
    public CosNumber? HorizontalOffset { get; set; }

    /// <summary>
    /// Vertical offset.
    /// </summary>
    public CosNumber? VerticalOffset { get; set; }
}

/// <summary>
/// Text position and leading operator.
/// </summary>
public class Op_TD_Big : OpTextPosition
{
    /// <summary>
    /// Horizontal offset.
    /// </summary>
    public CosNumber? HorizontalOffset { get; set; }

    /// <summary>
    /// Vertical offset.
    /// </summary>
    public CosNumber? VerticalOffset { get; set; }
}

/// <summary>
/// Text matrix operator.
/// </summary>
public class Op_Tm : OpTextPosition
{
    /// <summary>
    /// Elements of text matrix.
    /// </summary>
    public IReadOnlyList<CosNumber> ControlPoints { get; set; } = [];
}

/// <summary>
/// Move to next text line operator.
/// </summary>
public class Op_T_Star : OpTextPosition
{
}

/// <summary>
/// Text show operators.
/// </summary>
public class OpTextShow : Operator
{
    /// <summary>
    /// Current font.
    /// </summary>
    public PDFont? Font { get; set; }

    /// <summary>
    /// Glyphs used in the text.
    /// </summary>
    public IReadOnlyList<Glyph> UsedGlyphs { get; set; } = [];

    /// <summary>
    /// Fill color space for fill rendering modes.
    /// </summary>
    public PDColorSpace? FillCS { get; set; }

    /// <summary>
    /// Stroke color space for stroke rendering modes.
    /// </summary>
    public PDColorSpace? StrokeCS { get; set; }
}

/// <summary>
/// Text show operator.
/// </summary>
public class Op_Tj : OpTextShow
{
    /// <summary>
    /// String to show before font encoding mapping.
    /// </summary>
    public CosString? ShowString { get; set; }
}

/// <summary>
/// Text show operator with individual glyph positioning.
/// </summary>
public class Op_TJ_Big : OpTextShow
{
    /// <summary>
    /// Array containing strings and glyph widths.
    /// </summary>
    public CosArray? SpecialStrings { get; set; }
}

/// <summary>
/// EOL and text show operator.
/// </summary>
public class Op_Quote : OpTextShow
{
    /// <summary>
    /// String to show before font encoding mapping.
    /// </summary>
    public CosString? ShowString { get; set; }
}

/// <summary>
/// EOL and text show operator with char and word spacing.
/// </summary>
public class Op_DoubleQuote : OpTextShow
{
    /// <summary>
    /// String to show before font encoding mapping.
    /// </summary>
    public CosString? ShowString { get; set; }

    /// <summary>
    /// Unscaled word spacing.
    /// </summary>
    public CosNumber? WordSpacing { get; set; }

    /// <summary>
    /// Unscaled character spacing.
    /// </summary>
    public CosNumber? CharacterSpacing { get; set; }
}

/// <summary>
/// Type3 font operators.
/// </summary>
public class OpType3Font : Operator
{
}

/// <summary>
/// Type3 d0 operator.
/// </summary>
public class Op_d0 : OpType3Font
{
    /// <summary>
    /// Horizontal displacement in glyph coordinate system.
    /// </summary>
    public CosNumber? HorizontalDisplacement { get; set; }

    /// <summary>
    /// Vertical displacement in glyph coordinate system.
    /// </summary>
    public CosNumber? VerticalDisplacement { get; set; }
}

/// <summary>
/// Type3 d1 operator.
/// </summary>
public class Op_d1 : OpType3Font
{
    /// <summary>
    /// Width and bounding-box information for the glyph.
    /// </summary>
    public IReadOnlyList<CosNumber> ControlPoints { get; set; } = [];
}

/// <summary>
/// Set color operators without colorspace.
/// </summary>
public class OpSetColor : Operator
{
    /// <summary>
    /// Numeric arguments of the color operators.
    /// </summary>
    public IReadOnlyList<CosNumber> ColorValues { get; set; } = [];
}

/// <summary>
/// Stroke color values operator.
/// </summary>
public class Op_SC_stroke : OpSetColor
{
}

/// <summary>
/// Fill color values operator.
/// </summary>
public class Op_sc_fill : OpSetColor
{
}

/// <summary>
/// Color operators with color space.
/// </summary>
public class OpColor : OpSetColor
{
    /// <summary>
    /// Color space set by this operator.
    /// </summary>
    public PDColorSpace? ColorSpace { get; set; }

    /// <summary>
    /// Name of tiling pattern for scn/SCN with Pattern color space.
    /// </summary>
    public CosName? PatternName { get; set; }
}

/// <summary>
/// Stroke color with optional pattern operator.
/// </summary>
public class Op_SCN_stroke : OpColor
{
}

/// <summary>
/// Fill color with optional pattern operator.
/// </summary>
public class Op_scn_fill : OpColor
{
}

/// <summary>
/// Stroke gray color operator.
/// </summary>
public class Op_G_stroke : OpColor
{
}

/// <summary>
/// Fill gray color operator.
/// </summary>
public class Op_g_fill : OpColor
{
}

/// <summary>
/// Stroke RGB color operator.
/// </summary>
public class Op_RG_stroke : OpColor
{
}

/// <summary>
/// Fill RGB color operator.
/// </summary>
public class Op_rg_fill : OpColor
{
}

/// <summary>
/// Stroke CMYK color operator.
/// </summary>
public class Op_K_stroke : OpColor
{
}

/// <summary>
/// Fill CMYK color operator.
/// </summary>
public class Op_k_fill : OpColor
{
}

/// <summary>
/// Shading operators.
/// </summary>
public class OpShading : Operator
{
}

/// <summary>
/// Smooth shading operator.
/// </summary>
public class Op_sh : OpShading
{
    /// <summary>
    /// Corresponding shading resource.
    /// </summary>
    public PDShading? Shading { get; set; }
}

/// <summary>
/// Inline image operators.
/// </summary>
public class OpInlineImage : Operator
{
}

/// <summary>
/// Begin inline image operator.
/// </summary>
public class Op_BI : OpInlineImage
{
}

/// <summary>
/// End inline image dictionary operator.
/// </summary>
public class Op_ID : OpInlineImage
{
    /// <summary>
    /// Dictionary containing image parameters.
    /// </summary>
    public CosDict? InlineImageDictionary { get; set; }
}

/// <summary>
/// End inline image data operator.
/// </summary>
public class Op_EI : OpInlineImage
{
    /// <summary>
    /// Inline image object.
    /// </summary>
    public PDInlineImage? InlineImage { get; set; }
}

/// <summary>
/// XObject operators.
/// </summary>
public class OpXObject : Operator
{
}

/// <summary>
/// XObject Do operator.
/// </summary>
public class Op_Do : OpXObject
{
    /// <summary>
    /// Corresponding XObject resource.
    /// </summary>
    public PDXObject? XObject { get; set; }
}

/// <summary>
/// Marked content operators.
/// </summary>
public class OpMarkedContent : Operator
{
    /// <summary>
    /// ActualText value.
    /// </summary>
    public CosActualText? ActualText { get; set; }

    /// <summary>
    /// Alt value.
    /// </summary>
    public CosAlt? Alt { get; set; }
}

/// <summary>
/// Define marked content point operator.
/// </summary>
public class Op_MP : OpMarkedContent
{
    /// <summary>
    /// Tag name.
    /// </summary>
    public CosName? Tag { get; set; }
}

/// <summary>
/// Define marked content point with properties operator.
/// </summary>
public class Op_DP : OpMarkedContent
{
    /// <summary>
    /// Tag name.
    /// </summary>
    public CosName? Tag { get; set; }

    /// <summary>
    /// Properties dictionary.
    /// </summary>
    public CosDict? Properties { get; set; }
}

/// <summary>
/// Start marked content region operator.
/// </summary>
public class Op_BMC : OpMarkedContent
{
    /// <summary>
    /// Tag name.
    /// </summary>
    public CosName? Tag { get; set; }
}

/// <summary>
/// Start marked content region with properties operator.
/// </summary>
public class Op_BDC : OpMarkedContent
{
    /// <summary>
    /// Tag name.
    /// </summary>
    public CosName? Tag { get; set; }

    /// <summary>
    /// Properties dictionary.
    /// </summary>
    public CosDict? Properties { get; set; }

    /// <summary>
    /// Lang value from properties dictionary.
    /// </summary>
    public CosLang? Lang { get; set; }
}

/// <summary>
/// End marked content region operator.
/// </summary>
public class Op_EMC : OpMarkedContent
{
}

/// <summary>
/// Compatibility operators.
/// </summary>
public class OpCompatibility : Operator
{
}

/// <summary>
/// Begin compatibility region operator.
/// </summary>
public class Op_BX : OpCompatibility
{
}

/// <summary>
/// End compatibility region operator.
/// </summary>
public class Op_EX : OpCompatibility
{
}

/// <summary>
/// Undefined operator.
/// </summary>
public class Op_Undefined : OpCompatibility
{
    /// <summary>
    /// Operator name.
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// Glyph used in text.
/// </summary>
public class Glyph : PDFObject
{
    /// <summary>
    /// Glyph name or empty when not name-identified.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Glyph width from font dictionary.
    /// </summary>
    public decimal WidthFromDictionary { get; set; }

    /// <summary>
    /// Glyph width from embedded font program.
    /// </summary>
    public decimal WidthFromFontProgram { get; set; }

    /// <summary>
    /// True if glyph is present in the font program.
    /// </summary>
    public bool IsGlyphPresent { get; set; }

    /// <summary>
    /// Unicode value of the glyph.
    /// </summary>
    public string? ToUnicode { get; set; }

    /// <summary>
    /// Rendering mode value when glyph is shown.
    /// </summary>
    public int RenderingMode { get; set; }

    /// <summary>
    /// True if ToUnicode contains private-use-area codes.
    /// </summary>
    public bool UnicodePUA { get; set; }

    /// <summary>
    /// True if ActualText is present for this glyph.
    /// </summary>
    public bool ActualTextPresent { get; set; }

    /// <summary>
    /// True if Alt is present for this glyph.
    /// </summary>
    public bool AltPresent { get; set; }

    /// <summary>
    /// True if this glyph is real content.
    /// </summary>
    public bool IsRealContent { get; set; }
}

/// <summary>
/// Glyph in a composite font.
/// </summary>
public class CIDGlyph : Glyph
{
    /// <summary>
    /// CID value derived from the font CMap.
    /// </summary>
    public int CID { get; set; }
}
