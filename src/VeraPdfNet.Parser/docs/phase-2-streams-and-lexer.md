# Phase 2: Streams and Lexer

Phase 2 provides the binary stream and token foundations for later C#
parser phases.

## Implemented components

- `PdfSeekableStream`
- memory-backed seekable streams;
- temporary-file-backed seekable streams for input above 10 KB;
- independent substreams;
- seek, peek, unread, and stream-origin operations;
- configurable maximum input size;
- `PdfLexer`;
- byte-safe whitespace and comment handling;
- integer and real numbers;
- names, including `#xx` escapes;
- literal strings and escapes;
- hexadecimal strings;
- arrays and dictionaries;
- keyword token recognition;
- `CosValue` models for PDF booleans, numbers, strings, names, null,
  arrays, dictionaries, indirect references, and streams;
- `PdfCosParser`;
- direct `/Length` stream reading.

## Resource behavior

Input at or below 10 KB stays in memory. Larger input is copied to an
independent temporary file. Disposing substreams and parent streams releases
their references; the temporary file is deleted after the final reference is
disposed.

`PdfSeekableStream.MaximumStreamSize` can enforce a hard input limit.
Configured limits return `PdfResourceLimitExceededException`.

## Tests

Phase 2 tests cover:

- 10, 10239, and 15000-byte seekable input;
- independent substream positions;
- unread behavior;
- PDF comments and whitespace;
- numbers, names, strings, arrays, and dictionaries;
- indirect references;
- direct-length streams;
- configured resource limits.
