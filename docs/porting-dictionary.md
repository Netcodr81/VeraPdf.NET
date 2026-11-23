# veraPDF Parser and PDF Porting Dictionary

This dictionary defines PDF terminology and veraPDF-specific concepts that are useful when
reading this library or porting it to C#/.NET. Terms are grouped by the layer in which they
are encountered. A name in backticks is a concrete Java type or PDF key used by this
repository.

## How to read this dictionary

Keep these relationships in mind:

1. A PDF file contains a byte-level syntax made of PDF objects.
2. Traditional cross-reference tables or cross-reference streams map indirect object keys to
   file offsets or compressed object-stream locations.
3. `COS*` classes represent the low-level PDF object model.
4. `PD*` classes interpret those `COS*` objects as document-domain concepts such as pages,
   fonts, annotations, and encryption.
5. `Reader`, `XRefReader`, and `PDFParser` bridge the file representation and the in-memory
   model.

In this library, "direct" and "indirect" usually refer to how a PDF object is stored, not to
whether an API returns a reference type.

## Core PDF file and syntax terms

| Term | Definition and porting relevance |
|---|---|
| **PDF object** | One of the basic values used to construct a PDF document. PDF objects are boolean, number, string, name, array, dictionary, stream, null, or an indirect reference. Objects combine into an object graph: dictionaries and arrays contain values directly or refer to indirect objects elsewhere in the file. |
| **PDF primitive object types** | The scalar PDF types: boolean (`true`/`false`), number (integer or real), string (literal or hexadecimal bytes), name (`/Type`), and null (`null`). These types are the syntax building blocks used inside arrays and dictionaries. |
| **Indirect reference** | A value of the form `n g R`, where `n` is an object number and `g` is a generation number. It points to an indirect object's value rather than containing that value inline. Resolve it through the cross-reference table and never treat `n g R` itself as the object value. |
| **PDF header** | The `%PDF-x.y` line near the beginning of a file. This parser records the header offset because a PDF can contain leading bytes before the marker. Do not assume the first byte is `%`. |
| **Header offset** | Number of bytes before the `%PDF-` marker. `PDFParser` adds this shift to object offsets obtained from xrefs. A C# port must preserve this adjustment. |
| **PDF version** | Major/minor syntax version such as 1.7 or 2.0. A catalog dictionary can also declare a `/Version` overriding the header. |
| **End-of-line (EOL)** | PDF allows CR, LF, or CRLF. Parsers must not treat line endings as ordinary whitespace when counting stream lengths or offsets. |
| **Whitespace** | PDF whitespace includes NUL, tab, LF, FF, CR, and space. Binary PDF streams are not text streams, so use byte-oriented parsing rather than `StreamReader`. |
| **Delimiter** | Character separating PDF lexical tokens: `()<>[]{}/%`. A delimiter normally ends a token without necessarily being data. |
| **Regular character** | Any PDF lexical character that is neither whitespace nor a delimiter. Token boundaries depend on this distinction. |
| **Token** | Small lexical unit returned by the parser, such as an integer, name, keyword, string, dictionary delimiter, or array delimiter. In .NET this is commonly a readonly record or struct with a token kind and typed value. |
| **Keyword** | Bare syntax word such as `obj`, `endobj`, `stream`, `endstream`, `xref`, `trailer`, `startxref`, `true`, `false`, or `null`. Keywords are bytes and are case-sensitive. |
| **Comment** | Content from `%` through EOL. The special `%PDF-` header and `%%EOF` marker are comments-shaped markers with structural meaning. |
| **EOF marker** | `%%EOF`, used to locate the end of a PDF revision. A valid file may contain garbage after it, and an incremental PDF has multiple EOF markers. |
| **`startxref`** | Keyword followed by the byte offset of the most recent cross-reference section or stream. The reader searches near the end of the file for the final valid occurrence. |
| **Byte offset** | Absolute or revision-relative location in the source file. `long` is safer than `int`; large or prepended PDFs can exceed smaller address spaces. In .NET use `long` for positions. |
| **Random access** | Seeking to offsets when resolving objects. PDF parsing therefore normally requires a seekable stream or a buffering/spooling layer around a non-seekable input. |
| **Incremental update** | A new body, xref, trailer, and EOF appended to an existing PDF. The newest revision takes precedence, while older objects remain available unless replaced or freed. |
| **Revision** | One incremental layer of a PDF file. veraPDF stores multiple xref/trailer records so first and last revisions can be distinguished. |
| **Linearized PDF** | A PDF arranged for efficient progressive display. Its special linearization dictionary must appear early in the file and can change the reading order, but does not replace normal xref resolution. |
| **Hybrid-reference PDF** | A PDF using an xref stream while retaining supplemental traditional xref table data. A port should not assume that a file has only one xref representation. |

## COS: low-level object model

**COS** means *Carousel Object System*, a name historically used for PDF's low-level object
model. The `org.verapdf.cos` package is the syntax-level model: it knows PDF arrays,
dictionaries, streams, and references but does not know what it means to be a page or font.

The PDF object model is a graph, not a list of application-defined classes. A page dictionary
can contain direct values and can refer by indirect reference to resources, content streams,
and fonts:

```text
4 0 obj
  << /Type /Page /Contents 5 0 R /Resources 6 0 R >>
endobj
```

Resolving object `4 0` yields the page dictionary. Resolving `/Contents 5 0 R` retrieves the
object identified by `5 0`; resolving `/Resources 6 0 R` retrieves another object. A C# port
should keep the distinction between a key/reference, the resolved value, and its document-domain
interpretation in classes such as `PDPage`.

