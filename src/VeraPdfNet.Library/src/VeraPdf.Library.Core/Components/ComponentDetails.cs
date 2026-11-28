namespace VeraPdf.Library.Components;

public sealed class ComponentDetails : IEquatable<ComponentDetails>
{
    public static Uri DefaultId { get; } = new("http://component.verapdf.org#default");

    public static ComponentDetails Default { get; } =
        new(DefaultId, "name", "version", "provider", "description");

    private ComponentDetails(Uri? id, string? name, string? version, string? provider, string? description)
    {
        Id = id;
        Name = name;
        Version = version;
        Provider = provider;
        Description = description;
    }

    public Uri? Id { get; }
    public string? Name { get; }
    public string? Version { get; }
    public string? Provider { get; }
    public string? Description { get; }

    public static ComponentDetails FromValues(
        Uri? id, string? name, string? version, string? provider, string? description)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(version);
        ArgumentException.ThrowIfNullOrEmpty(provider);
        ArgumentException.ThrowIfNullOrEmpty(description);
        return new(id, name, version, provider, description);
    }

    public bool Equals(ComponentDetails? other) =>
        other is not null &&
        Equals(Id, other.Id) &&
        Name == other.Name &&
        Version == other.Version &&
        Provider == other.Provider &&
        Description == other.Description;

    public override bool Equals(object? obj) => obj is ComponentDetails other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, Name, Version, Provider, Description);
}
