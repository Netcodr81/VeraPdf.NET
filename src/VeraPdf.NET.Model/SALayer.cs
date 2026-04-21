using System;
using System.Collections.Generic;
using System.Text;
using VeraPdf.NET.Model.Contracts.StructureAnalysis;

namespace VeraPdf.NET.Model;

/// <summary>
/// Base structure-analysis object.
/// </summary>
public class SAObject : PDFObject, ISAObject
{
    /// <summary>
    /// Group identifier in format id:&lt;number&gt;.
    /// </summary>
    public string? StructureID { get; set; }

    /// <summary>
    /// List of error codes.
    /// </summary>
    public IReadOnlyList<string> ErrorCodes { get; set; } = [];

    /// <summary>
    /// List of error arguments.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> ErrorArguments { get; set; } = [];

    /// <summary>
    /// Number of error codes.
    /// </summary>
    public int ErrorCodesCount => ErrorCodes.Count;

    /// <summary>
    /// Number of error argument groups.
    /// </summary>
    public int ErrorArgumentsCount => ErrorArguments.Count;
}

/// <summary>
/// High-level PDF document for structure analysis.
/// </summary>
public class SAPDFDocument : PDFObject, ISAPDFDocument
{
    /// <summary>
    /// Document pages.
    /// </summary>
    public IReadOnlyList<SAPage> Pages { get; set; } = [];

    /// <summary>
    /// Repeated character groups in document.
    /// </summary>
    public IReadOnlyList<SARepeatedCharacters> RepeatedCharacters { get; set; } = [];

    /// <summary>
    /// Document lists.
    /// </summary>
    public IReadOnlyList<SAList> Lists { get; set; } = [];

    /// <summary>
    /// Structure tree root.
    /// </summary>
    public SAStructTreeRoot? StructTreeRoot { get; set; }

    /// <summary>
    /// Number of pages.
    /// </summary>
    public int PagesCount => Pages.Count;

    /// <summary>
    /// Number of repeated-character groups.
    /// </summary>
    public int RepeatedCharactersCount => RepeatedCharacters.Count;

    /// <summary>
    /// Number of lists.
    /// </summary>
    public int ListsCount => Lists.Count;

    /// <summary>
    /// True if structure tree root exists.
    /// </summary>
    public bool ContainsStructTreeRoot => StructTreeRoot != null;
}

/// <summary>
/// Structure tree root.
/// </summary>
public class SAStructTreeRoot : PDFObject, ISAStructTreeRoot
{
    /// <summary>
    /// Immediate children in structure hierarchy.
    /// </summary>
    public IReadOnlyList<SAStructElem> Children { get; set; } = [];

    /// <summary>
    /// Number of immediate children.
    /// </summary>
    public int ChildrenCount => Children.Count;
}

/// <summary>
/// Structure element.
/// </summary>
public class SAStructElem : SAObject, ISAStructElem
{
    /// <summary>
    /// Immediate children of the structure element.
    /// </summary>
    public IReadOnlyList<PDFObject> Children { get; set; } = [];

    /// <summary>
    /// Structure type.
    /// </summary>
    public string? ValueS { get; set; }

    /// <summary>
    /// Probability score that semantic type is correct.
    /// </summary>
    public decimal CorrectSemanticScore { get; set; }

    /// <summary>
    /// Suggested correct element type.
    /// </summary>
    public string? CorrectType { get; set; }

    /// <summary>
    /// Standard type of structure element.
    /// </summary>
    public string? StandardType { get; set; }

    /// <summary>
    /// True if this is a table element.
    /// </summary>
    public bool IsTableElem { get; set; }

    /// <summary>
    /// True if this is a table child element.
    /// </summary>
    public bool IsTableChild { get; set; }

    /// <summary>
    /// True if this is a list element.
    /// </summary>
    public bool IsListElem { get; set; }

    /// <summary>
    /// True if this is a list child element.
    /// </summary>
    public bool IsListChild { get; set; }

    /// <summary>
    /// False if structure element contains other structure elements.
    /// </summary>
    public bool IsLeafElem { get; set; }

    /// <summary>
    /// Ampersand-separated standard types of parent elements.
    /// </summary>
    public string? ParentsStandardTypes { get; set; }

