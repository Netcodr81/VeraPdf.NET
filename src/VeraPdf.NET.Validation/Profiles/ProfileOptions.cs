namespace VeraPdf.NET.Validation.Profiles;

/// <summary>
/// Controls how validation profiles are loaded.
/// </summary>
public sealed class ProfileOptions
{
    /// <summary>
    /// Load from XML (veraPDF original format)
    /// </summary>
    public bool UseXmlProfiles { get; init; } = true;

    /// <summary>
    /// Load from pre-converted JSON
    /// </summary>
    public bool UseJsonProfiles { get; init; } = false;

    /// <summary>
    /// Cache compiled rules
    /// </summary>
    public bool CacheCompiledRules { get; init; } = true;

    /// <summary>
    /// Load profiles from the embedded compiled JSON artifact.
    /// </summary>
    public bool UseCompiledProfileArtifact { get; init; } = true;

    /// <summary>
    /// Optional profile id from the compiled catalog.
    /// If null, the first available profile is used.
    /// </summary>
    public string? SelectedProfileId { get; init; }

    /// <summary>
    /// Skip rules that target model types not yet ported.
    /// </summary>
    public bool IgnoreRulesWithUnknownTargetType { get; init; } = true;
}