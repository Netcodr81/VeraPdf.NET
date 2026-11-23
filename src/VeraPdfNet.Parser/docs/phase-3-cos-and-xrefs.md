# Phase 3: COS Model and Cross-References

Phase 3 adds native .NET cross-reference lookup and object resolution.

## Implemented components

- `CosKey` value type;
- `CosIndirectObject` for resolved indirect values;
- sparse `PdfXrefTable`;
- free, direct, and compressed `XrefEntry` record types;
- PDF revision and trailer grouping;
- traditional `xref` table parsing;
- xref stream parsing;
- `W`, `Index`, `/Size`, and `/Prev` handling;
- direct and compressed indirect-object resolution;
- raw and Flate xref/object streams;
- incremental revision merge;
- cross-reference loop detection;
- public `PdfDocumentReader.Open` file and stream APIs.

## Incremental behavior

The newest revision is read first. `/Prev` references are followed only once.
Entries from newer revisions override older entries. The newest trailer becomes
the `Trailer` and `LastTrailer`; the oldest visited trailer becomes
`FirstTrailer`.

The reader does not silently accept repeated revision offsets. Repeated
offsets raise an `IOException` describing a cross-reference loop.

## Object resolution

`PdfDocumentReader.Resolve(CosKey)` supports:

- direct file offsets;
- free entries;
- compressed entries in object streams.

Compressed object lookup requires the containing object stream to resolve
normally. The `/N` and `/First` header is used to locate the requested object
body.

## Tests

Phase 3 tests cover:

- traditional xref tables;
- xref stream dictionaries and entries;
- direct object resolution;
- incremental old/new object resolution;
- trailer `/Root`;
- cross-reference loops;
- seekable stream and lexer behavior shared with Phase 2.