    /// <summary>
    /// Ampersand-separated standard types of child elements.
    /// </summary>
    public string? KidsStandardTypes { get; set; }

    /// <summary>
    /// Parent element standard type.
    /// </summary>
    public string? ParentStandardType { get; set; }

    /// <summary>
    /// True if this element has lowest-depth error.
    /// </summary>
    public bool HasLowestDepthError { get; set; }

    /// <summary>
    /// First page number containing this structure element.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Last page number containing this structure element.
    /// </summary>
    public int LastPage { get; set; }

    /// <summary>
    /// Number of immediate children.
    /// </summary>
    public int ChildrenCount => Children.Count;
}

/// <summary>
/// Generic chunk such as text, image, or other content.
/// </summary>
public class SAChunk : PDFObject
{
}

/// <summary>
/// Page in structure analysis.
/// </summary>
public class SAPage : PDFObject, ISAPage
{
    /// <summary>
    /// Page annotations.
    /// </summary>
    public IReadOnlyList<SAAnnotation> Annots { get; set; } = [];

    /// <summary>
    /// Table borders on this page.
    /// </summary>
    public IReadOnlyList<SATableBorder> TableBorders { get; set; } = [];

    /// <summary>
    /// Number of annotations on this page.
    /// </summary>
    public int AnnotsCount => Annots.Count;

    /// <summary>
    /// Number of table borders on this page.
    /// </summary>
    public int TableBordersCount => TableBorders.Count;
}

/// <summary>
/// Annotation in structure analysis.
/// </summary>
public class SAAnnotation : PDFObject, ISAAnnotation
{
    /// <summary>
    /// Text located in annotation rectangle.
    /// </summary>
    public string? TextValue { get; set; }

    /// <summary>
    /// Annotation flags.
    /// </summary>
    public int F { get; set; }

    /// <summary>
    /// Contents entry value.
    /// </summary>
    public string? Contents { get; set; }

    /// <summary>
    /// Alt entry of parent structure element.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// True if annotation rectangle is outside CropBox.
    /// </summary>
    public bool IsOutsideCropBox { get; set; }
}

/// <summary>
/// Repeated characters group.
/// </summary>
public class SARepeatedCharacters : PDFObject
{
    /// <summary>
    /// True if repeated characters are not spaces.
    /// </summary>
    public bool IsNonSpace { get; set; }

    /// <summary>
    /// Number of repeated characters.
    /// </summary>
    public int NumberOfRepeatedCharacters { get; set; }
}

/// <summary>
/// Link annotation in structure analysis.
/// </summary>
public class SALinkAnnotation : SAAnnotation, ISALinkAnnotation
{
    /// <summary>
    /// True if text value is a link.
    /// </summary>
    public bool TextValueIsLink { get; set; }

    /// <summary>
    /// True if Contents entry is a link.
    /// </summary>
    public bool ContentsIsLink { get; set; }

    /// <summary>
    /// True if Alt entry is a link.
    /// </summary>
    public bool AltIsLink { get; set; }
}

/// <summary>
/// Text chunk.
/// </summary>
public class SATextChunk : SAChunk, ISATextChunk
{
    /// <summary>
    /// Text size.
    /// </summary>
    public decimal TextSize { get; set; }

    /// <summary>
    /// Contrast ratio.
    /// </summary>
    public decimal ContrastRatio { get; set; }

    /// <summary>
    /// Text weight.
    /// </summary>
    public decimal TextWeight { get; set; }

    /// <summary>
    /// True if chunk has style different from common style.
    /// </summary>
    public bool HasSpecialStyle { get; set; }

    /// <summary>
    /// True if chunk has background different from common background.
    /// </summary>
    public bool HasSpecialBackground { get; set; }

    /// <summary>
    /// True if this text chunk is underlined.
    /// </summary>
    public bool IsUnderlined { get; set; }

    /// <summary>
    /// Ampersand-separated parent standard types.
    /// </summary>
    public string? ParentsStandardTypes { get; set; }

    /// <summary>
    /// True if this text chunk consists of white spaces.
    /// </summary>
    public bool IsWhiteSpaceChunk { get; set; }
}

/// <summary>
/// Image chunk.
/// </summary>
public class SAImageChunk : SAChunk
{
}

/// <summary>
/// Line art chunk.
/// </summary>
public class SALineArtChunk : SAChunk
{
}

