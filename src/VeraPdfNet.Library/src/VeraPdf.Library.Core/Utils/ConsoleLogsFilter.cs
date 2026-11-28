namespace VeraPdf.Library.Core.Utils;

public sealed class ConsoleLogsFilter
{
    public const int FineLevelValue = 500;

    private readonly LogsFilter _sourceFilter = new();

    public bool IsLoggable(string sourceClassName, int levelValue) =>
        _sourceFilter.IsLoggable(sourceClassName) || levelValue > FineLevelValue;
}
