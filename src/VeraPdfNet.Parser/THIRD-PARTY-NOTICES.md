# Third-Party Notices

The native parser implementation is derived from and licensed with the
veraPDF Parser source in this repository.

Current package dependencies introduced by the project templates:

- xUnit
- xUnit runner visual studio
- Microsoft.NET.Test.Sdk
- coverlet collector

Those dependencies are test-project dependencies and must not be assumed to
be runtime dependencies of `VeraPdf.Parser.Core`.

Embedded font metrics under `src/main/resources/font/stdmetrics` are existing
repository resources. Redistributors must retain the repository licensing and
any applicable metric licensing notices.

PdfPig and PDFium are evaluation options for independent testing only in the
current project. They are not runtime dependencies unless explicitly added
and approved.
