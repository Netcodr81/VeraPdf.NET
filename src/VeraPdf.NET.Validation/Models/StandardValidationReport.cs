namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Contains the result of validating a PDF against a single standard profile.
/// </summary>
public sealed class StandardValidationReport
{
    /// <summary>
    /// Gets the display name of the validation standard.
    /// </summary>
    public required string Standard { get; init; }

    /// <summary>
    /// Gets a value indicating whether validation passed for this standard.
    /// </summary>
    public bool Passed { get; init; }

    /// <summary>
    /// Gets the process exit code returned by the underlying validator.
    /// </summary>
    public int ExitCode { get; init; }

    /// <summary>
    /// Gets a human-readable description of the process exit code.
    /// </summary>
    public string ExitCodeDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether execution timed out.
    /// </summary>
    public bool TimedOut { get; init; }

    /// <summary>
    /// Gets the normalized error code representing the execution outcome.
    /// </summary>
    public ValidationErrorCode ErrorCode { get; init; }

    /// <summary>
    /// Gets a human-readable description of the normalized validation error code.
    /// </summary>
    public string ErrorCodeDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the raw JSON report emitted by the validator.
    /// </summary>
    public string JsonReport { get; init; } = "{}";

    /// <summary>
    /// Gets the parsed, strongly-typed veraPDF report when JSON parsing succeeds.
    /// </summary>
    public VeraPdfCliReport? ParsedReport { get; init; }

    /// <summary>
    /// Gets the standard error output captured from the process, when available.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
