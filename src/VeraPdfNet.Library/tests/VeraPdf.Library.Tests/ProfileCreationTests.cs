using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Tests;

public class ProfileCreationTests
{
    [Fact]
    public void CustomProfileCopiesCollectionsIncludingNestedRuleData()
    {
        var args = new List<ErrorArgument> { new("value", "name") };
        var refs = new List<RuleReference> { new("standard", "section") };
        var rules = new List<Rule>
        {
            new(new RuleId("SPEC", "1", 1), "Object", false, "tag", "description", "true",
                new ErrorDetails("message", args), refs)
        };
        var variables = new List<Variable> { new("v", "Object", "null", "value") };
        var profile = ValidationProfile.Create("CUSTOM", new ProfileDetails("name", null, null, null), "hash", rules, variables);
        args.Clear();
        refs.Clear();
        rules.Clear();
        variables.Clear();

        var rule = Assert.Single(profile.Rules);
        Assert.Single(rule.Error!.Arguments);
        Assert.Single(rule.References);
        Assert.Single(profile.Variables);
        using var stream = new MemoryStream();
        ProfileWriter.Write(profile, stream);
        stream.Position = 0;
        var roundTrip = ProfileReader.Read(stream);
        Assert.Equal("CUSTOM", roundTrip.Flavour);
        Assert.Equal(rule.Id, Assert.Single(roundTrip.Rules).Id);
        Assert.Single(roundTrip.Variables);
    }

    [Fact]
    public void NullTopLevelInputsAreRejected()
    {
        var details = new ProfileDetails("name", null, null, null);
        Assert.Throws<ArgumentNullException>(() => ValidationProfile.Create(null!, details, "", [], []));
        Assert.Throws<ArgumentNullException>(() => ValidationProfile.Create("CUSTOM", null!, "", [], []));
        Assert.Throws<ArgumentNullException>(() => ValidationProfile.Create("CUSTOM", details, null!, [], []));
        Assert.Throws<ArgumentNullException>(() => ValidationProfile.Create("CUSTOM", details, "", null!, []));
        Assert.Throws<ArgumentNullException>(() => ValidationProfile.Create("CUSTOM", details, "", [], null!));
    }

    [Fact]
    public void NullNestedValuesAreRejected()
    {
        var details = new ProfileDetails("name", null, null, null);
        var id = new RuleId("SPEC", "1", 1);
        var badReferences = new Rule(id, "Object", null, null, null, null, null, [null!]);
        var badArguments = new Rule(id, "Object", null, null, null, null, new ErrorDetails(null, [null!]), []);
        Assert.Throws<ArgumentException>(() => ValidationProfile.Create("CUSTOM", details, "", [badReferences], []));
        Assert.Throws<ArgumentException>(() => ValidationProfile.Create("CUSTOM", details, "", [badArguments], []));
        Assert.Throws<ArgumentException>(() => ValidationProfile.Create("CUSTOM", details, "", [], [null!]));
    }
}
