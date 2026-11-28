using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class LogsFilterTests
{
    [Theory]
    [InlineData("org.verapdf", true)]
    [InlineData("org.verapdf.validation.Validator", true)]
    [InlineData("org.verapdfx.other", true)]
    [InlineData("com.org.verapdf.Logger", false)]
    [InlineData("ORG.VERAPDF.Logger", false)]
    public void FiltersByExactVeraPdfSourcePrefix(string sourceClassName, bool expected)
    {
        Assert.Equal(expected, new LogsFilter().IsLoggable(sourceClassName));
    }

    [Fact]
    public void RejectsNullSourceClassName()
    {
        Assert.Throws<ArgumentNullException>(() => new LogsFilter().IsLoggable(null!));
    }
}
