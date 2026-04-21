namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Specifies per-standard command argument overrides for validation runs.
/// </summary>
public sealed class ValidationProfileOverrides
{
    /// <summary>
    /// Gets the PDF/A command arguments override.
    /// </summary>
    public string? PdfAArguments { get; init; }

    /// <summary>
    /// Gets the PDF/UA command arguments override.
    /// </summary>
    public string? PdfUaArguments { get; init; }

    /// <summary>
    /// Gets the WCAG 2.2 command arguments override.
    /// </summary>
    public string? Wcag22Arguments { get; init; }
}
