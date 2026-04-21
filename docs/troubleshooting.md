# Troubleshooting

## Runtime unavailable
- Symptom: error code `RuntimeUnavailable`
- Check `veraPDF.zip` and `Java.zip` paths and checksum settings.
- Verify extraction root is writable.

## Process timeout
- Symptom: error code `ProcessTimeout`
- Increase `ProcessTimeout`.
- Use async job endpoint for large PDFs.

## Invalid PDF precheck
- Symptom: error code `InvalidPdf`, no standard reports generated
- Parser precheck failed on structural markers before CLI validation.
- Inspect `ParserPrecheck.Diagnostics` in response payload.

## Async job stuck in pending/running
- Verify hosted service registration for `ValidationJobProcessor`.
- Confirm app process is healthy and not shutting down.
