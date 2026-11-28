namespace VeraPdf.Library.Xmp;

/// <summary>Optional XML recovery applied only after the initial XML parse fails.</summary>
public sealed record XmpRecoveryOptions
{
    /// <summary>Repair ASCII controls and numeric control-character references.</summary>
    public bool FixControlCharacters { get; init; }

    /// <summary>Repair invalid UTF-8 bytes using the reference's Windows-1252 fallback.</summary>
    public bool AcceptLatin1 { get; init; }
}
