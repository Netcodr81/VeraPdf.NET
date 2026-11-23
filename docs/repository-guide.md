# veraPDF Parser: Repository Reading Guide

This guide is a route through the repository for understanding how a PDF is read and represented. The project is a Java library for parsing, modifying, and creating PDF documents, rather than a standalone command-line application.

## Suggested reading route

Follow one PDF from its input bytes up through the document model:

1. **Start with the public document API — `PDDocument`**
   - [`src/main/java/org/verapdf/pd/PDDocument.java`](../src/main/java/org/verapdf/pd/PDDocument.java)
   - Look at its constructors and `open` methods to see how a document is loaded. Then see how it exposes the catalog and pages.

2. **Follow document initialization — `COSDocument` and `Reader`**
   - [`src/main/java/org/verapdf/cos/COSDocument.java`](../src/main/java/org/verapdf/cos/COSDocument.java)
   - [`src/main/java/org/verapdf/io/Reader.java`](../src/main/java/org/verapdf/io/Reader.java)
   - `COSDocument` initializes the lower-level document state and reader. `Reader` obtains the PDF header and cross-reference information and resolves indirect objects from the PDF.

3. **See how PDF syntax is parsed**
   - [`src/main/java/org/verapdf/parser/PDFParser.java`](../src/main/java/org/verapdf/parser/PDFParser.java)
   - [`src/main/java/org/verapdf/parser/`](../src/main/java/org/verapdf/parser/)
   - `PDFParser` handles PDF-level parsing, including the header and cross-reference structures. Other parser classes handle tokens, streams, object streams, and related syntax.

4. **Understand the low-level object model — `COS*`**
   - [`src/main/java/org/verapdf/cos/`](../src/main/java/org/verapdf/cos/)
   - These classes represent PDF primitives and structures such as names, numbers, arrays, dictionaries, streams, indirect references, and cross-reference tables. Start with `COSObject`, `COSDictionary`, `COSStream`, and `COSKey`.

5. **Move to PDF concepts — `PD*`**
   - [`src/main/java/org/verapdf/pd/`](../src/main/java/org/verapdf/pd/)
   - These classes provide higher-level wrappers for PDF concepts such as the catalog, pages, resources, fonts, forms, and annotations. For example, `PDDocument` works with a `PDCatalog`, which exposes the page tree.

6. **Trace the selected font-cache helper**
   - [`src/main/java/org/verapdf/tools/FontProgramIDGenerator.java`](../src/main/java/org/verapdf/tools/FontProgramIDGenerator.java)
   - Find its callers in `PDCIDFont`, `PDTrueTypeFont`, and `PDType1Font`. Those classes construct identifiers for embedded font programs and look them up through `StaticResources`.
   - [`src/main/java/org/verapdf/tools/StaticResources.java`](../src/main/java/org/verapdf/tools/StaticResources.java)
   - The identifier distinguishes font programs by the PDF font object and relevant interpretation inputs (for example, CMap, encoding, symbolic/CFF flags, and subset status). When there is no indirect object key, some generator methods return `null`; the cache ignores null keys.

## Useful areas to explore

- **Parsing:** [`src/main/java/org/verapdf/parser/`](../src/main/java/org/verapdf/parser/)
- **PDF object model:** [`src/main/java/org/verapdf/cos/`](../src/main/java/org/verapdf/cos/)
- **High-level document model:** [`src/main/java/org/verapdf/pd/`](../src/main/java/org/verapdf/pd/)
- **Fonts:** [`src/main/java/org/verapdf/pd/font/`](../src/main/java/org/verapdf/pd/font/)
- **Object writing and visitors:** [`src/main/java/org/verapdf/cos/visitor/`](../src/main/java/org/verapdf/cos/visitor/)
- **Factories:** [`src/main/java/org/verapdf/factory/`](../src/main/java/org/verapdf/factory/)
- **Static data used during parsing:** [`src/main/resources/`](../src/main/resources/)
- **Tests:** [`src/test/java/`](../src/test/java/) and [`src/test/resources/`](../src/test/resources/)

## Tests as examples

Tests can show how parts of the parser are expected to behave:

- [`src/test/java/org/verapdf/parser/PDFParserTest.java`](../src/test/java/org/verapdf/parser/PDFParserTest.java) exercises cross-reference parsing.
- Tests under `src/test/java/org/verapdf/pd/font/` cover font formats and CMaps.
- [`src/test/java/org/verapdf/cos/visitor/WriterTest.java`](../src/test/java/org/verapdf/cos/visitor/WriterTest.java) demonstrates document writing and reopening.

## Build and test

The project uses Maven. The README lists Java and Maven prerequisites and describes `mvn clean install` as the full build/install command. To run tests without installing the artifact, use:

```powershell
mvn test
```

## Project orientation

- [`README.md`](../README.md) gives the project summary, prerequisites, and build instructions.
- [`pom.xml`](../pom.xml) defines the Maven artifact, dependencies, and build plugins.
- [`porting-dictionary.md`](./porting-dictionary.md) defines PDF, veraPDF, Java, and C#/.NET
  terms useful when understanding or porting the parser.
- `src/main/java` contains the implementation; `src/main/resources` contains runtime data.
- `src/test` contains tests and test PDFs/fonts.
- `target` contains generated build output; start with `src`, not `target`, when reading or editing source.
