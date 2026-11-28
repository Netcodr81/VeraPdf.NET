using System.Text;
using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Tests;

public class ProfileTagTests
{
    private static ValidationProfile CreateProfile()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile" flavour="CUSTOM">
              <rules>
                <rule object="A"><id specification="S" clause="1" testNumber="1"/></rule>
                <rule object="A" tags="red,blue,"><id specification="S" clause="1" testNumber="2"/></rule>
                <rule object="B" tags="blue,green"><id specification="S" clause="1" testNumber="3"/></rule>
                <rule object="B" tags="Red"><id specification="S" clause="1" testNumber="4"/></rule>
              </rules>
              <variables><variable object="A" name="v"><value>1</value></variable></variables>
            </profile>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return ProfileReader.Read(stream);
    }

    [Fact]
    public void FindsIdsAndEnumeratesDistinctTags()
    {
        var profile = CreateProfile();
        Assert.Equal(new[] { "Red", "blue", "green", "red" }, profile.GetTags());
        Assert.Equal("B", profile.GetRuleById(new RuleId("S", "1", 3))!.Object);
        Assert.Null(profile.GetRuleById(new RuleId("S", "1", 5)));
        Assert.Throws<ArgumentNullException>(() => profile.GetRuleById(null!));
    }

    [Theory]
    [InlineData("red", "", new[] { 1, 2 })]
    [InlineData("*", "blue", new[] { 1, 4 })]
    [InlineData("blue", "red", new[] { 1, 3 })]
    [InlineData("", "", new[] { 1 })]
    [InlineData("*", "*", new int[0])]
    [InlineData("RED", "", new[] { 1 })]
    public void FiltersRulesLikeUpstream(string included, string excluded, int[] expected)
    {
        var profile = CreateProfile();
        var filtered = profile.WithTags(
            included.Length == 0 ? [] : [included],
            excluded.Length == 0 ? [] : [excluded]);
        Assert.Equal(expected, filtered.Rules.Select(rule => rule.Id.TestNumber));
        Assert.Single(filtered.Variables);
        Assert.Equal(profile.Flavour, filtered.Flavour);
        Assert.Equal(4, profile.Rules.Count);
    }

    [Fact]
    public void NullArgumentsAreRejected()
    {
        var profile = CreateProfile();
        Assert.Throws<ArgumentNullException>(() => profile.WithTags(null!, []));
        Assert.Throws<ArgumentNullException>(() => profile.WithTags([], null!));
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData(",", 0)]
    [InlineData("a,,", 1)]
    [InlineData(",a", 2)]
    public void FollowsJavaSplitForEmptyTags(string tags, int expectedCount)
    {
        var rule = new Rule(new RuleId("S", "1", 1), "A", null, tags, null, null, null, []);
        Assert.Equal(expectedCount, rule.GetTags().Count);
    }
}