| Term / type | Definition and porting relevance |
|---|---|
| **COS object** | The Java/COS representation of a PDF object. COS values include booleans, numbers, strings, names, null, arrays, dictionaries, streams, and indirect references. `COSObject` represents the value together with an optional indirect key, while concrete `COS*` types represent the value itself. |
| **`COSObject`** | Wrapper that carries a PDF value and, for an indirect object, its key. It is a natural sum-like type in C#: a discriminated union or base class with typed derived models is usually safer than `object`. |
| **`COSBase`** | Base for COS value implementations. In .NET, use an abstract `CosValue` hierarchy; preserve PDF type identity and direct/indirect distinction. |
| **Direct object** | Object value written inline in another object. In PDF, a direct array/dictionary can still contain references to indirect objects. |
| **Indirect object** | Object stored in the body as `n g obj ... endobj` and addressable by object number and generation. Its identity and storage location are distinct from its value. |
| **`COSIndirect`** | COS representation of an indirect reference. A C# port should use an immutable value or record carrying object number and generation. |
| **`COSKey`** | Object identity made from an integer object number and generation. It is not a file offset. Use value equality and a stable hash code in C#. |
| **Object number** | Positive integer identifying an indirect object within a PDF revision. Reuse is governed by xref generations and free/in-use state. |
| **Generation number** | Integer distinguishing reuses of an indirect object number. Compressed objects inside object streams have generation 0. |
| **`COSDirect`** | Common base/marker for directly represented objects. A C# hierarchy should avoid exposing Java-specific mutability assumptions if values can be immutable. |
| **`COSNull`** | PDF null value written as `null`. Keep it distinct from a missing dictionary key and distinct from C# `null`; a PDF dictionary can explicitly contain null. |
| **`COSBoolean`** | PDF `true` or `false`. A `bool?` is often useful at PD accessor boundaries: `null` means absent, while `false` is a real PDF value. |
| **`COSNumber`** | Base for PDF integers and reals. Preserve distinction because PDF integers may have semantics or ranges not appropriate for floating point. |
| **`COSInteger`** | Signed integer token. Use `long`/`Int64` for file offsets and parser safety even where individual spec values are smaller. |
| **`COSReal`** | Decimal/fixed-point or real PDF number. Parsing should preserve the accepted PDF lexical range and avoid locale-dependent conversion; use invariant culture in .NET. |
| **`COSName`** | Atomic PDF name beginning with `/`. Names may contain `#xx` escape sequences and are not .NET strings by convention alone. Decode PDF escapes exactly once. |
| **`ASAtom`** | Interned/constant name-like atom used by this implementation instead of repeatedly allocating common PDF name strings. A C# port can use frozen singleton strings, enums, or an interning table. |
| **`COSString`** | PDF string in literal parentheses or hexadecimal angle brackets. It is bytes, not necessarily UTF-16 text. Decoding depends on PDFDocEncoding or PDF Unicode rules at the semantic layer. |
| **Literal string** | PDF string in parentheses. Supports balanced parentheses, escapes, octal escapes, and line continuations. Preserve byte values until a documented text encoding step. |
| **Hexadecimal string** | String in `<...>` syntax. Odd-length values are padded with a trailing zero nibble. This is binary PDF syntax, not necessarily printable hexadecimal. |
| **`COSArray`** | Ordered sequence of PDF objects. Maps naturally to `IReadOnlyList<CosValue>` when immutable, or `IList<CosValue>` when exposing mutation. |
| **`COSDictionary`** | Name-keyed collection of PDF objects. Maps to `Dictionary<string, CosValue>` internally; distinguish absent keys, explicit null values, and invalid types. |
| **Dictionary entry** | A `/Key value` pair. PDF names are the keys; values can be direct, indirect, or null. Most PD properties are typed accessors over these entries. |
| **`COSStream`** | PDF dictionary followed by binary stream bytes and `endstream`. It has dictionary entries as metadata plus raw/decoded data and therefore is not just a `Stream`. |
| **Stream length** | Value of `/Length`, specifying the raw stored byte count in a PDF syntax stream. It may itself be indirect. Off-by-one handling around EOL after `stream` is security- and correctness-sensitive. |
| **Raw stream data** | Bytes stored in the file after applying no decoding. Encoded data can be compressed or encrypted. |
| **Decoded stream data** | Raw stream bytes with its `/Filter` chain and predictor processing undone and with encryption removed at the appropriate layer. |
| **`FilterFlags`** | `COSStream` mode selecting raw, decoded, or related data access. A .NET port should use explicit methods or an enum rather than a stateful flag that callers can confuse. |
| **`COSDocument`** | Owns document-level state: reader, indirect objects, resources, and encryption/security integration. In C# this is the logical owner for stream/disposable lifetimes. |
| **`COSHeader`** | Parsed `%PDF-` version plus header offset. |
| **`COSTrailer`** | Trailer dictionary state such as `/Size`, `/Root`, `/Encrypt`, `/Info`, `/ID`, and optional `/Prev` or xref-stream metadata. |
| **`COSBody`** | Representation of PDF body indirect-object content used around parsing or writing. |
| **`COSObjType`** | Enum naming the runtime COS value kind. Pattern matching over a C# class hierarchy can replace many type-switches. |
| **Visitor** | Object that dispatches behavior by COS object type. `ICOSVisitor`/`IVisitor` can map to the Visitor pattern, or to C# pattern matching if the operation is local. |
| **Copier** | Visitor/utility that duplicates a COS graph. A port must decide whether to copy streams, preserve object identities, and remap indirect keys. |
| **Writer** | Visitor/serializer that emits COS values and indirect objects in PDF syntax. Exact byte layout is needed when writing xref offsets. |
| **Object graph cycle** | Indirect references can create cycles. Traversal, copying, equality, or serialization must track visited keys to avoid recursion. |
| **Reference resolution** | Turning `COSIndirect`/`COSKey` into an actual object. Resolution can require seeking, parsing, decryption, and decoding, so typed `Task` APIs may be tempting but must be end-to-end asynchronous. |

## Cross-reference terms

