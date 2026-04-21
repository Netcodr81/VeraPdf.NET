# Signing and Attestation Notes

This repository includes workflow scaffolding for governance checks.

## Package signing
Integrate organization-approved signing infrastructure in `.github/workflows/release-governance.yml` before production release tagging.

## Provenance/attestation
Publish build provenance and attestation artifacts in your enterprise artifact system when releasing NuGet packages.

## Minimum release evidence
- Release pipeline run id
- Commit SHA
- SBOM manifest artifact
- Signing/attestation record references
