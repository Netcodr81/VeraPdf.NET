namespace VeraPdf.NET.Parser.ModelMapping;

/// <summary>
/// Represents extracted structural signals from raw PDF content.
/// </summary>
public sealed class ParsedPdfSnapshot
{
    /// <summary>
    /// Gets a value indicating whether a PDF header marker was detected.
    /// </summary>
    public bool HasHeader { get; init; }

    /// <summary>
    /// Gets a value indicating whether an EOF marker was detected.
    /// </summary>
    public bool HasEofMarker { get; init; }

    /// <summary>
    /// Gets a value indicating whether a startxref section was detected.
    /// </summary>
    public bool HasStartXref { get; init; }

    /// <summary>
    /// Gets a value indicating whether a catalog object marker was detected.
    /// </summary>
    public bool HasCatalogObject { get; init; }

    /// <summary>
    /// Gets a value indicating whether an /Info entry marker was detected.
    /// </summary>
    public bool HasInfoEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether a /PieceInfo entry marker was detected.
    /// </summary>
    public bool HasPieceInfoEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether an /EmbeddedFiles entry marker was detected.
    /// </summary>
    public bool HasEmbeddedFilesEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether a /MarkInfo entry marker was detected.
    /// </summary>
    public bool HasMarkInfoEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether /Marked true was detected.
    /// </summary>
    public bool MarkedTrue { get; init; }

    /// <summary>
    /// Gets a value indicating whether /Suspects true was detected.
    /// </summary>
    public bool SuspectsTrue { get; init; }

    /// <summary>
    /// Gets a value indicating whether a /ViewerPreferences entry marker was detected.
    /// </summary>
    public bool HasViewerPreferencesEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether /DisplayDocTitle true was detected.
    /// </summary>
    public bool DisplayDocTitleTrue { get; init; }

    /// <summary>
    /// Gets a value indicating whether a /Metadata entry marker was detected.
    /// </summary>
    public bool HasMetadataEntry { get; init; }

    /// <summary>
    /// Gets a value indicating whether an xref stream marker was detected.
    /// </summary>
    public bool HasXRefStream { get; init; }

    /// <summary>
    /// Gets a value indicating whether a linearized PDF marker was detected.
    /// </summary>
    public bool IsLinearized { get; init; }

    /// <summary>
    /// Gets the matched PDF header value.
    /// </summary>
    public string? Header { get; init; }

    /// <summary>
    /// Gets the parsed PDF version from the header.
    /// </summary>
    public decimal HeaderVersion { get; init; }

    /// <summary>
    /// Gets the byte offset where the header was found.
    /// </summary>
    public int HeaderOffset { get; init; }

    /// <summary>
    /// Gets the first binary comment byte following the header line.
    /// </summary>
    public int HeaderByte1 { get; init; }

    /// <summary>
    /// Gets the second binary comment byte following the header line.
    /// </summary>
    public int HeaderByte2 { get; init; }

    /// <summary>
    /// Gets the third binary comment byte following the header line.
    /// </summary>
    public int HeaderByte3 { get; init; }

    /// <summary>
    /// Gets the fourth binary comment byte following the header line.
    /// </summary>
    public int HeaderByte4 { get; init; }

    /// <summary>
    /// Gets the size of trailing data detected after the final EOF marker.
    /// </summary>
    public int PostEofDataSize { get; init; }

    /// <summary>
    /// Gets the number of indirect object declarations detected.
    /// </summary>
    public int IndirectObjectCount { get; init; }

    /// <summary>
    /// Gets the parsed startxref offset when available.
    /// </summary>
    public int StartXrefOffset { get; init; }
}
