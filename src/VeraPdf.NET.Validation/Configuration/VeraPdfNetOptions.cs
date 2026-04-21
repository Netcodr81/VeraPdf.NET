using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation.Configuration;

/// <summary>
/// Provides top-level configuration for VeraPdf.NET package registration.
/// </summary>
public sealed class VeraPdfNetOptions
{
    /// <summary>
    /// Gets or sets runtime provisioning and process execution options.
    /// </summary>
    public VeraPdfRuntimeOptions Runtime { get; set; } = new();

    /// <summary>
    /// Gets or sets default per-request execution overrides applied when request values are not provided.
    /// </summary>
    public ValidationExecutionOptions Execution { get; set; } = new();
}
