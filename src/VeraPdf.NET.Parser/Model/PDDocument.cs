namespace VeraPdf.NET.Model;

/// <summary>
/// Represents selected high-level PDF document catalog characteristics.
/// </summary>
public sealed class PDDocument
{
    /// <summary>
    /// Gets or sets a value indicating whether document metadata is present.
    /// </summary>
    public bool ContainsMetadata { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an xref stream is present.
    /// </summary>
    public bool ContainsXRefStream { get; set; }
}
