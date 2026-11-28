using VeraPdf.Library.Components;

namespace VeraPdf.Library.Tests;

public class LogTests
{
    [Fact]
    public void CreatesLogWithValidatedValues()
    {
        var log = Log.FromValues(3, "WARNING", "Problem");

        Assert.Equal(3, log.Occurrences);
        Assert.Equal("WARNING", log.Level);
        Assert.Equal("Problem", log.Message);
        Assert.Throws<ArgumentOutOfRangeException>(() => Log.FromValues(0, "WARNING", "Problem"));
        Assert.Throws<ArgumentNullException>(() => Log.FromValues(1, null!, "Problem"));
        Assert.Throws<ArgumentNullException>(() => Log.FromValues(1, "WARNING", null!));
    }

    [Fact]
    public void SummaryKeepsLogCountAndAnImmutableSetSnapshot()
    {
        var log = Log.FromValues(2, "WARNING", "Problem");
        var entries = new HashSet<Log> { log };
        var summary = LogsSummary.Create(5, entries);
        entries.Add(Log.FromValues(1, "ERROR", "Other"));

        Assert.Equal(5, summary.LogsCount);
        Assert.Single(summary.Logs);
        Assert.Contains(log, summary.Logs);
        Assert.Throws<NotSupportedException>(() => ((ISet<Log>)summary.Logs).Add(
            Log.FromValues(1, "ERROR", "Mutation")));
        Assert.Throws<ArgumentOutOfRangeException>(() => LogsSummary.Create(-1, []));
    }
}
