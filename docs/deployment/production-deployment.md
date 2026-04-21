# Production Deployment Guide

## Runtime prerequisites
- Deploy with writable temp storage for runtime extraction and PDF temp files.
- Configure archive checksums (`VeraPdfZipSha256`, `JavaZipSha256`) in production.
- Ensure host process has permission to execute extracted binaries.

## API hardening settings
- Enforce inbound request limits at reverse proxy and API (`MultipartBodyLengthLimit`).
- Keep health endpoint available: `/health`.
- Enable centralized logging and metrics ingestion for validation counters/histograms.

## Scale guidance
- Tune `MaxConcurrentValidations` per node CPU/memory profile.
- Use async job endpoints for high-latency or burst validation workloads.
- Run multiple instances behind load balancer; treat async jobs as node-local in-memory unless externalized.
