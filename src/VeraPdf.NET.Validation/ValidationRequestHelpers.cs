using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation;

/// <summary>
/// Provides shared helper methods for validation request setup and input checks.
/// </summary>
public static class ValidationRequestHelpers
{
    /// <summary>
    /// Resolves the selected validation standards, defaulting to all standards when none are selected.
    /// </summary>
    /// <param name="pdfA">Whether PDF/A validation is selected.</param>
    /// <param name="pdfUa">Whether PDF/UA validation is selected.</param>
    /// <param name="wcag22">Whether WCAG 2.2 validation is selected.</param>
    /// <returns>The effective validation standard flags.</returns>
    public static ValidationStandard ResolveStandards(bool pdfA, bool pdfUa, bool wcag22)
    {
        var standards = ValidationStandard.None;
        if (pdfA) standards |= ValidationStandard.PdfA;
        if (pdfUa) standards |= ValidationStandard.PdfUa;
        if (wcag22) standards |= ValidationStandard.Wcag22;

        return standards == ValidationStandard.None ? ValidationStandard.All : standards;
    }

    /// <summary>
    /// Validates PDF upload metadata and returns an error with recommended HTTP status code when invalid.
    /// </summary>
    /// <param name="pdfLength">The uploaded PDF length in bytes.</param>
    /// <param name="contentType">The uploaded PDF content type.</param>
    /// <param name="maxPdfSizeBytes">The allowed maximum upload size in bytes.</param>
    /// <returns>
    /// A tuple containing whether the input is valid, an optional validation error, and an optional HTTP status code.
    /// </returns>
    public static (bool IsValid, ValidationError? Error, int? StatusCode) ValidatePdfInput(
        long? pdfLength,
        string? contentType,
        long maxPdfSizeBytes)
    {
        if (!pdfLength.HasValue || pdfLength.Value == 0)
        {
            return (false, new ValidationError
            {
                Code = ValidationErrorCode.InvalidInput,
                Message = "A PDF file is required.",
                Target = "pdf"
            }, 400);
        }

        if (pdfLength.Value > maxPdfSizeBytes)
        {
            return (false, new ValidationError
            {
                Code = ValidationErrorCode.InvalidInput,
                Message = $"PDF exceeds max upload size of {maxPdfSizeBytes} bytes.",
                Target = "pdf"
            }, 413);
        }

        if (!IsSupportedContentType(contentType))
        {
            return (false, new ValidationError
            {
                Code = ValidationErrorCode.InvalidInput,
                Message = "Only PDF content types are supported.",
                Target = "pdf.contentType"
            }, 415);
        }

        return (true, null, null);
    }

    /// <summary>
    /// Determines whether the content type is supported for PDF uploads.
    /// </summary>
    /// <param name="contentType">The content type to validate.</param>
    /// <returns><see langword="true"/> when the content type is supported; otherwise <see langword="false"/>.</returns>
    public static bool IsSupportedContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
               || contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
    }
}