| Term / type | Definition and porting relevance |
|---|---|
| **Cross-reference (xref)** | Index from indirect object keys to storage locations. It is the file's mechanism for resolving indirect references. |
| **Traditional xref table** | Text sections introduced by `xref`, with ranges of 10-digit offsets, 5-digit generation numbers, and `n`/`f` status. |
| **Xref stream** | A PDF stream containing compressed xref entries plus a trailer dictionary. It replaces a traditional xref section in many newer or compact PDFs. |
| **`COSXRefEntry`** | One xref entry. It needs more than an offset: it also distinguishes in-use, free, and compressed-in-object-stream entries. |
| **`COSXRefSection`** | One set/range collection in a cross-reference structure. |
| **`COSXRefRange`** | Contiguous object-number subsection. A C# immutable range record can represent it clearly. |
| **`COSXRefTable`** | Aggregated xref sections/entries. |
| **`COSXRefInfo`** | Bundle connecting one xref location, section, and trailer. Preserve revision boundaries when merging. |
| **`COSXRefTableReader`** | In-memory lookup over merged xref information. Maps `COSKey` to a `long` encoded location. |
| **In-use entry (`n`)** | Entry resolving to an indirect object stored at a file offset. |
| **Free entry (`f`)** | Available or deleted object slot. In this reader, a zero/free location can produce an empty `COSObject`; callers must not confuse it with a successfully parsed null object. |
| **Compressed entry** | Entry inside an object stream rather than at a direct file offset. The entry identifies the containing stream object and the compressed object's stream index. |
| **Negative offset convention** | This repository uses negative values for compressed entries: object stream number is encoded as `-offset`. This is an internal sentinel convention, not PDF syntax. A .NET port can replace it with a discriminated union. |
| **Positive offset** | Direct location of an indirect object's `n g obj` syntax. The header offset may need to be added before seeking. |
| **Zero offset** | Free/non-resolvable location in this reader. Do not generally seek to byte zero and parse it as an object. |
| **`/Prev`** | Trailer key identifying the previous xref location in an incrementally updated PDF. Traverse it carefully and detect loops. |
| **`/XRefStm`** | Trailer reference to an xref stream in a hybrid-reference PDF. Traditional trailer/xref handling may need to combine entries from both sources. |
| **`/Size`** | Highest object number plus one for a PDF/xref revision, not the file length and not necessarily the total live-object count. |
| **First trailer / last trailer** | Trailer for the earliest or latest revision. Their ordering matters for encryption, IDs, catalog revisions, and diagnostics. |
| **`XRefReader`** | Abstract reader that combines xref lookup with reader interface methods. In C#, split xref storage from I/O orchestration or retain as an abstract base implementing `IPdfReader`. |
| **`IReader`** | Contract for resolving source data, objects, xrefs, trailers, and encryption checks. It maps well to `IPdfReader : IDisposable` plus a separate writer/parser service. |

## Streams, filters, and binary data

| Term / type | Definition and porting relevance |
|---|---|
| **Stream dictionary** | Dictionary preceding binary stream data. It describes length, filters, decoding parameters, color/image dimensions, and other stream-specific metadata. |
| **Object stream** | Compressed collection of indirect objects. It is distinct from a content stream and is parsed by `DecodedObjectStreamParser`. Its `/N` and `/First` entries drive decoding. |
| **`/N` (object stream)** | Number of compressed objects in an object stream. Validate bounds before allocating. |
| **`/First` (object stream)** | Byte offset within decoded object-stream data where object bodies begin, after the object number/index header table. |
| **Content stream** | Stream containing page or form drawing operators. It is interpreted as PDF graphics-language instructions, not a generic text stream. |
| **`ASInputStream` / `SeekableInputStream`** | veraPDF input abstractions. `.NET` equivalents are `Stream`, `MemoryStream`, `FileStream`, and possibly a bounded seekable wrapper for spooled non-seekable input. |
| **Peek** | Read the next byte without consuming it. A buffered parser may implement this with `ReadByte` plus seek, or internal buffer lookahead. |
| **Unread** | Move backward after lookahead. Ensure the stream can seek before relying on this operation. |
| **Spooling** | Copying a non-seekable stream into memory or a temporary file to support random access. Set memory thresholds and disposal ownership explicitly. |
| **Resource users** | Reference counter-like mechanism around reusable streams/resources. In C#, prefer explicit ownership, `IDisposable`, and `MemoryStream` copies over manual counters unless sharing is required. |
| **`/Filter`** | One filter name or array of filter names to apply to raw stream bytes. Filter order is pipeline order: decode in the declared sequence. |
| **`/DecodeParms`** | Parameter dictionary or array accompanying filters. It must align with the filter list. A missing `DecodeParms` item is not equivalent to a malformed filter definition. |
| **Flate** | zlib/deflate compression represented by `FlateDecode`/`FlateEncode`. .NET compression APIs need care with zlib headers versus raw DEFLATE. |
| **ASCIIHex** | Filter using hexadecimal ASCII data, often terminated by `>`. Odd nibbles and whitespace require PDF-specific handling. |
| **ASCII85** | Base-85-like PDF encoding using `~>` termination. Do not replace it with an unrelated generic base-85 implementation without testing PDF edge cases. |
| **LZW** | Lempel-Ziv-Welch PDF stream filter. Its PDF code-width and clear/end-code behavior is format-specific. |
| **RunLength** | PackBits-like byte stream compression. Preserve 128-byte copy/repeat semantics and end marker. |
| **Predictor** | Post-decompression byte transform, commonly used for images: TIFF predictor or PNG row filters. Predictors act after decompression. |
| **Crypt filter** | Stream decryption strategy selected through `/Crypt` and the encryption dictionary. It is part of the decoding/decryption pipeline, not merely stream compression. |
| **Decompression bomb** | Small encoded input expanding beyond a safe memory/disk limit. A production .NET port needs bounded output, ratio/absolute caps, cancellation, and explicit errors. |
| **Binary-safe text** | File/token parsing that preserves every byte. Never use locale, newline normalization, or `Encoding.UTF8` indiscriminately on PDF bytes. |

