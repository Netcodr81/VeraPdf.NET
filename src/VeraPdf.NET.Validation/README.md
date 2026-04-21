# VeraPdf.NET.Validation

`VeraPdf.NET.Validation` is a .NET library that wraps the veraPDF CLI to validate PDF documents against PDF/A, PDF/UA, and WCAG 2.2-oriented profiles.

## Why this library exists

Integrating veraPDF directly in .NET applications usually requires custom setup for:

- Java runtime management
- veraPDF runtime extraction and execution
- process timeouts and error handling
- parser prechecks before expensive external validation
- normalized response models for API and UI use

This library centralizes those concerns so teams can add standards validation with a small, consistent integration surface.

## What you get

- Runtime provisioning from bundled `Java.zip` and `veraPDF.zip`
- Parser precheck diagnostics before CLI invocation
- Structured validation output for one or multiple standards
- Error code normalization for runtime/process/input failures
- DI registration extensions for ASP.NET Core/Blazor/Worker services

## Getting started

1. Install the package:

    ```bash
    dotnet add package VeraPdf.NET.Validation // not yet implemented. Clone and add the projects VeraPDF.NET.Validation and 
    VeraPDF.NET.Parser to your solution manually.
    ```

2. Register services in your startup code (`Program.cs`):

    ```csharp
    using VeraPdf.NET.Validation;

    builder.Services.AddVeraPDFNet(options =>
    {
        builder.Configuration.GetSection("VeraPdfRuntime").Bind(options.Runtime);
        builder.Configuration.GetSection("VeraPdfExecution").Bind(options.Execution);
    });
    ```

3. Inject and use `IVeraPdfValidationService`:

    ```csharp
    using VeraPdf.NET.Validation;
    using VeraPdf.NET.Validation.Models;

    public sealed class DocumentValidator(IVeraPdfValidationService validationService)
    {
        public async Task<VeraPdfValidationReport> ValidateAsync(Stream pdf, string fileName, CancellationToken cancellationToken)
        {
            return await validationService.ValidateAsync(
                pdf,
                fileName,
                ValidationStandard.PdfA | ValidationStandard.PdfUa,
                cancellationToken: cancellationToken);
        }
    }
    ```

## Integrating into a .NET project

### ASP.NET Core / Minimal API

- Register services with `AddVeraPDFNet(...)`
- Inject `IVeraPdfValidationService` in endpoints or services
- Return `VeraPdfValidationReport` directly from API handlers

### Blazor Server

- Register services once in `Program.cs`
- Inject validation service into components or backing services
- Use report summary and per-standard reports to render UI feedback

### Worker / Background service

- Register via `AddVeraPDFNet(...)`
- Resolve `IVeraPdfValidationService` inside hosted/background services
- Validate documents from queue/blob/file pipelines

## Optional configuration

Runtime and execution options can be supplied via configuration sections:

- `VeraPdfRuntime`
- `VeraPdfExecution`

Use these to customize:

- runtime archive paths
- extraction root path
- process timeout
- default profile arguments
- WCAG policy/profile file path

## Health checks

To expose runtime readiness:

```csharp
builder.Services.AddVeraPdfValidationHealthChecks();
```

## Operational docs

- `docs/api/async-validation-api.md`
- `docs/deployment/production-deployment.md`
- `docs/troubleshooting.md`
- `docs/governance/release-checklist.md`
