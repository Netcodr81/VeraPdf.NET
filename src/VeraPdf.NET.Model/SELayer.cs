using System;
using System.Collections.Generic;
using System.Text;

namespace VeraPdf.NET.Model;

/// <summary>
/// Structure element with no mapping to standard PDF tag.
/// </summary>
public class SENonStandard : PDStructElem
{
    /// <summary>
    /// True if this structure element is not mapped to a standard type.
    /// </summary>
    public bool IsNotMappedToStandardType { get; set; }
}

/// <summary>
/// &lt;Document&gt; structure element.
/// </summary>
public class SEDocument : PDStructElem
{
}

/// <summary>
/// &lt;Part&gt; structure element.
/// </summary>
public class SEPart : PDStructElem
{
}

/// <summary>
/// &lt;Div&gt; structure element.
/// </summary>
public class SEDiv : PDStructElem
{
}

/// <summary>
/// &lt;Caption&gt; structure element.
/// </summary>
public class SECaption : PDStructElem
{
}

/// <summary>
/// &lt;H&gt; structure element.
/// </summary>
public class SEH : PDStructElem
{
}

/// <summary>
/// &lt;P&gt; structure element.
/// </summary>
public class SEP : PDStructElem
{
}

/// <summary>
/// &lt;L&gt; structure element.
/// </summary>
public class SEL : PDStructElem
{
    /// <summary>
    /// Value of the ListNumbering attribute.
    /// </summary>
    public string? ListNumbering { get; set; }

    /// <summary>
    /// True if this list contains at least one Lbl descendant node.
    /// </summary>
    public bool ContainsLabels { get; set; }
}

/// <summary>
/// &lt;LI&gt; structure element.
/// </summary>
public class SELI : PDStructElem
{
}

/// <summary>
/// &lt;Lbl&gt; structure element.
/// </summary>
public class SELbl : PDStructElem
{
}

/// <summary>
/// &lt;LBody&gt; structure element.
/// </summary>
public class SELBody : PDStructElem
{
}

/// <summary>
/// &lt;Table&gt; structure element.
/// </summary>
public class SETable : PDStructElem
{
    /// <summary>
    /// True if table structure is determinable via Headers/IDs or Scope.
    /// </summary>
    public bool UseHeadersAndIdOrScope { get; set; }

    /// <summary>
    /// Column span of the table.
    /// </summary>
    public int ColumnSpan { get; set; }

    /// <summary>
    /// Row span of the table.
    /// </summary>
    public int RowSpan { get; set; }

    /// <summary>
    /// Number of row with wrong column span.
    /// </summary>
    public int NumberOfRowWithWrongColumnSpan { get; set; }

    /// <summary>
    /// Number of column with wrong row span.
    /// </summary>
    public int NumberOfColumnWithWrongRowSpan { get; set; }

    /// <summary>
    /// Wrong column span value for failing row.
    /// </summary>
    public int WrongColumnSpan { get; set; }
}

/// <summary>
/// &lt;TR&gt; structure element.
/// </summary>
public class SETR : PDStructElem
{
}

/// <summary>
/// &lt;TH&gt; or &lt;TD&gt; structure element.
/// </summary>
public class SETableCell : PDStructElem
{
    /// <summary>
    /// /ColSpan value.
    /// </summary>
    public int ColSpan { get; set; }

    /// <summary>
    /// /RowSpan value.
    /// </summary>
    public int RowSpan { get; set; }

    /// <summary>
    /// True if this cell intersects other cells.
    /// </summary>
    public bool HasIntersection { get; set; }
}

/// <summary>
/// &lt;TH&gt; structure element.
/// </summary>
public class SETH : SETableCell
{
}

/// <summary>
/// &lt;TD&gt; structure element.
/// </summary>
public class SETD : SETableCell
{
    /// <summary>
    /// True if this cell has connected headers.
    /// </summary>
    public bool HasConnectedHeader { get; set; }

    /// <summary>
    /// Headers not found as IDs in any TH element.
    /// </summary>
    public string? UnknownHeaders { get; set; }
}

/// <summary>
/// &lt;Span&gt; structure element.
/// </summary>
public class SESpan : PDStructElem
{
}

/// <summary>
/// &lt;Link&gt; structure element.
/// </summary>
public class SELink : PDStructElem
{
}

/// <summary>
/// &lt;Art&gt; structure element.
/// </summary>
public class SEArt : PDStructElem
{
}

/// <summary>
/// &lt;Sect&gt; structure element.
/// </summary>
public class SESect : PDStructElem
{
}

/// <summary>
/// &lt;BlockQuote&gt; structure element.
/// </summary>
public class SEBlockQuote : PDStructElem
{
}

/// <summary>
/// &lt;TOC&gt; structure element.
/// </summary>
public class SETOC : PDStructElem
{
}

