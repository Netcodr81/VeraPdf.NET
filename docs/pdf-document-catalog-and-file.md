# PDF Document, Catalog, and File Structure

This guide explains how the PDF file, the document model, and the catalog relate
to one another in veraPDF Parser.

## The relationship

The **PDF file** is the byte sequence stored on disk or supplied as a stream.
The **document** is the logical PDF represented by those bytes. The **catalog**
is a dictionary inside the PDF that acts as the entry point to its page tree and
other document-wide features.

```text
PDF file (bytes)
└── COSDocument: low-level PDF structure
    ├── Header: %PDF-1.4
    ├── Indirect objects: dictionaries, arrays, and streams
    ├── Cross-reference data: locations of indirect objects
    └── Trailer
        └── /Root ───────────────────┐
                                     ▼
                          Catalog (/Type /Catalog)
                          ├── /Pages ──► Page tree
                          │             ├── Page 1 ──► content stream
                          │             └── Page 2 ──► content stream
                          ├── /Metadata
                          ├── /Outlines
                          └── /AcroForm, etc.

PDDocument: high-level API coordinating COSDocument and PDCatalog
```

In the parser, [`PDDocument`](../src/main/java/org/verapdf/pd/PDDocument.java)
holds a `PDCatalog` and a `COSDocument`. `PDDocument` provides convenient
operations for working with pages and document features.
[`COSDocument`](../src/main/java/org/verapdf/cos/COSDocument.java) manages the
lower-level PDF state: objects, the header, cross-reference information, and
the trailer. [`PDCatalog`](../src/main/java/org/verapdf/pd/PDCatalog.java)
provides a higher-level view of the catalog dictionary.

The catalog is **not a separate file** or a list of files. It is a dictionary
object within the PDF. The trailer's `/Root` entry points to that dictionary;
the catalog's `/Pages` entry leads to the page tree. Page-tree nodes organize
the pages, and each page dictionary can reference its content stream and
resources, such as fonts and images.

## How a PDF file is constructed

A typical PDF file contains these parts:

```text
%PDF-1.7                 ← header / PDF version
...                      ← binary marker and other bytes

1 0 obj                  ← indirect object #1
<< /Type /Catalog
   /Pages 2 0 R
>>
endobj

2 0 obj                  ← indirect object #2: page-tree root
<< /Type /Pages
   /Kids [3 0 R]
   /Count 1
>>
endobj

3 0 obj                  ← indirect object #3: a page
<< /Type /Page
   /Parent 2 0 R
   /Contents 4 0 R
>>
endobj

4 0 obj                  ← a stream, often page-drawing instructions
<< /Length ... >>
stream
...drawing instructions...
endstream
endobj

xref                     ← maps object numbers to file byte offsets
...
trailer
<< /Root 1 0 R ... >>    ← points to the catalog
startxref
...                      ← byte offset of the cross-reference data
%%EOF
```

This is an illustrative sketch, not a complete PDF. Real files can contain
many more objects, compressed streams, fonts, images, annotations, and other
structures. The content stream contains drawing instructions; the page's
resources describe objects those instructions can use.

PDF dictionaries and arrays contain values such as names, strings, numbers, and
references. For example, `2 0 R` means “the indirect object numbered 2,
generation 0.” Cross-reference data lets a reader locate indirect objects
without scanning the entire file. A PDF may use a traditional cross-reference
table or a cross-reference stream.

## How veraPDF Parser follows the structure

When opening a PDF, `COSDocument` initializes the low-level state from the
source, including its header, cross-reference data, and trailer. When
`PDDocument.getCatalog()` is called, it follows the trailer's `/Root`
reference and associates that object with the `PDCatalog` wrapper.

For a newly constructed document, `PDDocument` initializes a `%PDF-1.4` header
and creates a catalog with an initial `/Pages` tree. Pages can then be added
through the document API.

When writing, the writer serializes PDF objects and then writes cross-reference
and trailer information. The current writer emits a classic `xref` table. The
`saveAs` path uses an `IndirectWriter` to write the document, while `saveTo`
appends an incremental update to the original PDF. The parameterless `save()`
method in `PDDocument` is currently a placeholder.

Related implementation:

- [`COSTrailer`](../src/main/java/org/verapdf/cos/COSTrailer.java) handles
  trailer entries such as `/Root`.
- [`Writer`](../src/main/java/org/verapdf/cos/visitor/Writer.java) serializes
  objects and cross-reference/trailer information.
- [`IndirectWriter`](../src/main/java/org/verapdf/cos/visitor/IndirectWriter.java)
  assigns compact sequential object numbers while writing.
