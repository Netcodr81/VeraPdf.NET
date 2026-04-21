namespace VeraPdf.NET.Parser.Results;

public sealed class ParseDiagnostic
{
    public ParseDiagnostic(string code, string message, ParseDiagnosticSeverity severity)
    {
        Code = code;
        Message = message;
        Severity = severity;
    }

    public string Code { get; }

    public string Message { get; }

    public ParseDiagnosticSeverity Severity { get; }
}
