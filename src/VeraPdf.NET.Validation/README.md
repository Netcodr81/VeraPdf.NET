# VeraPdf.NET.Validation

Portable .NET wrapper around veraPDF for PDF-A, PDF-UA, and WCAG 2.2 validation workflows.

## Features
- Portable runtime provisioning from bundled `veraPDF.zip` and `Java.zip`
- Parser precheck with diagnostics before external CLI execution
- Structured error codes and normalized summary in validation report
- Optional per-request profile override arguments
- Optional per-request WCAG policy file override

## Sample API capabilities
- Synchronous validation endpoint (`/api/validation`)
- Asynchronous job endpoints (`/api/validation/jobs`, status, and result)
- Runtime health endpoint (`/health`)

## Operational docs
- `docs/api/async-validation-api.md`
- `docs/deployment/production-deployment.md`
- `docs/troubleshooting.md`
- `docs/governance/release-checklist.md`
