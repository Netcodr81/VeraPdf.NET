namespace VeraPdf.NET.Model;

/// <summary>
/// Represents the parsed COS-level metadata and structural signals for a PDF document.
/// </summary>
public sealed class CosDocument
{
    /// <summary>
    /// Gets or sets the detected PDF header value.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// Gets or sets the byte offset where the PDF header was found.
    /// </summary>
    public int HeaderOffset { get; set; }

    /// <summary>
    /// Gets or sets the parsed PDF version from the header.
    /// </summary>
    public decimal HeaderVersion { get; set; }

    /// <summary>
    /// Gets or sets the first binary comment byte after the header line.
    /// </summary>
    public int HeaderByte1 { get; set; }

    /// <summary>
    /// Gets or sets the second binary comment byte after the header line.
    /// </summary>
    public int HeaderByte2 { get; set; }

    /// <summary>
    /// Gets or sets the third binary comment byte after the header line.
    /// </summary>
    public int HeaderByte3 { get; set; }

    /// <summary>
    /// Gets or sets the fourth binary comment byte after the header line.
    /// </summary>
    public int HeaderByte4 { get; set; }

    /// <summary>
    /// Gets or sets the number of trailing bytes after the last EOF marker.
    /// </summary>
    public int PostEOFDataSize { get; set; }

    /// <summary>
    /// Gets or sets the number of indirect object declarations detected.
    /// </summary>
    public int NrIndirects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is linearized.
    /// </summary>
    public bool IsLinearized { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document includes an /Info entry.
    /// </summary>
    public bool ContainsInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document includes a /PieceInfo entry.
    /// </summary>
    public bool ContainsPieceInfo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document includes an /EmbeddedFiles entry.
    /// </summary>
    public bool ContainsEmbeddedFiles { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document advertises MarkInfo /Marked true.
    /// </summary>
    public bool Marked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document advertises MarkInfo /Suspects true.
    /// </summary>
    public bool Suspects { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether viewer preferences include /DisplayDocTitle true.
    /// </summary>
    public bool DisplayDocTitle { get; set; }

    /// <summary>
    /// Gets or sets higher-level document metadata flags.
    /// </summary>
    public PDDocument? Document { get; set; }
}
