using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using VeraPdf.NET.Validation.Profiles;
using VeraPdf.NET.Validation.RuleSets.Normalization;

namespace VeraPdf.NET.Validation.RuleSets.Loading;

internal sealed class RuleSetProvider
{
    private const string ResourceName = "VeraPdf.NET.ValidationProfiles.verapdf-profiles.compiled.json";

    private readonly RuleSetBuilder _builder;
    private readonly RuleSetCache _cache;
    private readonly Dictionary<string, CompiledProfileEntry> _entries;

    public RuleSetProvider(RuleSetBuilder builder, RuleSetCache cache)
    {
        _builder = builder;
        _cache = cache;
        _entries = LoadManifestEntries();
    }

    public IReadOnlyCollection<string> GetAvailableProfileIds() => _entries.Keys.ToList();

    public ValidationProfile GetProfile(string profileId, bool ignoreUnknownTargetTypes)
    {
        var key = RuleNormalizer.NormalizeProfileId(profileId);

        if (!_entries.TryGetValue(key, out var entry))
            throw new InvalidOperationException($"Validation profile '{profileId}' was not found in the compiled profile artifact.");

        return _cache.GetOrAdd(key, _ => _builder.Build(key, entry.Xml, ignoreUnknownTargetTypes));
    }

    private static Dictionary<string, CompiledProfileEntry> LoadManifestEntries()
    {
        var assembly = typeof(VeraPdf.NET.ValidationProfiles.ProfileArtifactMarker).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Compiled profile artifact '{ResourceName}' was not found in assembly '{assembly.GetName().Name}'.");

        var manifest = JsonSerializer.Deserialize<CompiledProfileManifest>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException("Compiled profile artifact is invalid.");

        return manifest.Profiles
            .Where(p => !string.IsNullOrWhiteSpace(p.Id) && !string.IsNullOrWhiteSpace(p.Xml))
            .ToDictionary(
                p => RuleNormalizer.NormalizeProfileId(p.Id),
                p => p,
                StringComparer.OrdinalIgnoreCase);
    }
}
