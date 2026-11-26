# veraPDF Model for .NET

`VeraPdf.Model` is a native .NET 10 port of veraPDF's **validation contracts**.
It contains all eight model layers (373 interfaces in the initial snapshot),
schema lookup support and a minimal `GenericModelObject` base. It has no
runtime dependency on `VeraPdf.Parser`, Java, Maven or third-party packages.

**This package does not validate PDFs.** Parser-backed adapters, content
interpretation, XMP/embedded-format validators, structure analysis, validation
profiles and a rule engine are separate future work.

## Layout and build

Like `src\VeraPdfNet.Parser`, this sibling workspace contains its own SDK,
central test-package versions, source, tests, tooling and documentation.
Use .NET 10 SDK 10.0.100 or a compatible later 10.0 SDK.
From this directory:

```powershell
dotnet build ..\..\VeraPdfNet.slnx --configuration Release
dotnet test ..\..\VeraPdfNet.slnx --configuration Release --no-build
dotnet pack src\VeraPdf.Model.Core --configuration Release --no-build --output artifacts
```

The package includes XML documentation, symbols, the model manifest, README
and licensing notices. Generated contracts are checked in: ordinary builds
and NuGet consumers do not need the generator or Java.

## Public API

- `VeraPdf.Model.BaseLayer.IModelObject`: nine root methods, including
  `GetLinkedObjects(string linkName)`, context and model-name accessors.
- Interfaces under `CosLayer`, `PDLayer`, `Operator`, `External`, `XMPLayer`,
  `SELayer` and `SALayer` represent the respective DSL types. Empty interfaces
  are intentional semantic identities.
- `ModelHelper` exposes immutable declarations and inherited names.
- `GenericModelObject` supplies those names and empty context defaults;
  it throws on unimplemented link traversal instead of returning fake success.

Scalar methods use `Get` followed by the **unchanged** DSL identifier:
`Getca()` and `GetCA()` are different methods. Integer maps to `long?`,
Decimal to `double?`, Boolean to `bool?`, and String to `string?`.
Collection-valued properties use nullable read-only list/set interfaces.
Implementations must preserve missing/invalid values rather than return
false, zero or an invented object.

Unlike a typical object model, the upstream generator emits **no typed link
getters**. Use case-sensitive names such as `"pages"` with `GetLinkedObjects`,
returning an ordered model-object list even for nominal single/optional links.
Link target and multiplicity are retained in `ModelHelper` metadata; this
library does not enforce nominal multiplicities on malformed PDFs.

```csharp
using VeraPdf.Model;
using VeraPdf.Model.CosLayer;

sealed class MissingBoolean : GenericModelObject, ICosBool
{
    public MissingBoolean() : base("CosBool") { }
    public bool? Getvalue() => null;
}
```

This is an illustrative contract implementation, not a PDF adapter.

## Regeneration

Authoritative inputs remain under
`java_code\veraPDF-model-integration\src\main\java` at the repository root.
Edit the `.mdl` files, not `Generated` output. From this workspace:

```powershell
dotnet run --project tools\VeraPdf.Model.Generation -- `
  --input ..\..\java_code\veraPDF-model-integration\src\main\java `
  --output src\VeraPdf.Model.Core\Generated
```

Append `--check` to verify without writing. It exits nonzero for changed,
missing or obsolete generated files. Unsupported DSL syntax and unresolved
references are errors with source diagnostics, not silently skipped declarations.
The tool is intentionally limited to the pinned snapshot's declaration grammar.
Generation preserves source order for members and emits stable UTF-8/LF output.

Tests independently inventory the DSL and inspect the compiled interfaces,
in addition to checking generation and the reviewed API baseline. When the
upstream schema changes, regenerate and deliberately review compatibility
baseline changes rather than blindly accepting new counts.

See [contract mapping](docs/model-contract-mapping.md) for pinned Java
generator provenance, .NET mappings and intentional API differences.

## Licensing

Derived from the veraPDF model, dual-licensed under GPL-3.0-or-later or MPL-2.0.
Retain upstream notices and choose the applicable license for redistribution.
See `THIRD-PARTY-NOTICES.md` and the included `LICENSE.GPL` / `LICENSE.MPL`.