## Parser and reader terms in this repository

| Term / type | Definition and porting relevance |
|---|---|
| **`PDFParser`** | PDF-level random-access parser responsible for header parsing, revision discovery, traditional and stream xrefs, trailers, linearization detection, encryption metadata, and indirect-object lookup by byte offset. Java type: `org.verapdf.parser.PDFParser`, extending `SeekableCOSParser`. C# equivalent: a `PdfParser : SeekableCosParser, IDisposable` class, or preferably a `PdfParser` service composed with `IPdfLexer`, `ICosParser`, and `ISeekableStream`; keep `PdfReader`/xref orchestration separate if it improves ownership and testability. |
| **`PDFParser` constructors** | Java exposes filename and `Stream` overloads, both with and without a caller-supplied `COSDocument`. C# equivalents should use `Stream` plus explicit ownership: `PdfParser(Stream source, CosDocument document, bool leaveOpen)` or a static `CreateAsync` only if I/O becomes genuinely asynchronous. A path overload can open `FileStream` internally and set `leaveOpen: false`; do not assume a caller's stream should remain open. Use `ArgumentNullException` for missing sources and structured `InvalidDataException`/`PdfParseException` derivatives for malformed PDFs. |
| **`PDFParser.getHeader()`** | Parses `%PDF-x.y`, leading junk, the binary comment, and the header offset. C# equivalent: `ValueTask<CosHeader> GetHeaderAsync(...)` only for an async design; for a direct port use `CosHeader GetHeader()`. Search the first lines defensively, preserve the full `HeaderOffset` as `long`, and rewind to stream position zero afterward. |
| **`PDFParser.isLinearized()` / `getLinearizationDictionary()`** | Checks for the special linearization dictionary in the first 1024 bytes and validates `/L` against the source length. C# equivalents: `bool IsLinearized()` and `CosObject? GetLinearizationDictionary()`. Bound the search, preserve `/L` as a PDF integer, and report I/O failures separately from "not linearized"; do not convert every parse failure to `false` in diagnostics. |
| **`PDFParser.getObject(long)`** | Seeks to an indirect-object offset, parses the object number, generation, `obj`, value, and final `endobj`, and may decrypt a stream. C# equivalent: `CosObject GetObject(long offset)`, with a seekable `Stream` and a position/type-safe parser. Return a null-equivalent only as the implementation contract requires, never `null` for an explicit PDF `null`. Preserve header/EOL compliance flags and the current `COSKey` for string/stream decryption. |
| **`PDFParser.getXRefInfo(List<COSXRefInfo>)`** | Discovers the final `startxref`, follows `/Prev` and `/XRefStm`, merges traditional tables and xref streams, detects loops, and records revisions. C# equivalent: `IReadOnlyList<CosXRefInfo> GetXRefInfo()` or `void GetXRefInfo(ICollection<CosXRefInfo>)`. Use `HashSet<long>` for visited offsets, a stack for `/Prev`, `Stack<T>` or iterative discovery, and a loop exception when offsets repeat. |
| **`PDFParser.findLastXRef()`** | Searches backward for the final valid `startxref` within the parser's bounded 1024-byte tail search and then reads the integer offset. C# equivalent: `long? FindLastXRef()` backed by `Stream.Seek`/`ReadExactly`. Search bytes, not normalized text, and treat `startxref` with no valid integer or no `%%EOF` context as a structured parse failure. |
| **`PDFParser` trailer and encryption state** | Tracks the last trailer offset, last xref stream, `/Encrypt`, and whether the file is encrypted. C# equivalents: `CosObject? Encryption`, `CosObject? LastXRefStream`, `long LastTrailerOffset`, `bool IsEncrypted`, plus `IsContainsXRefStream`. Preserve missing/invalid encryption metadata as diagnostics rather than silently treating the PDF as unencrypted. |
| **`PDFParser` xref-table parsing** | Reads subsection headers and 20-byte entry forms, including offsets, generations, and `n`/`f` state. C# equivalent: `CosXRefSection ParseXrefTable(...)`. Use `Span<byte>`/`ReadOnlySpan<byte>` for bounded parsing where practical, but keep entry offsets as `long`; validate malformed spacing and row lengths and emit PDF/A diagnostics rather than throwing on recoverable formatting issues. |
| **`PDFParser` xref-stream parsing** | Parses an indirect-object stream at a `startxref`/`/Prev` location and extracts entries and trailer dictionary data. C# equivalent: `ParseXrefStream(CosXRefInfo section, bool isLastTrailer)`. Reuse the COS stream decoder and `/W`, `/Index`, `/Size`, and `/Prev` parsing; keep direct file entries, free entries, and object-stream entries as typed xref-entry kinds rather than a signed-offset convention. |
| **`SeekableCOSParser`** | Parser base that combines COS lexical/object parsing with seekable source access. A .NET port can use a composition of `PdfLexer`, `CosParser`, and seekable `Stream`. |
| **`BaseParserInputStream`** | Contract for parser stream operations such as offsets/lookahead. Document it as a byte-oriented parser stream boundary rather than exposing arbitrary callers. |
| **`DecodedObjectStreamParser`** | Parser of an already-decoded object stream. It builds an internal map of object numbers to offsets and parses one selected compressed object at a time. |
| **`Reader.getObject(COSKey)`** | Primary semantic operation: resolve a key through the xref map, parse a direct-offset object, or lazily parse an object stream. |
| **Object-stream cache** | `Reader` caches a `DecodedObjectStreamParser` by containing stream number. This avoids rebuilding its internal offset table. The parser is positionful; concurrent use requires synchronization. |
| **`Reader.close()`** | Releases cached decoded object-stream parsers in this implementation. Ownership of the underlying PDF source is managed across the parser/document layers, so a port should define one clear owner. |
| **Header-shifted offset** | Xref-derived offset adjusted by the recorded header offset. A parser of a source containing leading bytes must apply this consistently but not twice. |
| **`offsetShift`** | `PDFParser` state reflecting bytes before the PDF header. |
| **Linearization dictionary lookup** | Search in the first portion of the file for the special first indirect linearization object. Keep the lookup bounded, because it is not a general object search. |
| **Last xref stream** | Most recent xref stream found while reading revisions. It is tracked separately so callers can distinguish stream-based references from tables. |
| **Last trailer offset** | Recorded position of the final trailer or xref-related structure. A zero value is ambiguous and should be logged/validated rather than treated as a valid object offset without context. |
| **Parser state** | Current source position, flags, token state, current object key, and temporary parse objects. Do not share parser instances concurrently unless the implementation is redesigned around independent immutable jobs. |
| **Malformed-but-recoverable PDF** | File violating the specification but parseable with heuristics. Record warnings/diagnostics separately from strict validation errors and avoid silently changing object semantics. |

