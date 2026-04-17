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
}