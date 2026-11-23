# Phase 6: Fonts and Resources

Phase 6 adds font-resource metadata, CMap parsing, standard font metrics,
and bounded TrueType table parsing.

## Implemented components

- `PdfFont` and `PdfFontFactory`;
- Type1, TrueType, CID, Type0, and Type3 font subtype classification;
- `/Widths`, `/FirstChar`, `/BaseFont`, font descriptor, and bounding box
  metadata;
- resource lookup for named fonts;
- CMap code-space ranges;
- CMap CID character/range mappings;
- CMap `bfchar` and `bfrange` Unicode mappings;
- decoded `/ToUnicode` streams;
- embedded AFM standard-font metrics;
- TrueType header, `head`, `maxp`, `cmap` format 4, and `cmap` format 12
  parsing.

## Standard fonts

Standard 14 AFM files from `src/main/resources/font/stdmetrics` are linked
into the .NET core assembly as embedded resources. The metric service loads
Helvetica, Times, Courier, Symbol, ZapfDingbats, and their variants by name.

## CMaps

The CMap parser is focused on text CMap and `/ToUnicode` syntax. It supports
code-space ranges, CID character/range entries, Unicode character/range entries,
and `/WMode`.

Full predefined-CMap inheritance and every PostScript CMap operator are not
claimed in this phase.

## TrueType

The TrueType reader validates SFNT table offsets and parses glyph mapping data
from format 4 and format 12 `cmap` subtables. It also reads `UnitsPerEm` and
glyph-count metadata.

CFF, full OpenType layout, glyph outline, and Type1 charstring interpretation
remain later font-feature work.

## Font cache and safety

The standard-metrics service caches loaded AFM resources. CMap and font
objects are document/resource instances rather than global mutable document
state. Font byte ranges and CMap entry counts are checked before parsing.

## Tests

Phase 6 tests cover:

- CMap code-space and Unicode mappings;
- embedded Helvetica and bold metrics;
- TrueType format 4 `cmap` parsing;
- font resource lookup through the PD page model.
