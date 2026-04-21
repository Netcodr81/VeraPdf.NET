namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Provides optional overrides for a single validation execution.
/// </summary>
public sealed class ValidationExecutionOptions
{
    /// <summary>
    /// Gets or sets the per-standard argument overrides used when invoking veraPDF.
    /// </summary>
    public ValidationProfileOverrides? ProfileOverrides { get; set; }

    /// <summary>
    /// Gets or sets the WCAG 2.2 policy file path override for this execution.
    /// </summary>
    public string? Wcag22PolicyFilePathOverride { get; set; }
}
