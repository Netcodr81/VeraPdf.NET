using VeraPdf.NET.Validation.Abstractions;

namespace VeraPdf.NET.Validation.Configuration;

/// <summary>
/// Global configuration entry point.
/// Used by higher-level library.
/// </summary>
public sealed class ValidationConfiguration
{
    public ValidationOptions Validation { get; init; } = new();

    public Profiles.ProfileOptions Profiles { get; init; } = new();
}