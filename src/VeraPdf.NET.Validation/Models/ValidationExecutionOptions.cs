namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Provides optional overrides for a single validation execution.
/// </summary>
public sealed class ValidationExecutionOptions
{
    /// <summary>
    /// Gets the per-standard argument overrides used when invoking veraPDF.
    /// </summary>
    public ValidationProfileOverrides? ProfileOverrides { get; init; }

    /// <summary>
    /// Gets the WCAG 2.2 policy file path override for this execution.
    /// </summary>
    public string? Wcag22PolicyFilePathOverride { get; init; }
}
