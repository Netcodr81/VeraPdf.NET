namespace VeraPdf.Library.Core.Utils;

public static class FileOutputMappers
{
    public static IFileOutputMapper SibFiles() => RelativeDirectoryMapper.DefaultInstance;

    public static IFileOutputMapper VerSibFiles() => VersioningMapper.DefaultInstance;

    public static IFileOutputMapper SibFiles(string prefix) =>
        SibFiles(prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper VerSibFiles(string prefix) =>
        VerSibFiles(prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper SibFiles(string prefix, string suffix) =>
        RelativeDirectoryMapper.WithPrefixAndSuffix(prefix, suffix);

    public static IFileOutputMapper VerSibFiles(string prefix, string suffix) =>
        VersioningMapper.FromMapper(SibFiles(prefix, suffix));

    public static IFileOutputMapper SubFold(string relativePath) =>
        SubFold(relativePath, RelativeDirectoryMapper.DefaultInstance.Prefix);

    public static IFileOutputMapper VerSubFold(string relativePath) =>
        VerSubFold(relativePath, RelativeDirectoryMapper.DefaultInstance.Prefix);

    public static IFileOutputMapper SubFold(string relativePath, string prefix) =>
        SubFold(relativePath, prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper VerSubFold(string relativePath, string prefix) =>
        VerSubFold(relativePath, prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper SubFold(string relativePath, string prefix, string suffix)
    {
        ValidateRelativePath(relativePath);
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);
        return RelativeDirectoryMapper.FromValues(relativePath, prefix, suffix);
    }

    public static IFileOutputMapper VerSubFold(string relativePath, string prefix, string suffix) =>
        VersioningMapper.FromMapper(SubFold(relativePath, prefix, suffix));

    public static IFileOutputMapper ToRelativeDest(DirectoryInfo source, DirectoryInfo destination) =>
        ToRelativeDest(source, destination, RelativeDirectoryMapper.DefaultInstance.Prefix);

    public static IFileOutputMapper ToRelativeDest(DirectoryInfo source, DirectoryInfo destination, string prefix) =>
        ToRelativeDest(source, destination, prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper ToRelativeDest(
        DirectoryInfo source,
        DirectoryInfo destination,
        string prefix,
        string suffix)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);
        return RelativeRootMapper.FromValues(source, destination, prefix, suffix);
    }

    public static IFileOutputMapper Fold(string path) =>
        Fold(path, RelativeDirectoryMapper.DefaultInstance.Prefix);

    public static IFileOutputMapper VerFold(string path) =>
        VerFold(path, RelativeDirectoryMapper.DefaultInstance.Prefix);

    public static IFileOutputMapper Fold(string path, string prefix) =>
        Fold(path, prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper VerFold(string path, string prefix) =>
        VerFold(path, prefix, RelativeDirectoryMapper.DefaultInstance.Suffix);

    public static IFileOutputMapper Fold(string path, string prefix, string suffix)
    {
        ValidateOutputArguments(path, prefix, suffix);
        var outputDirectory = CreateWritableOutputDirectory(path);
        return SingleOutputDirMapper.FromValues(outputDirectory, prefix, suffix);
    }

    public static IFileOutputMapper VerFold(string path, string prefix, string suffix)
    {
        ValidateOutputArguments(path, prefix, suffix);
        var outputDirectory = CreateWritableOutputDirectory(path);
        return VersioningMapper.FromMapper(SingleOutputDirMapper.FromValues(outputDirectory, prefix, suffix));
    }

    private static void ValidateOutputArguments(string path, string prefix, string suffix)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Length == 0)
        {
            throw new ArgumentException("Argument path cannot be an empty string.", nameof(path));
        }

        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);
    }

    private static void ValidateRelativePath(string relativePath)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        if (relativePath.Length == 0)
        {
            throw new ArgumentException("Argument relativePath cannot be an empty string.", nameof(relativePath));
        }
    }

    private static DirectoryInfo CreateWritableOutputDirectory(string path)
    {
        try
        {
            var outputDirectory = Directory.CreateDirectory(path);
            var probePath = Path.Combine(outputDirectory.FullName, $".veraPDF-write-test-{Guid.NewGuid():N}");
            using (var probe = new FileStream(
                       probePath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None,
                       bufferSize: 1,
                       FileOptions.DeleteOnClose))
            {
                probe.WriteByte(0);
            }

            return outputDirectory;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new ArgumentException($"Path is not a writable output directory: {path}", nameof(path), exception);
        }
    }
}
