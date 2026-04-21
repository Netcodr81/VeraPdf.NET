namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Represents a parser diagnostic entry captured during PDF precheck.
/// </summary>
public sealed class ParserDiagnosticReport
{
    /// <summary>
    /// Gets the diagnostic code.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the diagnostic severity level.
    /// </summary>
    public required string Severity { get; init; }
}
