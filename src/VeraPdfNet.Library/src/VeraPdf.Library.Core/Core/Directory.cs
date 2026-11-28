namespace VeraPdf.Library.Core;

public interface Directory<TKey, TValue> where TKey : notnull
{
    /// <summary>Returns the value for a key, or the default value when the key is absent.</summary>
    TValue? GetItem(TKey key);
    IReadOnlyList<TValue> GetItems();
    IReadOnlySet<TKey> GetKeys();
    int Size { get; }
    bool IsEmpty { get; }
}

public interface Registry<TKey, TValue> : Directory<TKey, TValue> where TKey : notnull
{
    /// <summary>Registers a value and returns the previous value, or default if none existed.</summary>
    TValue? RegisterItem(TKey key, TValue value);
    void RegisterItems(IEnumerable<KeyValuePair<TKey, TValue>> items);
    /// <summary>Removes a value and returns it, or default if the key was absent.</summary>
    TValue? RemoveItem(TKey key);
    void RemoveItems(IEnumerable<TKey> keys);
    /// <summary>Sets a value and returns the previous value, or default if none existed.</summary>
    TValue? PutdateItem(TKey key, TValue value);
    void PutdateItems(IEnumerable<KeyValuePair<TKey, TValue>> items);
    TValue UpdateItem(TKey key, TValue value);
    void UpdateItems(IEnumerable<KeyValuePair<TKey, TValue>> items);
}

public class MapBackedDirectory<TKey, TValue> : Directory<TKey, TValue>, IEquatable<Directory<TKey, TValue>>
    where TKey : notnull
{
    protected readonly Dictionary<TKey, TValue> Map;

    public MapBackedDirectory()
        : this(new Dictionary<TKey, TValue>())
    {
    }

    public MapBackedDirectory(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        Map = new Dictionary<TKey, TValue>();
        foreach (var item in items)
            Map[item.Key] = item.Value;
    }

    public TValue? GetItem(TKey key) => Map.TryGetValue(key, out var value) ? value : default;

    public IReadOnlyList<TValue> GetItems() => Array.AsReadOnly(Map.Values.ToArray());

    public IReadOnlySet<TKey> GetKeys() => new HashSet<TKey>(Map.Keys);

    public int Size => Map.Count;

    public bool IsEmpty => Map.Count == 0;

    public bool Equals(Directory<TKey, TValue>? other) =>
        other is not null &&
        GetItems().SequenceEqual(other.GetItems()) &&
        GetKeys().SetEquals(other.GetKeys());

    public override bool Equals(object? obj) => obj is Directory<TKey, TValue> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var item in GetItems())
            hash.Add(item);
        var keysHash = 0;
        foreach (var key in Map.Keys)
            keysHash ^= EqualityComparer<TKey>.Default.GetHashCode(key);
        hash.Add(keysHash);
        return hash.ToHashCode();
    }
}

public sealed class MapBackedRegistry<TKey, TValue> : MapBackedDirectory<TKey, TValue>, Registry<TKey, TValue>
    where TKey : notnull
{
    public MapBackedRegistry()
    {
    }

    public MapBackedRegistry(IEnumerable<KeyValuePair<TKey, TValue>> items)
        : base(items)
    {
    }

    public TValue? RegisterItem(TKey key, TValue value) => PutdateItem(key, value);

    public void RegisterItems(IEnumerable<KeyValuePair<TKey, TValue>> items) => PutdateItems(items);

    public TValue? RemoveItem(TKey key)
    {
        if (!Map.Remove(key, out var value)) return default;
        return value;
    }

    public void RemoveItems(IEnumerable<TKey> keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        foreach (var key in keys)
            Map.Remove(key);
    }

    public TValue? PutdateItem(TKey key, TValue value)
    {
        var previous = Map.TryGetValue(key, out var existing) ? existing : default;
        Map[key] = value;
        return previous;
    }

    public void PutdateItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        foreach (var item in items)
            Map[item.Key] = item.Value;
    }

    public TValue UpdateItem(TKey key, TValue value)
    {
        if (!Map.TryGetValue(key, out var previous))
            throw new KeyNotFoundException($"No item is registered for key '{key}'.");
        Map[key] = value;
        return previous;
    }

    public void UpdateItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var updates = items.ToArray();
        foreach (var item in updates)
            if (!Map.ContainsKey(item.Key))
                throw new KeyNotFoundException($"No item is registered for key '{item.Key}'.");
        foreach (var item in updates)
            Map[item.Key] = item.Value;
    }
}
