namespace VeraPdf.NET.Parser;

public sealed class PdfParseOptions
{
    public bool CaptureDiagnostics { get; init; } = true;

    public bool StrictHeaderCheck { get; init; } = true;

    public bool RequireEofMarker { get; init; } = true;

    public bool RequireStartXref { get; init; } = true;
}