## PD: document-level object model

**PD** classes wrap COS dictionaries and other values as PDF domain concepts. They generally
do not replace the COS layer. A PD wrapper is therefore best ported as a typed facade with
nullable properties and explicit validation.

| Term / type | Definition and porting relevance |
|---|---|
| **`PDObject`** | Base wrapper around a `COSObject`. The C# design should preserve the wrapper's access to original raw values for diagnostics and serialization. |
| **`PDDocument`** | High-level document API over a `COSDocument`. Maps naturally to a disposable `PdfDocument` that owns parsing/session state. |
| **Catalog** | Root document dictionary referenced by trailer `/Root`. `PDCatalog` exposes page tree, names, structure, forms, output intents, optional content, and other document-wide features. |
| **`PDCatalog`** | Typed facade over the catalog. Most property lookup should resolve inherited/default values carefully rather than assuming direct keys. |
| **Page tree** | Hierarchical tree of intermediate page-tree nodes and leaf page dictionaries. Traverse iteratively for hostile files to avoid stack overflows. |
| **`PDPageTree` / `PDPageTreeNode`** | Page-tree facade and node wrapper. `/Kids`, `/Count`, and `/Parent` may be malformed or cyclic in untrusted PDFs. |
| **`PDPage`** | One page dictionary, commonly exposing media/crop boxes, resources, rotation, contents, and annotations. |
| **Inherited page attribute** | Page-tree attributes that a page can inherit from ancestors, such as resources, media box, crop box, and rotation. A property getter may need to walk upward. |
| **`PDResource` / `PDResources`** | Wrapper(s) around resource dictionaries such as fonts, XObjects, ext graphics states, color spaces, patterns, and shadings. |
| **Resource category** | Named dictionary under a page/resources dictionary, for example `/Font`, `/XObject`, `/ColorSpace`, or `/ExtGState`. Resource names are local identifiers, not globally unique. |
| **Page content stream** | One or many streams under `/Contents` providing page drawing commands. |
| **`PDContentStream`** | High-level stream wrapper used where PDF content operators may be parsed. |
| **XObject** | Reusable PDF object such as image, form, or PostScript object. A form XObject has its own resources/content stream and nested coordinate system. |
| **`PDXObject` / `PDXForm` / `PDXImage`** | Typed XObject base, form XObject, and image XObject. Keep subtype checks separate from stream decoding. |
| **Annotation** | Interactive or markup object referenced by a page. It may be a widget, link, appearance, file attachment, 3D view, or other subtype. |
| **`PDAnnotation`** | Base typed annotation wrapper. Optional fields and subtype-specific dictionaries make nullable, validated properties important. |
| **Appearance stream** | Form XObject defining how an annotation, form field, or widget looks. Appearance state may select among multiple streams. |
| **Widget** | Annotation representing an interactive form field on a page. |
| **AcroForm** | Interactive form dictionary referenced by `/AcroForm`. It has fields, default resources, calculation order, and signature-related structures. |
| **Form field** | Interactive field dictionary, possibly nonterminal. Inheritance from parent fields is important for field type, value, flags, and default value. |
| **Name tree** | Balanced tree mapping names/strings to values. It uses `/Names` leaf arrays and `/Kids` child nodes rather than a flat dictionary. |
| **Number tree** | Balanced tree mapping integers to values using `/Nums` arrays and `/Kids`. |
| **Destination** | Named or explicit target location/document view, represented by names, strings, arrays, or dictionary forms. |
| **Outline** | Document bookmark hierarchy. Preserve sibling links and parent-child traversal invariants, and protect against malformed cycles. |
| **`PDOutlineItem`** | One bookmark node, commonly linked via `/Next`, `/First`, `/Last`, `/Parent`, and `/Prev`. |
| **Action** | Executable/navigation behavior from annotations, outlines, catalog, or document events. A parser should inspect action dictionaries without treating them as executable trusted code. |
| **Additional actions** | Event-triggered action dictionaries for annotations, pages, fields, document/catalog, or widgets. |
| **Optional content / OCG** | Optional-content group (layer) and state machinery used to show/hide content. |
| **OCMD** | Optional-content membership dictionary combining groups with a visibility expression. |
| **Structure tree** | Tagged-PDF hierarchy defining logical reading structure. It maps independently from visual page content and page coordinates. |
| **Struct element** | Logical document node such as heading, paragraph, table, figure, or artifact mapping. |
| **Marked-content reference (MCR)** | Entry linking marked content in a content stream to a structure element. |
| **Object reference (OBJR)** | Structure-tree dictionary pointing to a PDF object such as an annotation. |
| **Namespace role mapping** | Mapping between structure types and roles under tagged-PDF namespace handling. |
| **Metadata stream** | XMP metadata stored as a PDF stream. Preserve raw XML bytes and validate encoding/XML separately from PDF stream decoding. |
| **Output intent** | Dictionary describing intended output condition, commonly including an ICC profile. Used for color/PDF-X/PDF-A validation. |
| **Embedded file** | File carried in PDF, usually via an embedded-file stream, filespec, or name tree. Treat extracted bytes as untrusted. |

