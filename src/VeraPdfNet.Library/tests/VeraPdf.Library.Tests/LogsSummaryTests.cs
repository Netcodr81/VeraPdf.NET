using VeraPdf.Library.Components;

namespace VeraPdf.Library.Tests;

public class LogsSummaryTests
{
    [Fact]
    public void ReadsAndGroupsLogEntriesPreservingMultilineMessages()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        var contents = string.Join(Environment.NewLine,
            "prefix",
            "[log]",
            "validator check in process 123",
            "WARNING: repeated",
            "continued",
            "[log]",
            "validator check in process 123",
            "WARNING: repeated",
            "continued",
            "[log]",
            "validator check in process 123",
            "ERROR: failed: with details",
            "");
        File.WriteAllText(path, contents);

        try
        {
            var summary = LogsSummary.GetSummary(path);

            Assert.Equal(3, summary.LogsCount);
            Assert.Equal(2, summary.Logs.Count);
            Assert.Contains(summary.Logs, log =>
                log.Occurrences == 2 && log.Level == "WARNING" && log.Message == "repeated\ncontinued");
            Assert.Contains(summary.Logs, log =>
                log.Occurrences == 1 && log.Level == "ERROR" && log.Message == "failed: with details");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void EmptyLogFileProducesEmptySummary()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        File.WriteAllText(path, "");

        try
        {
            var summary = LogsSummary.GetSummary(path);

            Assert.Equal(0, summary.LogsCount);
            Assert.Empty(summary.Logs);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void MalformedLoggedMessageReportsFormatError()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.log");
        File.WriteAllText(path, "[log]\nsource\nmissing separator");

        try
        {
            Assert.Throws<FormatException>(() => LogsSummary.GetSummary(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
