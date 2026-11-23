# Phase 7: Validation and Release Readiness

Phase 7 makes the native port measurable, packageable, and explicit about
unsupported behavior.

## Package configuration

The `VeraPdf.Parser.Core` project produces a NuGet package:

- package ID: `VeraPdf.Parser`;
- preview version: `0.7.0-preview`;
- target framework: `.NET 10`;
- package readme: `dotnet/README.md`;
- symbol package: `.snupkg`;
- source repository: veraPDF Parser.

The package license declaration is:

```text
GPL-3.0-or-later OR MPL-2.0
```

A release owner must confirm the chosen distribution path and third-party
notice requirements before publishing.

## API

Phase 7 considers these public entry points preview-stable:

- `PdfDocument`;
- `PdfCatalog`;
- `PdfPage`;
- `PdfResources`;
- `PdfMetadata`;
- `PdfDocumentReader`;
- `PdfStreamFilterService`;
- `CMapParser`;
- `TrueTypeFontInfo`.

Implementation namespaces for COS values, lexer, xref, writer, and temporary
stream ownership are not stable until a public API review is complete.

## CI

The .NET workflow restores, builds, tests, and packs on Windows, Linux, and
macOS. Java CI remains separate and is not replaced by the C# workflow.

The release job should also upload the `.nupkg` and `.snupkg` artifacts for
manual review until NuGet publishing authorization is configured.

## Security review

Reviewed security-sensitive areas:

- seekable stream offsets;
- stream substream ranges;
- xref offsets and revisions;
- object-count and array bounds;
- decompression absolute and expansion limits;
- temporary-file cleanup;
- TrueType and CMap offsets;
- CMap and font entry-count limits.

Required before production:

- independent PDF security review;
- encrypted PDF password and decryption implementation;
- raw stream length and `/Length` indirect-value support;
- incremental save integrity tests;
- native and managed dependency vulnerability scan;
- release license attribution review.

Do not open untrusted PDFs with the preview if its resource limits or security
boundary has not been approved for the target deployment.

## Compatibility

The C# suite contains 27 passing unit tests covering:

- streams and spooling;
- lexer values;
- traditional and stream cross-references;
- incremental revision merge;
- direct and Flate decoding;
- predictor and filter behavior;
- catalog and page traversal;
- full rewrite and reopen;
- CMap and TrueType metadata.

Phase 7 does not claim PDF/A or PDF/UA fixture parity. A compatibility corpus
and frozen Java baseline are release work for the next stage.

## Performance baseline

Run and record the baseline on the target host:

```powershell
dotnet test VeraPdf.Parser.slnx --configuration Release
dotnet run --project src/VeraPdf.Parser.Cli/VeraPdf.Parser.Cli.csproj --configuration Release --no-build -- <pdf-path>
```

Record:

- PDF file size;
- elapsed open time;
- elapsed page enumeration time;
- peak working-set memory;
- total stream decode bytes;
- host OS and CPU.

No performance target is approved until fixture and host values are recorded.