## Fonts and text

| Term | Definition and porting relevance |
|---|---|
| **Simple font** | Font mapping one-byte character codes directly through an encoding. Includes Type 1, TrueType, and some related classes. |
| **Composite font** | Font, such as Type 0, taking multi-byte or CID character codes and mapping them through a CMap to descendant fonts. |
| **Type 0 font** | Composite PDF font referencing a CMap and CID descendant font. |
| **CID font** | Character Identifier font, where glyph access is based on CIDs rather than Unicode directly. |
| **CID** | Character Identifier used by CID-keyed fonts. It may be mapped to Unicode by ToUnicode and to a GID for font rendering. |
| **GID** | Glyph index in the font program. CID-to-GID mapping selects the actual glyph for a TrueType-based CID font. |
| **Font descriptor** | Dictionary describing metrics and font flags and potentially referencing embedded font programs. |
| **Font program** | Embedded or standard font binary, such as Type 1, CFF, OpenType, or TrueType. Parsing is byte-oriented and independent from PDF text encoding. |
| **Encoding** | Mapping character codes in content streams to glyph names/other font semantics. PDF has predefined encodings, Differences arrays, and special font-defined behavior. |
| **Differences array** | Encoding array that overrides character-code to glyph-name mappings at specific positions. |
| **CMap** | Map from encoded character bytes to CIDs, byte sequences, or Unicode. CMap files have their own PostScript-like parser and can vary by writing mode. |
| **ToUnicode CMap** | CMap stream mapping character codes/CIDs to Unicode text. It is for text extraction and not sufficient by itself to draw the glyph. |
| **Codespace** | Range(s) defining how many bytes belong to one character code for a CMap. Overlapping/invalid ranges must be validated. |
| **CIDToGIDMap** | Name/stream mapping CIDs to TrueType glyph IDs. Identity mapping may be implied by `/Identity`. |
| **Widths (`/W`, `/DW`, `/W2`, `/DW2`)** | PDF dictionary values defining glyph advance metrics for CID fonts, horizontal or vertical. These are parsing targets rather than values to infer from Unicode. |
| **Type 1 font** | PostScript outline font format. This library contains CharString parsing and eexec decryption-like decoding needed for font program interpretation. |
| **CharString** | Type 1/CFF compact graphics bytecode describing a glyph. It is stack-oriented and distinct from PDF content-stream operators. |
| **CFF** | Compact Font Format used in OpenType/PostScript fonts. Indexes, DICT operands, private dictionaries, and CharStrings must be parsed with format-specific rules. |
| **TrueType** | Outline font with directory tables (`head`, `hhea`, `hmtx`, `maxp`, `cmap`, `post`, and others). Table offsets are relative to the font binary. |
| **OpenType** | Font container capable of holding TrueType or CFF outlines. `.NET` drawing libraries may parse it for rendering, but PDF semantic validation still needs its own metric/model handling. |
| **Type 3 font** | PDF-defined font whose glyphs are executable PDF graphics content streams (`CharProc`s), not an external font binary. |
| **CharProc** | Character procedure for a Type 3 glyph. It is a PDF graphics stream with a font-specific coordinate system. |
| **Standard 14 fonts** | Historically predefined PDF fonts. A validator cannot always expect an embedded font program or modern OS font substitute. |
| **Subset name/tag** | Six uppercase letters plus `+` prefix commonly added to embedded font names. Font caching must normalize or preserve it according to the comparison being performed. |
| **Symbolic flag** | Font descriptor flag indicating a symbolic/custom character encoding. It changes mapping behavior; do not infer encoding only from the font program. |
| **Text showing operator** | Content operator such as `Tj`, `TJ`, `'`, or `"` that displays encoded font character codes. It operates on bytes, followed by mapping through the active font. |
| **Text rendering vs extraction** | Rendering converts glyph programs and matrices into pixels/outlines; extraction maps codes to semantic Unicode. They are related but require different data paths. |

## Colors, graphics, and functions

| Term | Definition and porting relevance |
|---|---|
| **Color space** | Domain mapping component values to color. DeviceGray, DeviceRGB, DeviceCMYK, CIE-based, ICC, Indexed, Separation, DeviceN, Lab, and pattern spaces have different component counts and semantics. |
| **Color component count** | Number of values required for a color in a space. Verify array lengths rather than assuming RGB. |
| **Device color space** | Device-dependent Gray/RGB/CMYK space. Actual appearance depends on output device/color management. |
| **CIE-based color space** | CalGray/CalRGB/Lab and related spaces defined by colorimetric dictionaries and white points. |
| **ICC-based color space** | Color space backed by an ICC profile stream. Treat the embedded profile parser as an external binary format boundary. |
| **Indexed color space** | Color values are integer indexes into a lookup table. The high value and lookup byte length constrain legal indexes. |
| **Separation** | Single-component special color space using a colorant name and tint transform. |
| **DeviceN** | Multiple-component special color space with named colorants. Alternate-space/tint-transform fallback matters. |
| **Pattern color space** | Tiling or shading pattern used as paint instead of ordinary component values. |
| **Graphics state** | Parameters affecting drawing, including line width, dash, matrices, color, opacity, blend mode, transfer functions, and clipping. |
| **Extended graphics state (`ExtGState`)** | Resource dictionary carrying advanced graphics settings. `PDExtGState` is a typed facade over it. |
| **Soft mask** | Transparency mask carried in graphics state, often referring to a form XObject/group. |
| **Transparency group** | Form XObject or page marked with a group dictionary, isolating compositing behavior. |
| **Shading** | Continuous color function across a geometric domain. Types include function, mesh, and other PDF-defined forms. |
| **PDF function** | Maps input sample(s) to output sample(s). Types 0, 2, 3, and 4 differ in representation and must not be modeled as a generic .NET delegate only. |
| **Type 0 function** | Sampled lookup function storing interpolation samples in a stream. |
| **Type 2 function** | Exponential interpolation function. |
| **Type 3 function** | Stitching of subfunctions across domain ranges. |
| **Type 4 function** | PostScript calculator function represented as a restricted expression/operator program. It is interpreted, never compiled/executed as trusted host code. |
| **Content operator** | Instruction in page/form content streams, such as `q`, `Q`, `cm`, `BT`, `Tj`, or `Do`. Operators consume operands from a PDF operand stack. |
| **Inline image** | Image embedded between `BI`, `ID`, and `EI` inside a content stream rather than stored as a named XObject. Stream delimiters are context-sensitive binary data. |

