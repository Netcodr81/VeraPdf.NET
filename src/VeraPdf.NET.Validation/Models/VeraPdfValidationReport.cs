namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Represents the full output of a validation request, including parser diagnostics, standard reports, and errors.
/// </summary>
public sealed class VeraPdfValidationReport
{
    /// <summary>
    /// Gets the logical file name associated with the validated PDF.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the standards requested for validation.
    /// </summary>
    public required ValidationStandard RequestedStandards { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the report was generated.
    /// </summary>
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets parser precheck details, when available.
    /// </summary>
    public ParserPrecheckReport? ParserPrecheck { get; init; }

    /// <summary>
    /// Gets aggregate validation summary information.
    /// </summary>
    public ValidationReportSummary Summary { get; init; } = new();

    /// <summary>
    /// Gets errors raised during validation processing.
    /// </summary>
    public IReadOnlyList<ValidationError> Errors { get; init; } = [];

    /// <summary>
    /// Gets per-standard validation reports.
    /// </summary>
    public IReadOnlyList<StandardValidationReport> Reports { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the overall validation request passed.
    /// </summary>
    public bool Passed => (ParserPrecheck?.Success ?? true) && Reports.All(r => r.Passed) && Errors.Count == 0;
}
