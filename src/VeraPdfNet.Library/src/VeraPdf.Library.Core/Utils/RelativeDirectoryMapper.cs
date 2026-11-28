using VeraPdf.Library.Core;

namespace VeraPdf.Library.Core.Utils;

public sealed class RelativeDirectoryMapper : AbstractFileOutputMapper
{
    public const string DefaultRelativePath = ".";

    public static RelativeDirectoryMapper DefaultInstance { get; } = new();

    private readonly string _relativePath;

    private RelativeDirectoryMapper() : base()
    {
        _relativePath = DefaultRelativePath;
    }

    private RelativeDirectoryMapper(string relativePath, string prefix, string suffix) : base(prefix, suffix)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        _relativePath = relativePath;
    }

    public static RelativeDirectoryMapper WithPrefixAndSuffix(string prefix, string suffix) =>
        new(DefaultRelativePath, prefix, suffix);

    public static RelativeDirectoryMapper FromValues(string relativePath, string prefix, string suffix) =>
        new(relativePath, prefix, suffix);

    protected override FileInfo DoMapFile(FileInfo original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var newName = AddPrefixAndSuffix(original, this);
        var parentPath = Path.GetDirectoryName(original.FullName)
            ?? throw new VeraPdfException($"Cannot determine source directory: {original}");
        var destinationPath = _relativePath == DefaultRelativePath
            ? parentPath
            : Path.GetFullPath(Path.Combine(parentPath, _relativePath));

        if (_relativePath != DefaultRelativePath)
        {
            try
            {
                Directory.CreateDirectory(destinationPath);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                throw new VeraPdfException($"Cannot create directory: {destinationPath}", exception);
            }
        }

        return new FileInfo(Path.Combine(destinationPath, newName));
    }

    public override string ToString() =>
        $"RelativeDirectoryMapper [prefix={Prefix}, suffix={Suffix},relDirPath={_relativePath}]";
}