/// <summary>
/// &lt;TOCI&gt; structure element.
/// </summary>
public class SETOCI : PDStructElem
{
    /// <summary>
    /// True if this TOCI or descendants contain Ref entry.
    /// </summary>
    public bool ContainsRef { get; set; }
}

/// <summary>
/// &lt;Index&gt; structure element.
/// </summary>
public class SEIndex : PDStructElem
{
}

/// <summary>
/// &lt;NonStruct&gt; structure element.
/// </summary>
public class SENonStruct : PDStructElem
{
}

/// <summary>
/// &lt;Private&gt; structure element.
/// </summary>
public class SEPrivate : PDStructElem
{
}

/// <summary>
/// &lt;Quote&gt; structure element.
/// </summary>
public class SEQuote : PDStructElem
{
}

/// <summary>
/// &lt;Note&gt; structure element.
/// </summary>
public class SENote : PDStructElem
{
    /// <summary>
    /// Value of the ID entry.
    /// </summary>
    public string? NoteID { get; set; }

    /// <summary>
    /// True if this ID has already been found.
    /// </summary>
    public bool HasDuplicateNoteID { get; set; }
}

/// <summary>
/// &lt;Reference&gt; structure element.
/// </summary>
public class SEReference : PDStructElem
{
}

/// <summary>
/// &lt;BibEntry&gt; structure element.
/// </summary>
public class SEBibEntry : PDStructElem
{
}

/// <summary>
/// &lt;Code&gt; structure element.
/// </summary>
public class SECode : PDStructElem
{
}

/// <summary>
/// &lt;Hn&gt; structure element.
/// </summary>
public class SEHn : PDStructElem
{
    /// <summary>
    /// False if heading nesting level is incorrect.
    /// </summary>
    public bool HasCorrectNestingLevel { get; set; }

    /// <summary>
    /// Heading nesting level.
    /// </summary>
    public int NestingLevel { get; set; }
}

/// <summary>
/// &lt;Figure&gt; structure element.
/// </summary>
public class SEFigure : PDStructElem
{
}

/// <summary>
/// &lt;Formula&gt; structure element.
/// </summary>
public class SEFormula : PDStructElem
{
}

/// <summary>
/// MathML namespace structure element.
/// </summary>
public class SEMathMLStructElem : PDStructElem
{
    /// <summary>
    /// True if at least one parent is Formula or MathML type.
    /// </summary>
    public bool HasParentFormulaOrMathML { get; set; }
}

/// <summary>
/// Content item such as text, image, lineart, shading, or form.
/// </summary>
public class SEContentItem : PDFObject
{
    /// <summary>
    /// Tags associated with parent marked content sequences.
    /// </summary>
    public IReadOnlyList<string> ParentsTags { get; set; } = [];

    /// <summary>
    /// Parent structure tag for parent marked content sequence.
    /// </summary>
    public string? ParentStructureTag { get; set; }

    /// <summary>
    /// True if parent struct element is part of structure tree.
    /// </summary>
    public bool IsTaggedContent { get; set; }

    /// <summary>
    /// Object key of parent struct element.
    /// </summary>
    public string? ParentStructureElementObjectKey { get; set; }

    /// <summary>
    /// Standard type of parent structure element.
    /// </summary>
    public string? ParentStandardTag { get; set; }

    /// <summary>
    /// ActualText value.
    /// </summary>
    public string? ActualText { get; set; }

    /// <summary>
    /// Alt value.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// True if item is inside signature widget appearance stream.
    /// </summary>
    public bool IsSignature { get; set; }

    /// <summary>
    /// True if item is inside Artifact structure element.
    /// </summary>
    public bool IsArtifact { get; set; }
}

/// <summary>
/// Grouped content sequence.
/// </summary>
public class SEGroupedContent : SEContentItem
{
    /// <summary>
    /// Content items within this sequence.
    /// </summary>
    public IReadOnlyList<SEContentItem> ContentItem { get; set; } = [];
}

/// <summary>
/// Simple content item.
/// </summary>
public class SESimpleContentItem : SEContentItem
{
    /// <summary>
    /// Item type ('text', 'image', 'lineart', 'shading').
    /// </summary>
    public string? ItemType { get; set; }

    /// <summary>
    /// True if item is within Formula structure element.
    /// </summary>
    public bool IsFormula { get; set; }

    /// <summary>
    /// True if item is within Figure structure element.
    /// </summary>
    public bool IsFigure { get; set; }
}

/// <summary>
/// Marked content sequence.
/// </summary>
public class SEMarkedContent : SEGroupedContent
{
    /// <summary>
    /// Tag associated with this marked content sequence.
    /// </summary>
    public string? Tag { get; set; }

    /// <summary>
    /// Value of the E entry.
    /// </summary>
    public string? E { get; set; }

