using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation;

/// <summary>
/// Provides helpers for creating validation execution options.
/// </summary>
public static class ValidationExecutionOptionsExtensions
{
    /// <summary>
    /// Creates <see cref="ValidationExecutionOptions"/> from optional per-standard argument overrides.
    /// </summary>
    /// <param name="values">Tuple containing PDF/A, PDF/UA, WCAG 2.2 arguments and WCAG policy path override.</param>
    /// <returns>A populated <see cref="ValidationExecutionOptions"/> instance, or <see langword="null"/> when no overrides are provided.</returns>
    public static ValidationExecutionOptions? ToValidationExecutionOptions(
        this (string? PdfAArgs, string? PdfUaArgs, string? Wcag22Args, string? Wcag22PolicyPath) values)
    {
        var hasProfileOverrides = !string.IsNullOrWhiteSpace(values.PdfAArgs)
                                  || !string.IsNullOrWhiteSpace(values.PdfUaArgs)
                                  || !string.IsNullOrWhiteSpace(values.Wcag22Args);

        var hasPolicyOverride = !string.IsNullOrWhiteSpace(values.Wcag22PolicyPath);
        if (!hasProfileOverrides && !hasPolicyOverride)
        {
            return null;
        }

        return new ValidationExecutionOptions
        {
            Wcag22PolicyFilePathOverride = hasPolicyOverride ? values.Wcag22PolicyPath : null,
            ProfileOverrides = hasProfileOverrides
                ? new ValidationProfileOverrides
                {
                    PdfAArguments = values.PdfAArgs,
                    PdfUaArguments = values.PdfUaArgs,
                    Wcag22Arguments = values.Wcag22Args
                }
                : null
        };
    }
}
