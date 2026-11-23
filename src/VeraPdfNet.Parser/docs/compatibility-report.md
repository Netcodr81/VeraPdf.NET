# .NET Port Compatibility Report

Status: Phase 7 preview

| Area | Status | Evidence |
|---|---|---|
| PDF header and seekable streams | Implemented | 27 passing unit tests |
| PDF primitives and names | Implemented | Lexer and COS parser tests |
| Traditional xrefs | Implemented | Synthetic traditional-xref test |
| Xref streams | Implemented | Synthetic xref-stream test |
| Incremental `/Prev` revisions | Implemented | Synthetic incremental test |
| Object-stream compressed entries | Implemented | Resolver implementation; fixture validation remains |
| ASCIIHex and ASCII85 | Implemented | Unit tests |
| RunLength and LZW | Implemented | Unit tests and existing Java behavior |
| Flate | Implemented | Zlib and raw DEFLATE test |
| PNG/TIFF predictors | Implemented | PNG Sub test and implementation |
| PDF catalog and pages | Implemented | Synthetic document test |
| Page inheritance | Implemented | Media/crop box tests |
| Resources and metadata | Implemented | Synthetic resource test |
| Full rewrite | Implemented | Save/reopen test |
| Incremental save | Not implemented | Planned writer follow-up |
| Password/RC4/AES | Not implemented | Security follow-up |
| DCT/JPX/JBIG2 | Not implemented | External-format follow-up |
| CMaps | Partial | Common CID/bfchar/bfrange syntax |
| TrueType cmap | Partial | Format 4 and 12 parsing |
| CFF/OpenType | Not implemented | Font follow-up |
| PDF/A validation | Not implemented | Separate validation project |
| PDF/UA validation | Not implemented | Separate validation project |

## Release gate

This report is not a conformance claim. Release approval requires:

- a legally cleared fixture corpus;
- frozen Java output baselines;
- independent library cross-checks;
- malformed-input fuzz testing;
- security review;
- NuGet license review.
