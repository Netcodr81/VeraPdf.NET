using VeraPdf.NET.Model;

namespace VeraPdf.NET.Parser.Results;

public sealed class PdfParseResult
{
    public bool Success { get; init; }

    public CosDocument? Document { get; init; }

    public IReadOnlyList<ParseDiagnostic> Diagnostics { get; init; } = [];

    public IReadOnlyList<string> DiagnosticMessages => Diagnostics.Select(d => d.Message).ToList();
}
