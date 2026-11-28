using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class LogsFileHandlerTests
{
    [Fact]
    public void ReusesOneTemporaryLogFilePerProcess()
    {
        var path = LogsFileHandler.GetLogFile();

        Assert.Equal(path, LogsFileHandler.GetLogFile());
        Assert.Equal(Environment.ProcessId, LogsFileHandler.GetPid());
        Assert.StartsWith($"logs{Environment.ProcessId}-", Path.GetFileName(path));
        Assert.EndsWith(LogsFileHandler.Suffix, path);
        Assert.True(File.Exists(path));
    }

    [Fact]
    public void CreateNewLogFileCreatesOrTruncatesTheProcessLog()
    {
        var path = LogsFileHandler.GetLogFile();
        File.WriteAllText(path, "previous log contents");

        LogsFileHandler.CreateNewLogFile();

        Assert.True(File.Exists(path));
        Assert.Equal(0, new FileInfo(path).Length);
    }
}
