# VeraPdf.NET

Native .NET libraries based on veraPDF:

- [`src\VeraPdfNet.Parser`](src/VeraPdfNet.Parser/README.md): PDF parsing and document structures.
- [`src\VeraPdfNet.Model`](src/VeraPdfNet.Model/README.md): generated validation-model contracts and schema lookup support.

The model library does not include parser-backed adapters, validation profiles
or a PDF/A/PDF/UA validation engine. The Java reference snapshots remain under
`java_code`; the C# projects are under the root `src` directory.

Build and test both workspaces through `VeraPdfNet.slnx`:

```powershell
dotnet build VeraPdfNet.slnx --configuration Release
dotnet test VeraPdfNet.slnx --configuration Release --no-build
```