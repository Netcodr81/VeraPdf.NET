using VeraPdf.NET.Model;

namespace VeraPdf.NET.Parser.Results;

/// <summary>
/// Represents the outcome of a PDF parse operation.
/// </summary>
public sealed class PdfParseResult
{
    /// <summary>
    /// Gets a value indicating whether parsing completed without error-level diagnostics.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the mapped COS document when parsing succeeds.
    /// </summary>
    public CosDocument? Document { get; init; }

    /// <summary>
    /// Gets parse diagnostics discovered during scanning.
    /// </summary>
    public IReadOnlyList<ParseDiagnostic> Diagnostics { get; init; } = [];
}