/// <summary>
/// Table border.
/// </summary>
public class SATableBorder : PDFObject, ISATableBorder
{
    /// <summary>
    /// Rows of this table.
    /// </summary>
    public IReadOnlyList<SATableBorderRow> Rows { get; set; } = [];

    /// <summary>
    /// Number of rows.
    /// </summary>
    public int RowsCount => Rows.Count;
}

/// <summary>
/// Table border row.
/// </summary>
public class SATableBorderRow : PDFObject, ISATableBorderRow
{
    /// <summary>
    /// Cells in this row.
    /// </summary>
    public IReadOnlyList<SATableBorderCell> Cells { get; set; } = [];

    /// <summary>
    /// Number of cells.
    /// </summary>
    public int CellsCount => Cells.Count;
}

/// <summary>
/// Table border cell.
/// </summary>
public class SATableBorderCell : SAObject, ISATableBorderCell
{
    /// <summary>
    /// Column span.
    /// </summary>
    public int ColSpan { get; set; }

    /// <summary>
    /// Row span.
    /// </summary>
    public int RowSpan { get; set; }
}

/// <summary>
/// List.
/// </summary>
public class SAList : PDFObject, ISAList
{
    /// <summary>
    /// List items.
    /// </summary>
    public IReadOnlyList<SAListItem> Items { get; set; } = [];

    /// <summary>
    /// Number of list items.
    /// </summary>
    public int ItemsCount => Items.Count;
}

/// <summary>
/// List item.
/// </summary>
public class SAListItem : SAObject, ISAListItem
{
}

/// <summary>
/// &lt;Document&gt; structure element.
/// </summary>
public class SADocument : SAStructElem
{
}

/// <summary>
/// &lt;Part&gt; structure element.
/// </summary>
public class SAPart : SAStructElem
{
}

/// <summary>
/// &lt;Div&gt; structure element.
/// </summary>
public class SADiv : SAStructElem
{
}

/// <summary>
/// &lt;Caption&gt; structure element.
/// </summary>
public class SACaption : SAStructElem
{
}

/// <summary>
/// &lt;H&gt; structure element.
/// </summary>
public class SAH : SAStructElem
{
}

/// <summary>
/// &lt;P&gt; structure element.
/// </summary>
public class SAP : SAStructElem
{
}

/// <summary>
/// &lt;L&gt; structure element.
/// </summary>
public class SAL : SAStructElem
{
}

/// <summary>
/// &lt;LI&gt; structure element.
/// </summary>
public class SALI : SAStructElem
{
}

/// <summary>
/// &lt;Lbl&gt; structure element.
/// </summary>
public class SALbl : SAStructElem
{
}

/// <summary>
/// &lt;LBody&gt; structure element.
/// </summary>
public class SALBody : SAStructElem
{
}

/// <summary>
/// &lt;Table&gt; structure element.
/// </summary>
public class SATable : SAStructElem
{
}

/// <summary>
/// &lt;TR&gt; structure element.
/// </summary>
public class SATR : SAStructElem
{
}

/// <summary>
/// &lt;TH&gt; or &lt;TD&gt; structure element.
/// </summary>
public class SATableCell : SAStructElem
{
}

/// <summary>
/// &lt;TH&gt; structure element.
/// </summary>
public class SATH : SATableCell
{
}

/// <summary>
/// &lt;TD&gt; structure element.
/// </summary>
public class SATD : SATableCell
{
}

/// <summary>
/// &lt;Span&gt; structure element.
/// </summary>
public class SASpan : SAStructElem
{
}

/// <summary>
/// &lt;Link&gt; structure element.
/// </summary>
public class SALink : SAStructElem
{
}

/// <summary>
/// &lt;Art&gt; structure element.
/// </summary>
public class SAArt : SAStructElem
{
}

/// <summary>
/// &lt;Sect&gt; structure element.
/// </summary>
public class SASect : SAStructElem
{
}

/// <summary>
/// &lt;BlockQuote&gt; structure element.
/// </summary>
public class SABlockQuote : SAStructElem
{
}

/// <summary>
/// &lt;TOC&gt; structure element.
/// </summary>
public class SATOC : SAStructElem
{
}

