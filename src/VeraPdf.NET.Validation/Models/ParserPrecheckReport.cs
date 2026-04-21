namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Captures parser precheck success state and diagnostics before external validation runs.
/// </summary>
public sealed class ParserPrecheckReport
{
    /// <summary>
    /// Gets a value indicating whether parser precheck completed without errors.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets parser diagnostics reported during precheck.
    /// </summary>
    public IReadOnlyList<ParserDiagnosticReport> Diagnostics { get; init; } = [];
}
