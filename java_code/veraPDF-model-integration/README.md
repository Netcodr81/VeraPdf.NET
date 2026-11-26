veraPDF-model
=============
The veraPDF Validation model described using a Domain Specific Language developed in [XText](https://eclipse.org/Xtext/).

[![Build Status](https://jenkins.openpreservation.org/job/veraPDF/job/1.31/job/model/badge/icon)](https://jenkins.openpreservation.org/job/veraPDF/job/1.31/job/model/ "OPF Jenkins")
[![Maven Central](https://img.shields.io/maven-central/v/org.verapdf/pdf-model.svg)](https://repo1.maven.org/maven2/org/verapdf/pdf-model/ "Maven central")
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/0b6a3e7f350c47d4a1ea1c5b500bbf68)](https://app.codacy.com/gh/veraPDF/veraPDF-model/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade "Codacy grade")

[![GitHub issues](https://img.shields.io/github/issues/veraPDF/veraPDF-library.svg)](https://github.com/veraPDF/veraPDF-library/issues "Open issues on GitHub")
[![GitHub issues](https://img.shields.io/github/issues-closed/veraPDF/veraPDF-library.svg)](https://github.com/veraPDF/veraPDF-library/issues?q=is%3Aissue+is%3Aclosed "Closed issues on GitHub")
[![GitHub issues](https://img.shields.io/github/issues-pr/veraPDF/veraPDF-model.svg)](https://github.com/veraPDF/veraPDF-model/pulls "Open pull requests on GitHub")
[![GitHub issues](https://img.shields.io/github/issues-pr-closed/veraPDF/veraPDF-model.svg)](https://github.com/veraPDF/veraPDF-model/pulls?q=is%3Apr+is%3Aclosed "Closed pull requests on GitHub")

Licensing
---------
The veraPDF Validation model is dual-licensed, see:

 - [GPLv3+](LICENSE.GPL "GNU General Public License, version 3")
 - [MPLv2+](LICENSE.MPL "Mozilla Public License, version 2.0")

Pre-requisites
--------------
In order to generate the model classes you'll need:

 * Java 11, 17, 21 or 25, which can be downloaded [from Oracle](https://www.oracle.com/technetwork/java/javase/downloads/index.html), or for Linux users [OpenJDK](https://openjdk.java.net/install/index.html).
 * [Maven v3+](https://maven.apache.org/)

If you want to edit and regenerate the model you'll need:

 * The [veraPDF-model-syntax project](https://github.com/veraPDF/veraPDF-model-syntax), the [README](https://github.com/veraPDF/veraPDF-model-syntax/blob/master/README.md) gives instructions on how to install and use XText support in Eclipse.

Generating the veraPDF Model
----------------------------
 1. Download the veraPDF-model repository, either: `git clone https://github.com/veraPDF/veraPDF-model`
 or download the [latest tar archive](https://github.com/veraPDF/veraPDF-model/archive/master.tar.gz "veraPDF-Model latest GitHub tar archive") or [zip equivalent](https://github.com/veraPDF/veraPDF-model/archive/master.zip "veraPDF-Model latest GitHub zip archive") from GitHub.
 2. Move to the downloaded project directory, e.g. `cd veraPDF-model`
 3. Build and install using Maven: `mvn clean install`

Reading this integration snapshot
---------------------------------

Despite the local directory name, this snapshot contains the **validation model
contracts**, not parser-backed implementations. The sources under
`src\main\java` are `.mdl` files in veraPDF's Xtext domain-specific language
(DSL), not handwritten `.java` classes. The Maven artifact is `org.verapdf:pdf-model`.
The generator and DSL grammar come from the separate veraPDF-model-syntax project.

The layers describe facts that validation rules can inspect. For example, a
parser reads a page dictionary; an adapter exposes a `PDPage` validation view;
rules inspect its links and properties. A declaration such as
`property containsMetadata: Boolean;` does not implement the check: the producing
adapter must supply it. Rendering a PDF successfully does not establish that
these validation facts satisfy PDF/A or PDF/UA requirements.

### Suggested reading route

| File in `src\main\java` | Responsibility |
| --- | --- |
| `BaseLayer.mdl` | Common model root and introductory DSL/porting notes. |
| `CosLayer.mdl` | Low-level PDF syntax evidence: primitive values, dictionaries, streams, xrefs and document revisions. |
| `PDLayer.mdl` | Semantic views of pages, resources, fonts, annotations, actions, signatures and structure trees. |
| `Operator.mdl` | Content-stream instructions, operands, effective graphics/text state and glyph usage. |
| `External.mdl` | Facts from embedded ICC profiles, font programs, CMaps, JPEG2000, files and PKCS#7 data. |
| `XMPLayer.mdl` | XMP packet serialization, PDF/A and PDF/UA identification, properties and extension declarations. |
| `SELayer.mdl` | Tag-specific structure elements and their associated marked/unmarked content. |
| `SALayer.mdl` | Structure-analysis results: detected chunks, tables, lists, semantic scores and diagnostics. |

Start with `CosDocument`, follow its `document` link to `PDDocument`, then
`pages` to `PDPage`, and `contentStream` to `PDContentStream.operators`.
This connects file-level evidence to semantic objects and drawing instructions.
`PDSemanticContentStream.content` connects instructions to SE content items;
`CosDocument.doc` connects to the separate SA analysis view.

### Reading the DSL

| Construct | Meaning for a reader or porter |
| --- | --- |
| `package` / `import` | Namespaces and referenced types. Imports do not implement the referenced functionality. |
| `% ...` | Existing DSL documentation syntax, used by the inline explanations in these files. |
| `type A extends B` | A model type inheriting another model contract. Empty types still give rules a distinct type to target. |
| `property name: T;` | A scalar or explicitly declared collection value, supplied by an implementation. It may be derived rather than stored. |
| `link name: T;` | A relationship to another validation object, retaining its own type and validation evidence. |
| `T?`, `T*`, `T+` on links | Optional, zero-or-more, and one-or-more multiplicity respectively. An unsuffixed link describes one target. These are not C# nullable/generic syntax. |

Multiplicity does **not** specify exact operand counts (a matrix link marked `+`
still needs a six-coefficient check), ownership, lazy/eager evaluation or the
generated Java return type. Consult the grammar, generator and generated API
before choosing the C# link representation. Existing comments explicitly describe
an empty `imageCS` traversal for `PDSMaskImage`, despite its inherited declaration.
Malformed PDFs must remain inspectable; do not manufacture missing targets to
make nominal multiplicity look valid.

Properties such as `containsLang` and a `Lang` link are not interchangeable:
a key may exist but contain a wrong type or an unresolved reference. Preserve
the producer's distinctions between missing, explicit PDF null, invalid, empty,
and valid values. Nullability is not fully expressed by scalar DSL declarations;
for example, `PDSimpleFont.FirstChar` is `Integer` but its documentation permits null.

.NET and C# porting reference
----------------------------

The independent native contract port now lives under
`src\VeraPdfNet.Model` at the enclosing repository root. Its
`docs\model-contract-mapping.md` records the inspected, pinned upstream generator:
`Integer` becomes Java `Long`, `Decimal` becomes Java `Double`, and links use
named traversal rather than typed getters. The candidate table below is general
porting guidance; use the native mapping document for the implemented API.

These are **candidate equivalents**, not existing C# model classes or a claim that
the Java generator has been inspected. This snapshot does not include that
generator or generated Java interfaces. Resolve numeric widths, getter shapes,
link traversal, nulls and exception behavior against the generated API and its
implementations before freezing a .NET public contract.

| Java / DSL concept | C# / .NET equivalent and caveat |
| --- | --- |
| Generated Java interface; model `extends` | C# interface and interface inheritance. Keep concrete parser-backed adapters separate. |
| `org.verapdf.model.baselayer.Object` | Custom root such as `IModelObject`, **not** `System.Object`. |
| DSL `String` / Java `String` | `string` / `string?`. Both use UTF-16; not every PDF byte-string projection is Unicode text. |
| DSL `Boolean` / boxed Java `Boolean` | `bool` / `bool?` according to the producer's null contract. Missing must not silently become false. |
| DSL `Integer` | Inspect the generator: Java `Integer` maps to `int`/`int?`, Java `Long` to `long`/`long?`. Do not infer a 32-bit width from the DSL name. Preserve large byte offsets/counts. |
| DSL `Decimal` | Inspect the generator: Java `Double` maps to `double`/`double?`; `BigDecimal` requires an explicit precision/range strategy. CLR `decimal` is not automatically equivalent to either. |
| `java.util.List<T>` | `System.Collections.Generic.List<T>` internally; `IReadOnlyList<T>` for an ordered read-only view. Keep duplicates. |
| `List<List<String>>` | Nested lists, or `IReadOnlyList<IReadOnlyList<string>>`; retain diagnostic argument grouping. |
| `java.util.Set<String>` | `HashSet<string>` / `IReadOnlySet<string>` where supported; use ordinal comparison for exact identifiers, without assuming iteration order. |
| Model links | Typed model relationships, potentially exposed through ordered collections even for single-target traversal. Match the actual generated/adapter API, not just the DSL suffix. |
| PDF dictionaries | Custom PDF-name/value model, backed by a dictionary if appropriate. Compare names ordinally; `/CA` and `/ca` differ. |
| PDF streams | Custom dictionary-plus-data model backed by `System.IO.Stream`; not a replacement with `Stream` alone. Stream ownership/disposal belongs to the producing implementation. |
| Binary-format readers | `System.Buffers.Binary.BinaryPrimitives`, byte buffers and streams; preserve each format's endianness and bounds. |
| XML/XMP readers | `System.Xml.XmlReader` / `System.Xml.Linq.XDocument` plus explicit RDF/XMP interpretation. Disable DTDs/external resolution for untrusted packets. |
| PKCS#7/CMS and X.509 | `System.Security.Cryptography.Pkcs.SignedCms`, `X509Certificates.X509Certificate2`, and `System.Formats.Asn1.AsnReader` for low-level ASN.1. Packaging depends on the target framework. Certificate presence is not cryptographic validity or trust. |
| ICC/font/JPEG2000 validators | Compatible format-specific parsers or separately ported implementations. The .NET base libraries do not supply all of these conformance checks. |
| Xtext / Xtend | No direct .NET base-library equivalent. Generate C# from the DSL with suitable tooling, or maintain equivalent interfaces with automated schema-parity checks. Roslyn can help emit C# but does not parse this DSL by itself. |
| Maven / JAR | MSBuild / .NET SDK projects and assemblies/NuGet packages. Reproduce source generation explicitly; `dotnet build` cannot compile `.mdl` files as C#. |

### Compatibility details worth preserving

- **Names and identity:** Keep rule-facing type/property/link names exact, including
  case, even if C# members are PascalCase behind an explicit name mapping. PDF
  object keys contain an object number and generation; they are not file offsets
  or CLR object hashes. Graph traversal needs cycle handling.
- **Raw evidence:** Keep lexical numbers, byte lengths, original packet encodings,
  PDF string bytes and invalid syntax alongside interpreted values. Use invariant
  culture for numeric conversion. XML normalization or bitmap rendering loses
  evidence the model intentionally exposes.
- **Context:** Resource inheritance, graphics state, role maps, heading order and
  analysis scores depend on surrounding objects. Do not cache context-dependent
  values solely by stream ID or in process-wide mutable state.
- **Diagnostic serialization:** Existing comma-, space- and ampersand-separated
  properties are rule-facing strings, not an invitation to change the API to
  collections. Preserve their ordering, separators and null/empty behavior.
- **Signatures:** PDF byte-range coverage and CMS inspection are separate checks.
  Examine the original bytes; reserializing objects changes signed content.
- **Type families:** Keep empty tag/operator/font types and PDF-version-specific
  distinctions. Avoid reducing all types to an enum that drops unknown input.

Build configuration
-------------------

`pom.xml` wires Xtext's `org.verapdf.ModelStandaloneSetup` to generate Java into
`generated-model\java`. The build-helper plugin adds that directory as a Java
source root; the clean plugin deletes it. Edit the `.mdl` sources, not generated
output. The model-syntax dependency is a build-plugin dependency, not an embedded
font/XML/PDF parser implementation.

The workflows under this snapshot's `.github\workflows` describe the upstream
model repository's Java build and Arlington-branch propagation. Nested workflows
are not automatically discovered as workflows for this enclosing repository.
They do not build the C# parser or prove .NET model parity.

From the enclosing repository root, the upstream model build can be invoked as:

```powershell
Set-Location java_code\veraPDF-model-integration
mvn --batch-mode verify
```

This requires the Java/Maven prerequisites above and access to the parent POM,
Xtext and veraPDF model-syntax artifacts. A .NET port needs its own schema-parity
checks and behavioral comparison against the Java adapters; successful generation
of interfaces alone does not verify validation behavior.