## Encryption, security, and resource handling

| Term | Definition and porting relevance |
|---|---|
| **Encryption dictionary** | PDF `/Encrypt` dictionary describing security handler, revision/length, permissions, password validation values, and crypt filters. It contains policy/derivation inputs, not a simple password. |
| **Standard security handler** | PDF's traditional password/security mechanism. `StandardSecurityHandler` validates passwords and derives/decrypts with PDF-standard algorithms. |
| **Security handler filter** | `/Filter` identifying the encryption scheme, e.g. standard handler. Unsupported filters should cause an explicit unsupported-policy result, not be guessed. |
| **User password** | Opens the document with permitted access. It is not necessarily recoverable from the encryption dictionary. |
| **Owner password** | Administrative password used for full access and to derive owner-related authorization data. |
| **Encryption key** | Derived byte key used for PDF strings/streams. Protect it as sensitive in-memory state and zero temporary buffers where feasible. |
| **Permissions (`/P`)** | Signed/bitfield value defining allowed operations. Preserve signed 32-bit PDF semantics in Java/.NET; do not shift as unsigned without a deliberate conversion. |
| **Revision (`/R`)** | Security-handler algorithm revision, which selects password derivation and cryptographic behavior. |
| **`/V`** | Encryption algorithm version. It is separate from the PDF file version and separate from security handler revision. |
| **Crypt filter method** | Algorithm selector for object strings/streams, e.g. none/identity, RC4, or AES. |
| **Object key derivation** | Per-indirect-object cryptographic key derivation that includes the document key, object number, and often generation. Reusing a single raw key for all streams is incorrect. |
| **Trailer `/ID`** | Array of document identifiers used in several processes, notably encryption key derivation. Distinguish the first and second identifiers; do not replace with a generated GUID. |
| **AES IV** | Initialization vector prefixed to encrypted PDF stream/string data in applicable AES revisions. PDF storage framing is format-specific. |
| **RC4** | Legacy PDF encryption algorithm. A .NET port must use or implement a vetted compatibility path if legacy PDFs are required; modern `System.Security.Cryptography` APIs may not expose it. |
| **Padding** | PDF standard-handler password/key padding uses a specified 32-byte constant plus truncation/repeat rules. It is not PKCS#7 message padding. |
| **Suppressed exception** | Java mechanism for retaining cleanup failures while throwing a primary exception. In .NET, use `AggregateException`, nested `Exception.Data`, or an explicit cleanup policy without overwriting the password failure. |
| **Resource limit** | Bound on decoded/spooled data, nesting, tokens, xref count, or bytes. Required for untrusted input to resist memory, disk, CPU, and recursion abuse. |
| **Ownership (`ownsStream`)** | Whether an object should dispose a stream passed by a caller. Port overloads should make this clear with `leaveOpen` or ownership parameters. |
| **Object disposed state** | State after `Dispose`. A public C# wrapper should throw `ObjectDisposedException` consistently rather than producing parser-state-dependent failures. |

## PDF semantic and validation concepts

| Term | Definition and porting relevance |
|---|---|
| **PDF/A** | ISO PDF subset/profile for archival use, covering embedding, metadata, color, encryption, and other conformance requirements. veraPDF validates these constraints above the syntax parser. |
| **Validation** | Checking a parsed model against a conformance profile/specification. It differs from parsing: a PDF can be parseable but nonconforming. |
| **Cosmetic vs structural error** | Whether an issue changes rendered/semantic behavior. A parser should preserve evidence and severity rather than silently normalizing during parse. |
| **Feature model** | Extracted document characteristics consumed by validators/reporters. Keep parsed raw values available when reporting type/encoding failures. |
| **Parser warning** | Nonfatal diagnostic about malformed syntax or recovery behavior. A port should offer structured diagnostics rather than logging only. |
| **Default value** | Spec-defined value used when a dictionary key is absent. Track defaulting separately from a key explicitly stored with that value. |
| **Inheritable attribute** | Page/page-tree property resolved upward. Missing at one node is not automatically missing from the resulting page. |
| **Malformed cycle** | Circular page trees, outlines, dictionaries, references, or structure nodes. Add visited-key sets and traversal caps. |
| **Type coercion** | Converting raw PDF values to a PD property. Prefer validation/error paths over permissive `Convert.ToX` behavior, because PDF semantics often require a specific array/dictionary/number form. |
| **Resolver** | Service converting an indirect key/ref to a concrete COS object. Dependency-injecting it makes PD wrappers easier to test in C#. |

## Java-to-C#/.NET mapping

