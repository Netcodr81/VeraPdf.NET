using VeraPdf.Library.Core;

namespace VeraPdf.Library.Core.Utils;

public sealed class RelativeRootMapper : AbstractFileOutputMapper
{
    private readonly DirectoryInfo _sourceRoot;
    private readonly DirectoryInfo _destinationRoot;

    private RelativeRootMapper(DirectoryInfo sourceRoot, DirectoryInfo destinationRoot, string prefix, string suffix)
        : base(prefix, suffix)
    {
        ArgumentNullException.ThrowIfNull(sourceRoot);
        ArgumentNullException.ThrowIfNull(destinationRoot);
        _sourceRoot = sourceRoot;
        _destinationRoot = destinationRoot;
    }

    public static RelativeRootMapper FromValues(
        DirectoryInfo sourceRoot,
        DirectoryInfo destinationRoot,
        string prefix,
        string suffix) =>
        new(sourceRoot, destinationRoot, prefix, suffix);

    protected override FileInfo DoMapFile(FileInfo original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var sourceDirectory = Path.GetDirectoryName(original.FullName)
            ?? throw new VeraPdfException($"Cannot determine source directory: {original}");
        var relativeSourceRoot = Path.GetRelativePath(sourceDirectory, _sourceRoot.FullName);
        var destinationDirectory = Path.GetFullPath(Path.Combine(_destinationRoot.FullName, relativeSourceRoot));

        try
        {
            Directory.CreateDirectory(destinationDirectory);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new VeraPdfException($"Cannot create directory: {destinationDirectory}", exception);
        }

        return new FileInfo(Path.Combine(destinationDirectory, AddPrefixAndSuffix(original, this)));
    }
}
