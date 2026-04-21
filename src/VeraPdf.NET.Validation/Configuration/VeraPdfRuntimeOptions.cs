namespace VeraPdf.NET.Validation.Configuration;

/// <summary>
/// Configures archive locations, runtime extraction settings, and veraPDF execution behavior.
/// </summary>
public sealed class VeraPdfRuntimeOptions
{
    /// <summary>
    /// Gets or sets the path to the veraPDF runtime archive.
    /// </summary>
    public string VeraPdfZipPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "veraPDF.zip");

    /// <summary>
    /// Gets or sets the path to the Java runtime archive.
    /// </summary>
    public string JavaZipPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Java.zip");

    /// <summary>
    /// Gets or sets the expected SHA-256 hash for the veraPDF archive.
    /// </summary>
    public string? VeraPdfZipSha256 { get; set; }

    /// <summary>
    /// Gets or sets the expected SHA-256 hash for the Java archive.
    /// </summary>
    public string? JavaZipSha256 { get; set; }

    /// <summary>
    /// Gets or sets the root directory where runtime dependencies are extracted.
    /// </summary>
    public string RuntimeRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "VeraPdf.NET", "runtime");

    /// <summary>
    /// Gets or sets the maximum number of concurrent validation executions.
    /// </summary>
    public int MaxConcurrentValidations { get; set; } = 4;

    /// <summary>
    /// Gets or sets the timeout applied to each external veraPDF process invocation.
    /// </summary>
    public TimeSpan ProcessTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the default command arguments used for PDF/A validation.
    /// </summary>
    public string PdfAArguments { get; set; } = "--format json --flavour 0";

    /// <summary>
    /// Gets or sets the default command arguments used for PDF/UA validation.
    /// </summary>
    public string PdfUaArguments { get; set; } = "--format json --flavour ua1";

    /// <summary>
    /// Gets or sets the default command arguments used for WCAG 2.2 validation.
    /// </summary>
    public string Wcag22Arguments { get; set; } = "--format json --flavour wt1a";

    /// <summary>
    /// Gets or sets the default policy file path for WCAG 2.2 validation.
    /// </summary>
    public string? Wcag22PolicyFilePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "Policies", "WCAG-2-2.xml");
}
