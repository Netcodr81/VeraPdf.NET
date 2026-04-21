namespace VeraPdf.NET.Parser;

/// <summary>
/// Controls strictness for structural checks performed during PDF parsing.
/// </summary>
public sealed class PdfParseOptions
{
    /// <summary>
    /// Gets a value indicating whether the PDF header must appear at byte offset zero.
    /// </summary>
    public bool StrictHeaderCheck { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether a missing EOF marker should be reported as an error.
    /// </summary>
    public bool RequireEofMarker { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether a missing startxref section should be reported as an error.
    /// </summary>
    public bool RequireStartXref { get; init; } = true;
}