/// <summary>
/// &lt;TOCI&gt; structure element.
/// </summary>
public class SATOCI : SAStructElem
{
}

/// <summary>
/// &lt;Index&gt; structure element.
/// </summary>
public class SAIndex : SAStructElem
{
}

/// <summary>
/// &lt;NonStruct&gt; structure element.
/// </summary>
public class SANonStruct : SAStructElem
{
}

/// <summary>
/// &lt;Private&gt; structure element.
/// </summary>
public class SAPrivate : SAStructElem
{
}

/// <summary>
/// &lt;Quote&gt; structure element.
/// </summary>
public class SAQuote : SAStructElem
{
}

/// <summary>
/// &lt;Note&gt; structure element.
/// </summary>
public class SANote : SAStructElem
{
}

/// <summary>
/// &lt;Reference&gt; structure element.
/// </summary>
public class SAReference : SAStructElem
{
}

/// <summary>
/// &lt;BibEntry&gt; structure element.
/// </summary>
public class SABibEntry : SAStructElem
{
}

/// <summary>
/// &lt;Code&gt; structure element.
/// </summary>
public class SACode : SAStructElem
{
}

/// <summary>
/// &lt;Hn&gt; structure element.
/// </summary>
public class SAHn : SAStructElem
{
}

/// <summary>
/// &lt;Figure&gt; structure element.
/// </summary>
public class SAFigure : SAStructElem
{
    /// <summary>
    /// True if figure has bounding box.
    /// </summary>
    public bool HasBBox { get; set; }
}

/// <summary>
/// &lt;Formula&gt; structure element.
/// </summary>
public class SAFormula : SAStructElem
{
}

/// <summary>
/// &lt;Form&gt; structure element.
/// </summary>
public class SAForm : SAStructElem
{
}

/// <summary>
/// &lt;THead&gt; structure element.
/// </summary>
public class SATHead : SAStructElem
{
}

/// <summary>
/// &lt;TBody&gt; structure element.
/// </summary>
public class SATBody : SAStructElem
{
}

/// <summary>
/// &lt;TFoot&gt; structure element.
/// </summary>
public class SATFoot : SAStructElem
{
}

/// <summary>
/// &lt;Annot&gt; structure element.
/// </summary>
public class SAAnnot : SAStructElem
{
}

/// <summary>
/// &lt;Ruby&gt; structure element.
/// </summary>
public class SARuby : SAStructElem
{
}

/// <summary>
/// &lt;Warichu&gt; structure element.
/// </summary>
public class SAWarichu : SAStructElem
{
}

/// <summary>
/// &lt;RB&gt; structure element.
/// </summary>
public class SARB : SAStructElem
{
}

/// <summary>
/// &lt;RT&gt; structure element.
/// </summary>
public class SART : SAStructElem
{
}

/// <summary>
/// &lt;RP&gt; structure element.
/// </summary>
public class SARP : SAStructElem
{
}

/// <summary>
/// &lt;WT&gt; structure element.
/// </summary>
public class SAWT : SAStructElem
{
}

/// <summary>
/// &lt;WP&gt; structure element.
/// </summary>
public class SAWP : SAStructElem
{
}

/// <summary>
/// &lt;DocumentFragment&gt; structure element.
/// </summary>
public class SADocumentFragment : SAStructElem
{
}

/// <summary>
/// &lt;Aside&gt; structure element.
/// </summary>
public class SAAside : SAStructElem
{
}

/// <summary>
/// &lt;Title&gt; structure element.
/// </summary>
public class SATitle : SAStructElem
{
}

/// <summary>
/// &lt;FENote&gt; structure element.
/// </summary>
public class SAFENote : SAStructElem
{
}

/// <summary>
/// &lt;Sub&gt; structure element.
/// </summary>
public class SASub : SAStructElem
{
}

/// <summary>
/// &lt;Em&gt; structure element.
/// </summary>
public class SAEm : SAStructElem
{
}

/// <summary>
/// &lt;Strong&gt; structure element.
/// </summary>
public class SAStrong : SAStructElem
{
}

/// <summary>
/// &lt;Artifact&gt; structure element.
/// </summary>
public class SAArtifact : SAStructElem
{
}

/// <summary>
/// Structure element with no standard tag mapping.
/// </summary>
public class SANonStandard : SAStructElem
{
}


