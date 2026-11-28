using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class ConsoleLogsFilterTests
{
    [Theory]
    [InlineData("org.verapdf.Validator", 300, true)]
    [InlineData("third.party.Logger", 501, true)]
    [InlineData("third.party.Logger", 500, false)]
    [InlineData("third.party.Logger", 400, false)]
    [InlineData("ORG.VERAPDF.Validator", 500, false)]
    public void AppliesVeraPdfPrefixOrLevelAboveFine(string sourceClassName, int levelValue, bool expected)
    {
        Assert.Equal(expected, new ConsoleLogsFilter().IsLoggable(sourceClassName, levelValue));
    }

    [Fact]
    public void RejectsNullSourceClassNameEvenForHighSeverity()
    {
        Assert.Throws<ArgumentNullException>(() => new ConsoleLogsFilter().IsLoggable(null!, 1000));
    }
}
