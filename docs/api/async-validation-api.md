# Async Validation API

## Submit Job
`POST /api/validation/jobs` (multipart/form-data)

Form fields:
- `pdf` (required)
- `pdfA`, `pdfUa`, `wcag22` (optional booleans; defaults to all if none selected)
- `pdfAArgs`, `pdfUaArgs`, `wcag22Args` (optional CLI override strings)
- `wcag22PolicyPath` (optional WCAG policy file path override)

Response: `202 Accepted`
- `jobId`
- `status`
- `createdAtUtc`

## Check Job Status
`GET /api/validation/jobs/{jobId}`

Response: current state (`Pending`, `Running`, `Completed`, `Failed`) and metadata.

## Fetch Job Result
`GET /api/validation/jobs/{jobId}/result`

- `200 OK`: validation report payload when completed/failed with report
- `409 Conflict`: still processing
- `404 NotFound`: unknown job id
