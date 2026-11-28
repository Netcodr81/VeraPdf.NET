using System.Globalization;
using VeraPdf.Library.Core;

namespace VeraPdf.Library.Core.Utils;

public sealed class VersioningMapper : IFileOutputMapper
{
    private const string VersionStart = "_(";
    private const string VersionEnd = ")";

    public static VersioningMapper DefaultInstance { get; } =
        new(RelativeDirectoryMapper.DefaultInstance);

    private readonly IFileOutputMapper _mapper;

    private VersioningMapper(IFileOutputMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        _mapper = mapper;
    }

    public string Prefix => _mapper.Prefix;
    public string Suffix => _mapper.Suffix;

    public static VersioningMapper FromMapper(IFileOutputMapper mapper) => new(mapper);

    public FileInfo MapFile(FileInfo original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var mapped = _mapper.MapFile(original);
        if (!File.Exists(mapped.FullName) && !Directory.Exists(mapped.FullName))
        {
            return mapped;
        }

        var directory = mapped.DirectoryName
            ?? throw new VeraPdfException($"Cannot determine output directory: {mapped}");
        var versionStart = GetVersionStart(mapped.Name);
        var versionEnd = GetVersionEnd(mapped.Name);
        var maxVersion = -1;

        foreach (var existingPath in Directory.EnumerateFileSystemEntries(directory))
        {
            var fileName = Path.GetFileName(existingPath);
            if (!fileName.StartsWith(versionStart, StringComparison.Ordinal)
                || !fileName.EndsWith(versionEnd, StringComparison.Ordinal))
            {
                continue;
            }

            var versionLength = fileName.Length - versionStart.Length - versionEnd.Length;
            if (versionLength < 0)
            {
                continue;
            }

            var versionText = fileName.Substring(versionStart.Length, versionLength);
            if (int.TryParse(versionText, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var version))
            {
                maxVersion = Math.Max(version, maxVersion);
            }
        }

        var nextVersion = (maxVersion + 1).ToString(CultureInfo.InvariantCulture);
        var versionedName = $"{versionStart}{nextVersion}{versionEnd}";
        return new FileInfo(Path.Combine(directory, versionedName));
    }

    private static string GetVersionStart(string fileName)
    {
        var extensionIndex = fileName.LastIndexOf('.');
        var stem = extensionIndex < 1 ? fileName : fileName[..extensionIndex];
        return stem + VersionStart;
    }

    private static string GetVersionEnd(string fileName)
    {
        var extensionIndex = fileName.LastIndexOf('.');
        var extension = extensionIndex < 1 ? string.Empty : fileName[extensionIndex..];
        return VersionEnd + extension;
    }
}