    /// <summary>
    /// Value of the Lang entry.
    /// </summary>
    public string? Lang { get; set; }

    /// <summary>
    /// Lang value determined by inheritance rules.
    /// </summary>
    public string? InheritedLang { get; set; }
}

/// <summary>
/// Unmarked content sequence.
/// </summary>
public class SEUnmarkedContent : SEGroupedContent
{
}

/// <summary>
/// Graphic content item.
/// </summary>
public class SEGraphicContentItem : SESimpleContentItem
{
}

/// <summary>
/// Text content item.
/// </summary>
public class SETextItem : SEGraphicContentItem
{
    /// <summary>
    /// Font name.
    /// </summary>
    public string? FontName { get; set; }

    /// <summary>
    /// Scale factor.
    /// </summary>
    public decimal ScaleFactor { get; set; }

    /// <summary>
    /// Lang value.
    /// </summary>
    public string? Lang { get; set; }
}

/// <summary>
/// Lineart content item.
/// </summary>
public class SELineArtItem : SEGraphicContentItem
{
}

/// <summary>
/// Shading content item.
/// </summary>
public class SEShadingItem : SESimpleContentItem
{
}

/// <summary>
/// Image content item.
/// </summary>
public class SEImageItem : SEGraphicContentItem
{
}

/// <summary>
/// Inline image content item.
/// </summary>
public class SEInlineImageItem : SEImageItem
{
}

/// <summary>
/// Image XObject content item.
/// </summary>
public class SEImageXObjectItem : SEImageItem
{
}

/// <summary>
/// &lt;Form&gt; structure element.
/// </summary>
public class SEForm : PDStructElem
{
    /// <summary>
    /// Role attribute value.
    /// </summary>
    public string? RoleAttribute { get; set; }

    /// <summary>
    /// True if form has one interactive child.
    /// </summary>
    public bool HasOneInteractiveChild { get; set; }

    /// <summary>
    /// Number of widget annotations in this form element.
    /// </summary>
    public int WidgetAnnotsCount { get; set; }
}

/// <summary>
/// &lt;THead&gt; structure element.
/// </summary>
public class SETHead : PDStructElem
{
}

/// <summary>
/// &lt;TBody&gt; structure element.
/// </summary>
public class SETBody : PDStructElem
{
}

/// <summary>
/// &lt;TFoot&gt; structure element.
/// </summary>
public class SETFoot : PDStructElem
{
}

/// <summary>
/// &lt;Annot&gt; structure element.
/// </summary>
public class SEAnnot : PDStructElem
{
}

/// <summary>
/// &lt;Ruby&gt; structure element.
/// </summary>
public class SERuby : PDStructElem
{
}

/// <summary>
/// &lt;Warichu&gt; structure element.
/// </summary>
public class SEWarichu : PDStructElem
{
}

/// <summary>
/// &lt;RB&gt; structure element.
/// </summary>
public class SERB : PDStructElem
{
}

/// <summary>
/// &lt;RT&gt; structure element.
/// </summary>
public class SERT : PDStructElem
{
}

/// <summary>
/// &lt;RP&gt; structure element.
/// </summary>
public class SERP : PDStructElem
{
}

/// <summary>
/// &lt;WT&gt; structure element.
/// </summary>
public class SEWT : PDStructElem
{
}

/// <summary>
/// &lt;WP&gt; structure element.
/// </summary>
public class SEWP : PDStructElem
{
}

/// <summary>
/// &lt;DocumentFragment&gt; structure element.
/// </summary>
public class SEDocumentFragment : PDStructElem
{
}

/// <summary>
/// &lt;Aside&gt; structure element.
/// </summary>
public class SEAside : PDStructElem
{
}

/// <summary>
/// &lt;Title&gt; structure element.
/// </summary>
public class SETitle : PDStructElem
{
}

/// <summary>
/// &lt;FENote&gt; structure element.
/// </summary>
public class SEFENote : PDStructElem
{
    /// <summary>
    /// NoteType attribute value.
    /// </summary>
    public string? NoteType { get; set; }

    /// <summary>
    /// Object numbers missing in Ref entry.
    /// </summary>
    public string? OrphanRefs { get; set; }

    /// <summary>
    /// Object numbers present in Ref but not referencing this FENote.
    /// </summary>
    public string? GhostRefs { get; set; }
}

/// <summary>
/// &lt;Sub&gt; structure element.
/// </summary>
public class SESub : PDStructElem
{
}

/// <summary>
/// &lt;Em&gt; structure element.
/// </summary>
public class SEEm : PDStructElem
{
}

/// <summary>
/// &lt;Strong&gt; structure element.
/// </summary>
public class SEStrong : PDStructElem
{
}

/// <summary>
/// &lt;Artifact&gt; structure element.
/// </summary>
public class SEArtifact : PDStructElem
{
}

