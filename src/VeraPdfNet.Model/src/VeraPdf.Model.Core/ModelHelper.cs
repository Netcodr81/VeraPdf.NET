using System.Collections.Frozen;
using System.Text.Json;

namespace VeraPdf.Model;

/// <summary>Schema lookups corresponding to upstream ModelHelper, without mutable global lists.</summary>
public static class ModelHelper
{
    private static readonly FrozenDictionary<string, ModelTypeDefinition> Definitions = Load();
    private static readonly FrozenSet<string> Types = Definitions.Keys.ToFrozenSet(StringComparer.Ordinal);

    /// <summary>Gets the upstream generator revision used to verify API mapping.</summary>
    public const string GeneratorCommit = "f8e9f87d98a96784cd78a15229736722bb77eff8";

    /// <summary>Gets all original simple model type names. No iteration order is guaranteed.</summary>
    public static IReadOnlySet<string> GetTypes() => Types;

    /// <summary>Gets immutable declared metadata by exact, case-sensitive simple name.</summary>
    /// <exception cref="ArgumentException">The name is not a known model type.</exception>
    public static ModelTypeDefinition GetDefinition(string objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        return Definitions.TryGetValue(objectName, out var definition)
            ? definition
            : throw new ArgumentException($"Unknown model type '{objectName}'.", nameof(objectName));
    }

    /// <summary>Gets all ancestor names, nearest ancestor first; Object has no ancestors.</summary>
    public static IReadOnlyList<string> GetListOfSuperNames(string objectName) =>
        Array.AsReadOnly(Hierarchy(objectName).Skip(1).Select(d => d.Name).ToArray());

    /// <summary>Gets properties in derived-to-base order, retaining each declaration's order.</summary>
    public static IReadOnlyList<string> GetListOfProperties(string objectName) =>
        Array.AsReadOnly(Hierarchy(objectName).SelectMany(d => d.Properties).Select(p => p.Name).ToArray());

    /// <summary>Gets named links in derived-to-base order, retaining each declaration's order.</summary>
    public static IReadOnlyList<string> GetListOfLinks(string objectName) =>
        Array.AsReadOnly(Hierarchy(objectName).SelectMany(d => d.Links).Select(l => l.Name).ToArray());

    private static IEnumerable<ModelTypeDefinition> Hierarchy(string objectName)
    {
        var current = GetDefinition(objectName);
        while (true)
        {
            yield return current;
            if (current.BaseName is null) yield break;
            current = GetDefinition(current.BaseName);
        }
    }

    private static FrozenDictionary<string, ModelTypeDefinition> Load()
    {
        using var stream = typeof(ModelHelper).Assembly.GetManifestResourceStream("VeraPdf.Model.Core.Generated.schema.json")
            ?? throw new InvalidOperationException("The model package is missing its generated schema resource.");
        using var document = JsonDocument.Parse(stream);
        if (document.RootElement.GetProperty("GeneratorCommit").GetString() != GeneratorCommit)
        {
            throw new InvalidOperationException("The embedded model schema has incompatible generator provenance.");
        }
        var definitions = new List<ModelTypeDefinition>();
        foreach (var item in document.RootElement.GetProperty("Types").EnumerateArray())
        {
            var properties = item.GetProperty("Properties").EnumerateArray().Select(p =>
                new ModelPropertyDefinition(Text(p, "Name"), Text(p, "DslType"), Text(p, "ClrType")));
            var links = item.GetProperty("Links").EnumerateArray().Select(l =>
                new ModelLinkDefinition(Text(l, "Name"), Text(l, "Target"), Text(l, "Multiplicity")));
            definitions.Add(new ModelTypeDefinition(Text(item, "Name"), Text(item, "JavaName"),
                Text(item, "ClrName"), Text(item, "Source"), item.GetProperty("BaseName").GetString(), properties, links));
        }
        return definitions.ToFrozenDictionary(d => d.Name, StringComparer.Ordinal);
    }

    private static string Text(JsonElement element, string name) =>
        element.GetProperty(name).GetString() ?? throw new InvalidOperationException($"Missing schema field '{name}'.");
}
