namespace VeraPdf.NET.Parser.Results;

/// <summary>
/// Represents a single parser diagnostic with code, message, and severity.
/// </summary>
/// <param name="Code">The diagnostic code identifier.</param>
/// <param name="Message">The diagnostic message text.</param>
/// <param name="Severity">The diagnostic severity level.</param>
public sealed record ParseDiagnostic(string Code, string Message, ParseDiagnosticSeverity Severity);
