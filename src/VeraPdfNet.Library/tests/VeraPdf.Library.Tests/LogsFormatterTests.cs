using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class LogsFormatterTests
{
    [Fact]
    public void FormatsLogContextAndCurrentProcessId()
    {
        var formatted = new LogsFormatter().Format("org.verapdf.Validator", "validate", "WARNING", "invalid PDF");

        Assert.Equal("[log]" + Environment.NewLine +
            $"org.verapdf.Validator validate in process {Environment.ProcessId}{Environment.NewLine}" +
            "WARNING: invalid PDF" + Environment.NewLine, formatted);
    }

    [Fact]
    public void FormatsNullFieldsUsingJavaStringFormattingConvention()
    {
        var formatted = new LogsFormatter().Format(null, null, null, null);

        Assert.Equal("[log]" + Environment.NewLine +
            $"null null in process {Environment.ProcessId}{Environment.NewLine}" +
            "null: null" + Environment.NewLine, formatted);
    }
}
