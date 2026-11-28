namespace VeraPdf.Library.Core.Utils;

public sealed class LogsFilter
{
    private const string VeraPdfSourcePrefix = "org.verapdf";

    public bool IsLoggable(string sourceClassName)
    {
        ArgumentNullException.ThrowIfNull(sourceClassName);
        return sourceClassName.StartsWith(VeraPdfSourcePrefix, StringComparison.Ordinal);
    }
}
