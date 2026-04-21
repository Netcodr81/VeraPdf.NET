namespace VeraPdf.NET.Parser.Results;

/// <summary>
/// Defines severity levels used by parser diagnostics.
/// </summary>
public enum ParseDiagnosticSeverity
{
    /// <summary>
    /// Informational diagnostic.
    /// </summary>
    Info = 0,
    /// <summary>
    /// Warning diagnostic that does not necessarily fail parsing.
    /// </summary>
    Warning = 1,
    /// <summary>
    /// Error diagnostic that causes parse failure.
    /// </summary>
    Error = 2
}
