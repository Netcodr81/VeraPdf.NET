# Phase 5: PD Document Model

Phase 5 provides read-oriented document-domain APIs over the COS and cross-
reference implementation.

## Implemented components

- `PdfDocument` public open/dispose API;
- catalog wrapper and `/Root` resolution;
- page-tree traversal with cycle detection;
- page count and page lookup;
- inherited page media, crop, bleed, trim, and art boxes;
- inherited page resources;
- page content streams;
- page annotations;
- resources for fonts, XObjects, and color spaces;
- metadata streams;
- basic AcroForm and structure wrappers;
- full PDF rewrite through `SaveAs`;
- COS value serialization for dictionaries, arrays, names, strings,
  numbers, indirect references, and streams.

## Ownership

`PdfDocument` owns its `PdfDocumentReader`. Calling `Dispose` releases the
seekable source and any temporary file owned by the reader.

The catalog, page tree, pages, resources, and annotation wrappers expose
read-only views. Optional PDF values are returned as `null` or empty collections
instead of synthetic empty dictionaries.

## Page inheritance

Page boxes and resources may be inherited from page-tree parent nodes.
The page wrapper walks parent dictionaries and detects repeated dictionary
instances to prevent cycles.

## Writing

`PdfDocument.SaveAs(path)` and `PdfDocument.SaveAs(stream)` perform a full
rewrite. The implementation writes indirect objects, a traditional xref table,
a trailer containing `/Root`, and `startxref`.

Incremental save-to-original behavior is not claimed as complete in Phase 5.
It requires preservation of original bytes plus xref offset and object-remap
state. It should be implemented as a focused writer task before callers rely
on modifying an existing file.

## Tests

Phase 5 tests cover:

- catalog and `/Root`;
- one-page and page-tree traversal;
- inherited media and crop boxes;
- page resources;
- content-stream decoding;
- annotations;
- metadata decoding;
- page-tree cycle rejection;
- full rewrite followed by reopen.
