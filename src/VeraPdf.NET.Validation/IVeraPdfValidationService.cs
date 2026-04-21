using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation;

/// <summary>
/// Validates PDF content against selected standards using parser prechecks and veraPDF runtime checks.
/// </summary>
public interface IVeraPdfValidationService
{
    /// <summary>
    /// Validates a PDF stream against one or more validation standards.
    /// </summary>
    /// <param name="pdfStream">The PDF content stream to validate.</param>
    /// <param name="fileName">The logical file name used in reporting.</param>
    /// <param name="standards">The validation standards to run.</param>
    /// <param name="executionOptions">Optional overrides for runtime profile arguments and policy file paths.</param>
    /// <param name="cancellationToken">A token used to cancel the validation workflow.</param>
    /// <returns>A structured validation report containing parser diagnostics and standard-level results.</returns>
    Task<VeraPdfValidationReport> ValidateAsync(
        Stream pdfStream,
        string fileName,
        ValidationStandard standards,
        ValidationExecutionOptions? executionOptions = null,
        CancellationToken cancellationToken = default);
}
