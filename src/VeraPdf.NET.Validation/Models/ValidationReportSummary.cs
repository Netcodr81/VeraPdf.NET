namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Summarizes aggregate outcomes for a validation run.
/// </summary>
public sealed class ValidationReportSummary
{
    /// <summary>
    /// Gets the number of standards that were evaluated.
    /// </summary>
    public int TotalStandardsChecked { get; init; }

    /// <summary>
    /// Gets the number of standards that passed.
    /// </summary>
    public int PassedStandards { get; init; }

    /// <summary>
    /// Gets the number of standards that failed.
    /// </summary>
    public int FailedStandards { get; init; }

    /// <summary>
    /// Gets a value indicating whether parser precheck errors were detected.
    /// </summary>
    public bool HasParserErrors { get; init; }

    /// <summary>
    /// Gets the names of standards that failed validation.
    /// </summary>
    public IReadOnlyList<string> FailedStandardsList { get; init; } = [];
}
