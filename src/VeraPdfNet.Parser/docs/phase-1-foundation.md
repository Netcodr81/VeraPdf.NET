# Phase 1: Repository and Test Foundation

Phase 1 establishes the C#/.NET side of the repository without changing or
moving any Java code.

## Completed structure

```text
dotnet/
├── VeraPdf.Parser.slnx
├── global.json
├── NuGet.config
├── Directory.Build.props
├── Directory.Packages.props
├── src/
│   ├── VeraPdf.Parser.Core/
│   └── VeraPdf.Parser.Cli/
├── tests/
│   ├── VeraPdf.Parser.Tests/
│   ├── VeraPdf.Parser.Fixtures/
│   └── VeraPdf.Parser.Compatibility/
├── samples/
└── docs/
```

## Java baseline

The Java implementation currently contains:

- 274 Java source files under `src/main/java`;
- 24 Java test files under `src/test/java`;
- test resources under `src/test/resources`.

The initial test areas are:

- COS documents and streams;
- stream filters;
- COS object writing;
- input and seekable-stream behavior;
- PDF parsing and cross-references;
- name-tree nodes;
- CFF, CMap, OpenType, TrueType, Type1, and Type3 fonts;
- PDF function types.

The known Java test paths are the baseline inventory under `src/test/java`.
They should be mapped to C# test names as each parser area is implemented.

## Fixture layout

Small fixtures belong in `dotnet/tests/VeraPdf.Parser.Fixtures`.

Each future fixture should include:

- a stable relative path;
- a purpose, such as Flate decoding or incremental update;
- the legal right to distribute it;
- the expected Java behavior;
- an optional independent PdfPig or PDFium result.

Do not place copyrighted, sensitive, or customer PDFs in source control.

## Verification

Verified Phase 1 commands:

```powershell
dotnet restore VeraPdf.Parser.slnx
dotnet build VeraPdf.Parser.slnx --configuration Release --no-restore
dotnet test VeraPdf.Parser.slnx --configuration Release --no-build
dotnet run --project src/VeraPdf.Parser.Cli/VeraPdf.Parser.Cli.csproj --configuration Release --no-build
```

The baseline build has no warnings or errors and includes one passing C#
foundation test.
