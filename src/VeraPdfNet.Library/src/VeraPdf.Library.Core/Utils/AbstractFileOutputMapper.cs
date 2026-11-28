namespace VeraPdf.Library.Core.Utils;

public abstract class AbstractFileOutputMapper : IFileOutputMapper
{
    protected const string DefaultPrefix = "veraPDF_";
    protected const string DefaultSuffix = "";

    protected AbstractFileOutputMapper() : this(DefaultPrefix)
    {
    }

    protected AbstractFileOutputMapper(string prefix) : this(prefix, DefaultSuffix)
    {
    }

    protected AbstractFileOutputMapper(string prefix, string suffix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(suffix);
        Prefix = prefix;
        Suffix = suffix;
    }

    public string Prefix { get; }
    public string Suffix { get; }

    public virtual FileInfo MapFile(FileInfo original) => DoMapFile(original);

    protected abstract FileInfo DoMapFile(FileInfo original);

    protected static string AddPrefixAndSuffix(FileInfo original, IFileOutputMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(mapper);
        var fileName = original.Name;
        var newName = mapper.Prefix + fileName;
        var fileExtensionIndex = fileName.LastIndexOf('.');
        var hasExtension = fileExtensionIndex > 0;
        var suffixIsExtension = mapper.Suffix.StartsWith(".", StringComparison.Ordinal);
        if (mapper.Suffix.Length > 0 && !suffixIsExtension && hasExtension)
        {
            newName = newName[..newName.LastIndexOf('.')] + mapper.Suffix + fileName[fileExtensionIndex..];
        }
        else
        {
            newName += mapper.Suffix;
        }

        return newName;
    }

    public override string ToString() =>
        $"AbstractFileOutputMapper [prefix={Prefix}, suffix={Suffix}]";
}
