using System.Reflection;

namespace VeraPdf.Library.Profiles;

/// <summary>Access to the profile XML included in the pinned veraPDF snapshot.</summary>
public static class BuiltInProfiles
{
    private const string Prefix = "VeraPdf.Library.Profiles.";
    private static readonly Assembly Assembly = typeof(BuiltInProfiles).Assembly;
    private static readonly IReadOnlyList<string> ProfileNames = Array.AsReadOnly(
        Assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(Prefix, StringComparison.Ordinal) && name.EndsWith(".xml", StringComparison.Ordinal))
            .Select(name => name[Prefix.Length..^4])
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray());
    private static readonly Lazy<IReadOnlyDictionary<string, string>> NamesByFlavour = new(CreateFlavourIndex);

    public static IReadOnlyList<string> Names => ProfileNames;

    /// <summary>Reads a bundled profile by its exact filename without the XML extension.</summary>
    public static ValidationProfile Read(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!ProfileNames.Contains(name, StringComparer.Ordinal))
            throw new ArgumentException($"Unknown built-in profile '{name}'.", nameof(name));
        using var stream = Assembly.GetManifestResourceStream(Prefix + name + ".xml")
            ?? throw new InvalidOperationException($"Embedded profile '{name}' is missing.");
        return ProfileReader.Read(stream);
    }

    /// <summary>Reads a bundled profile by its exact upstream flavour identifier.</summary>
    public static ValidationProfile ReadByFlavour(string flavour)
    {
        ArgumentNullException.ThrowIfNull(flavour);
        if (!NamesByFlavour.Value.TryGetValue(flavour, out var name))
            throw new KeyNotFoundException($"No bundled profile has flavour '{flavour}'.");
        return Read(name);
    }

    private static IReadOnlyDictionary<string, string> CreateFlavourIndex()
    {
        var index = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var name in ProfileNames)
        {
            var profile = Read(name);
            if (profile.Flavour is null)
                throw new InvalidDataException($"Bundled profile '{name}' has no flavour.");
            if (!index.TryAdd(profile.Flavour, name))
                throw new InvalidDataException($"More than one bundled profile has flavour '{profile.Flavour}'.");
        }
        return index;
    }
}
