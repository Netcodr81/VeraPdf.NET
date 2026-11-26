namespace VeraPdf.Model;

/// <summary>One declared scalar property, retaining its original DSL and CLR mapping.</summary>
public sealed record ModelPropertyDefinition(
    string Name, string DslType, string ClrType);

/// <summary>A named traversal relationship, not a generated typed getter.</summary>
/// <param name="Name">Exact rule-facing link name.</param>
/// <param name="Target">Original simple name of its target model type.</param>
/// <param name="Multiplicity">DSL suffix: empty for one, ? optional, * zero-or-more, + one-or-more.</param>
public sealed record ModelLinkDefinition(
    string Name, string Target, string Multiplicity);

/// <summary>Immutable declaration metadata for a model interface.</summary>
public sealed class ModelTypeDefinition
{
    internal ModelTypeDefinition(
        string name, string javaName, string clrName, string source, string? baseName,
        IEnumerable<ModelPropertyDefinition> properties, IEnumerable<ModelLinkDefinition> links)
    {
        Name = name;
        JavaName = javaName;
        ClrName = clrName;
        Source = source;
        BaseName = baseName;
        Properties = Array.AsReadOnly(properties.ToArray());
        Links = Array.AsReadOnly(links.ToArray());
    }

    /// <summary>Original simple model name, used for rule lookup.</summary>
    public string Name { get; }
    /// <summary>Original fully qualified Java interface name.</summary>
    public string JavaName { get; }
    /// <summary>Fully qualified ported C# interface name.</summary>
    public string ClrName { get; }
    /// <summary>Authoritative .mdl source filename.</summary>
    public string Source { get; }
    /// <summary>Original simple name of the immediate ancestor, or null for Object.</summary>
    public string? BaseName { get; }
    /// <summary>Declared properties, excluding inherited properties, in source order.</summary>
    public IReadOnlyList<ModelPropertyDefinition> Properties { get; }
    /// <summary>Declared links, excluding inherited links, in source order.</summary>
    public IReadOnlyList<ModelLinkDefinition> Links { get; }
}
