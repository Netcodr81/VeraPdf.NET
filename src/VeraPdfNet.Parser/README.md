# .NET Port

This directory contains the complete C#/.NET port of the veraPDF parser.
The Java implementation remains unchanged in `../../java_code/veraPDF-parser/`
and continues to be managed by Maven.

Keep C# source, project files, tests, fixtures, samples, build settings,
package settings, and port-specific documentation inside this directory.

## Prerequisites

- .NET 10 SDK 10.0.100 or a compatible later 10.0 SDK
- An internet connection for the first NuGet restore

## Build and test

Run these commands from this directory:

```powershell
dotnet restore ..\..\VeraPdfNet.slnx
dotnet build ..\..\VeraPdfNet.slnx --configuration Release --no-restore
dotnet test ..\..\VeraPdfNet.slnx --configuration Release --no-build
dotnet run --project src/VeraPdf.Parser.Cli/VeraPdf.Parser.Cli.csproj
```

The Java Maven project at `../../java_code/veraPDF-parser/pom.xml` is independent of this solution.

## Package

Create a NuGet package from the clean release build:

```powershell
dotnet pack src/VeraPdf.Parser.Core/VeraPdf.Parser.Core.csproj `
  --configuration Release `
  --no-build `
  --output ./artifacts
```

The core package has the package ID `VeraPdf.Parser`.

## Public API

The stable entry points for the first .NET preview are:

- `VeraPdf.Parser.Pd.PdfDocument`
- `VeraPdf.Parser.Pd.PdfCatalog`
- `VeraPdf.Parser.Pd.PdfPage`
- `VeraPdf.Parser.Pd.PdfResources`
- `VeraPdf.Parser.Pd.PdfMetadata`
- `VeraPdf.Parser.PdfDocumentReader`
- `VeraPdf.Parser.Filters.PdfStreamFilterService`
- `VeraPdf.Parser.Fonts.CMapParser`
- `VeraPdf.Parser.Fonts.TrueTypeFontInfo`

Internal parser and COS namespaces are implementation areas and may change
before the first stable package release.

## Known preview limitations

- Password and RC4/AES PDF security are not implemented.
- DCT, JPX, and JBIG2 stream encoding needs external format support.
- Incremental save is not implemented; use `PdfDocument.SaveAs`.
- CFF/OpenType layout and full font outlines are not complete.
- Predefined CMap inheritance is not complete.
- PDF/A and PDF/UA validation profiles are not included.
