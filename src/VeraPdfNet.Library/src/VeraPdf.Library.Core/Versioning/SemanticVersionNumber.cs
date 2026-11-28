using System.Globalization;

namespace VeraPdf.Library.Versioning;

public interface SemanticVersionNumber : IComparable<SemanticVersionNumber>
{
    string VersionString { get; }
    int Major { get; }
    int Minor { get; }
    int Patch { get; }
}

public static class Versions
{
    public static SemanticVersionNumber FromString(string versionString)
    {
        ArgumentNullException.ThrowIfNull(versionString);
        if (versionString.Length == 0)
            throw new ArgumentException("Version string cannot be empty.", nameof(versionString));

        var core = versionString;
        if (core.EndsWith("-SNAPSHOT", StringComparison.Ordinal))
            core = core[..^"-SNAPSHOT".Length];
        if (core.StartsWith('v'))
            core = core[1..];
        var metadataIndex = core.IndexOf('+');
        if (metadataIndex >= 0) core = core[..metadataIndex];
        var prereleaseIndex = core.IndexOf('-');
        if (prereleaseIndex >= 0) core = core[..prereleaseIndex];
        var parts = core.Split('.');
        var partCount = parts.Length;
        while (partCount > 0 && parts[partCount - 1].Length == 0) partCount--;
        if (partCount == 0)
            throw new ArgumentException("Version string does not contain numeric components.", nameof(versionString));
        return FromStrings(parts.Take(partCount).ToArray());
    }

    public static SemanticVersionNumber FromStrings(IReadOnlyList<string> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        var numbers = new int[3];
        for (var index = 0; index < numbers.Length && index < parts.Count; index++)
        {
            if (!int.TryParse(parts[index], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out numbers[index]))
                throw new ArgumentException($"Version component '{parts[index]}' is not a valid integer.", nameof(parts));
            if (numbers[index] < 0)
                throw new ArgumentException($"Version component '{parts[index]}' cannot be negative.", nameof(parts));
        }
        return FromInts(numbers[0], numbers[1], numbers[2]);
    }

    public static SemanticVersionNumber FromInts(IReadOnlyList<int> parts)
    {
        ArgumentNullException.ThrowIfNull(parts);
        if (parts.Count != 3)
            throw new ArgumentException("Version components must contain exactly three integers.", nameof(parts));
        return FromInts(parts[0], parts[1], parts[2]);
    }

    public static SemanticVersionNumber FromInts(int major, int minor, int patch)
    {
        if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), "Version components cannot be negative.");
        if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), "Version components cannot be negative.");
        if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), "Version components cannot be negative.");
        return new VersionNumber(major, minor, patch);
    }

    private sealed class VersionNumber(int major, int minor, int patch) : SemanticVersionNumber
    {
        public string VersionString => string.Create(CultureInfo.InvariantCulture, $"{major}.{minor}.{patch}");
        public int Major => major;
        public int Minor => minor;
        public int Patch => patch;

        public int CompareTo(SemanticVersionNumber? other)
        {
            ArgumentNullException.ThrowIfNull(other);
            var result = Major.CompareTo(other.Major);
            if (result != 0) return result;
            result = Minor.CompareTo(other.Minor);
            return result != 0 ? result : Patch.CompareTo(other.Patch);
        }

        public override bool Equals(object? obj) =>
            obj is VersionNumber other && Major == other.Major && Minor == other.Minor && Patch == other.Patch;

        public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch);

        public override string ToString() => VersionString;
    }
}
