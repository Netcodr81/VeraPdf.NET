using System.Text;
using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Tests;

public class ProfileWriterTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AllBuiltInProfilesRoundTrip(bool indent)
    {
        foreach (var name in BuiltInProfiles.Names)
        {
            var original = BuiltInProfiles.Read(name);
            using var stream = new MemoryStream();
            ProfileWriter.Write(original, stream, indent);
            Assert.True(stream.CanWrite);
            stream.Position = 0;
            var copy = ProfileReader.Read(stream);
            Assert.Equal(original.Flavour, copy.Flavour);
            Assert.Equal(original.Details, copy.Details);
            Assert.Equal(original.Hash, copy.Hash);
            Assert.Equal(original.Variables, copy.Variables);
            Assert.Equal(original.Rules.Count, copy.Rules.Count);
            for (var i = 0; i < original.Rules.Count; i++)
            {
                var expected = original.Rules[i];
                var actual = copy.Rules[i];
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Object, actual.Object);
                Assert.Equal(expected.Deferred, actual.Deferred);
                Assert.Equal(expected.Tags, actual.Tags);
                Assert.Equal(expected.Description, actual.Description);
                Assert.Equal(expected.Test, actual.Test);
                Assert.Equal(expected.Error?.Message, actual.Error?.Message);
                Assert.Equal(expected.Error?.Arguments, actual.Error?.Arguments);
                Assert.Equal(expected.References, actual.References);
            }
        }
    }

    [Fact]
    public void PreservesCustomTextAndOptionalFields()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile">
              <rules>
                <rule object="O" deferred="false" tags="">
                  <id specification="S" clause="C" testNumber="1"/>
                  <test>first&#xD;second &amp;&amp; x &lt; 4</test>
                  <error><message></message><arguments><argument name="n">x</argument></arguments></error>
                </rule>
              </rules>
            </profile>
            """;
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var original = ProfileReader.Read(source);
        using var destination = new MemoryStream();
        ProfileWriter.Write(original, destination);
        destination.Position = 0;
        var copy = ProfileReader.Read(destination);
        Assert.Null(copy.Flavour);
        Assert.Null(copy.Details);
        Assert.Null(copy.Hash);
        var rule = Assert.Single(copy.Rules);
        Assert.False(rule.Deferred);
        Assert.Equal("", rule.Tags);
        Assert.Equal("first\rsecond && x < 4", rule.Test);
        Assert.Equal("", rule.Error!.Message);
        Assert.Equal(new ErrorArgument("x", "n"), Assert.Single(rule.Error.Arguments));
    }
}
