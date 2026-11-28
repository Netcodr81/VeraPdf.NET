namespace VeraPdf.Library.Core.Utils;

public sealed class SingleOutputDirMapper : AbstractFileOutputMapper
{
    private readonly DirectoryInfo _outputDirectory;

    private SingleOutputDirMapper(DirectoryInfo outputDirectory, string prefix, string suffix)
        : base(prefix, suffix)
    {
        ArgumentNullException.ThrowIfNull(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    public static SingleOutputDirMapper FromValues(DirectoryInfo outputDirectory, string prefix, string suffix) =>
        new(outputDirectory, prefix, suffix);

    protected override FileInfo DoMapFile(FileInfo original)
    {
        ArgumentNullException.ThrowIfNull(original);
        var newName = AddPrefixAndSuffix(original, this);
        return new FileInfo(Path.Combine(_outputDirectory.FullName, newName));
    }
}