| Java / veraPDF concept | Practical C#/.NET mapping | Important caveat |
|---|---|---|
| `InputStream` | `System.IO.Stream` | Base `Stream` is not necessarily seekable. |
| `SeekableInputStream` | Seekable `Stream` wrapper/contract | Require `CanSeek`, use `Position`/`Length`, and define synchronization. |
| `Closeable`, `AutoCloseable` | `IDisposable` / `IAsyncDisposable` | Define stream ownership and make disposal idempotent. |
| `IOException` | `System.IO.IOException` | Keep PDF-specific typed exceptions where callers need stable handling. |
| `HashMap<K,V>` | `Dictionary<TKey,TValue>` | Clarify thread safety, key equality, and mutation. |
| `Map.Entry` | `KeyValuePair<TKey,TValue>` | C# enumeration order should not be assumed unless the collection guarantees it. |
| `List` | `List<T>` or `IReadOnlyList<T>` | Avoid exposing mutable internal lists when preserving invariants. |
| `SortedSet` | `SortedSet<T>` | Specify comparer semantics for xref/offset ordering. |
| `Optional` / absent sentinel | Nullable reference/value type | PDF explicit null remains distinct from a missing dictionary key. |
| Checked exception declaration | No exact C# equivalent | Document thrown exceptions and validate hostile-input boundaries. |
| `java.util.logging` | `Microsoft.Extensions.Logging.ILogger<T>` | Keep structured fields for object key, offset, and revision. |
| `MessageDigest` / crypto APIs | `System.Security.Cryptography` | PDF's RC4/legacy derivation needs format compatibility; do not substitute modern defaults. |
| `InflaterInputStream` | `ZLibStream`/`DeflateStream` | zlib header/raw-deflate framing and concatenated/trailing-byte behavior require tests. |
| `final` field | `readonly` field/init-only model | Make constructor/factory guarantees explicit. |
| Visitor interface | Visitor pattern or exhaustive C# pattern matching | Ensure future COS types fail explicitly rather than being silently ignored. |
| Java package | C# namespace | Use layered namespaces such as `VeraPdf.Cos`, `VeraPdf.Pd`, and `VeraPdf.Parser`. |
| `getXxx` / `setXxx` | C# property or explicit method | Use methods for expensive/lazy/I/O operations; use properties for cheap typed access. |
| `equals` / `hashCode` | Value records, `IEquatable<T>`, custom equality | COS identity and COS value equality are different design choices. |
| `synchronized` collection access | `lock`, `ConcurrentDictionary`, immutable cache | A seekable stream's mutable position makes a concurrent parser more than a collection problem. |
| `String` | `string` | PDF bytes and PDF names must be decoded according to PDF rules, not platform defaults. |
| `byte[]` | `byte[]`, `ReadOnlySpan<byte>`, or `ReadOnlyMemory<byte>` | Avoid converting parser syntax to .NET text. Spans cannot naturally escape async/lifetime boundaries. |
| `Task.Run` around sync code | Avoid for file parsing | It hides synchronous seek access and does not create true async stream I/O. |

## Porting invariants and traps

These rules are especially important when using the dictionary during a port:

1. **Preserve absolute byte offsets as `long`.** Do not narrow xref, header, stream, or trailer
   positions to `int`.
2. **Do not conflate C# `null`, PDF null, and an absent key.** Typed PD accessors need all
   three concepts.
3. **Do not expose mutable source positions unsafely.** A reader and all cached object-stream
   parsers operate on seekable, positionful streams.
4. **Keep raw bytes raw until a defined decoding stage.** Parsing PDF text with `Encoding.UTF8`
   destroys PDF syntax semantics.
5. **Apply header offset once.** Adding it repeatedly is a common leading-byte PDF failure.
6. **Honor incremental updates.** New revisions override older object entries, but old trailer and
   source information may still be relevant.
7. **Validate xref entry kinds.** Direct, free, and compressed entries are different states;
   signed offsets are only this Java implementation's representation.
8. **Separate stream decoding from PDF parsing.** Filter order, predictor parameters, encryption,
   and stream ownership each affect the result.
9. **Use bounded traversal.** Page trees, name trees, structure trees, outlines, and indirect
   references may be cyclic or deeply nested in malformed files.
10. **Treat cryptography as a compatibility boundary.** PDF security revisions require precise
    byte algorithms and parameter framing, not merely a modern cipher with similar names.
11. **Make font and color semantics explicit.** Text code, CID, Unicode, GID, glyph name,
    component vector, and ICC color are different concepts.
12. **Keep diagnostics and recovery visible.** A conformance library must not silently turn a
    malformed file into a seemingly normal document.
13. **Design ownership before implementing disposal.** A document, parser, cached parser, and
    caller-provided stream may have different lifetimes.
14. **Preserve integer generation and signing details.** PDF encryption permissions and xref
    values depend on exact bit widths and signed/unsigned interpretation.
15. **Test known PDF corpus cases.** Build tests for leading bytes, xref tables/streams,
    incremental updates, compressed objects, object-stream boundaries, encryption revisions,
    unusual EOL, empty/free objects, and all filter pipelines.

## Suggested C# conceptual architecture

A port does not need to mirror every Java class one-for-one. A useful layering is:

- **Syntax:** byte reader, lexer, token model, invariant-culture numeric/string parsing.
- **COS model:** `CosValue` hierarchy and immutable `CosKey`.
- **Storage/index:** xref/trailer revisions, source offsets, object-stream locations.
- **Parser/resolver:** object parser and `IPdfObjectResolver`.
- **Streams:** raw/decoded byte pipeline, filters, predictors, limits, encryption, ownership.
- **PD model:** typed wrappers and services over COS values.
- **Validation/reporting:** strict diagnostics separate from permissive parsing/recovery.
- **Writing:** serializer and xref/layout calculator, separate from read-only parsing.

Keep the first port synchronous and ownership-explicit. Add true asynchronous variants only
after the seek, parsing, cache, and disposal model has been designed for async composition.

