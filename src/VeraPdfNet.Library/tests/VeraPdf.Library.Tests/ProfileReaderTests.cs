using System.Text;
using System.Xml;
using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Tests;

public class ProfileReaderTests
{
    [Theory]
    [InlineData("PDFA-1A", "PDFA_1_A", 135, 1)]
    [InlineData("PDFA-1B", "PDFA_1_B", 129, 1)]
    [InlineData("PDFA-2A", "PDFA_2_A", 153, 1)]
    [InlineData("PDFA-2B", "PDFA_2_B", 144, 1)]
    [InlineData("PDFA-2U", "PDFA_2_U", 146, 1)]
    [InlineData("PDFA-3A", "PDFA_3_A", 155, 1)]
    [InlineData("PDFA-3B", "PDFA_3_B", 146, 1)]
    [InlineData("PDFA-3U", "PDFA_3_U", 148, 1)]
    [InlineData("PDFA-4", "PDFA_4", 111, 5)]
    [InlineData("PDFA-4E", "PDFA_4_E", 111, 5)]
    [InlineData("PDFA-4F", "PDFA_4_F", 111, 5)]
    [InlineData("PDFUA-1", "PDFUA_1", 106, 3)]
    [InlineData("PDFUA-2-ISO32005", "PDFUA_2", 1728, 1)]
    [InlineData("WTPDF-1-0-Accessibility", "WTPDF_1_0_ACCESSIBILITY", 1724, 1)]
    [InlineData("WTPDF-1-0-Reuse", "WTPDF_1_0_REUSE", 1711, 0)]
    public void ReadsAllReferenceProfiles(string name, string flavour, int rules, int variables)
    {
        var profile = BuiltInProfiles.Read(name);
        Assert.Equal(flavour, profile.Flavour);
        Assert.Equal(rules, profile.Rules.Count);
        Assert.Equal(variables, profile.Variables.Count);
        Assert.All(profile.Rules, rule => Assert.False(string.IsNullOrEmpty(rule.Test)));
    }

    [Fact]
    public void ReadsRuleDetailsWithoutChangingExpression()
    {
        var profile = BuiltInProfiles.Read("PDFA-1B");
        var first = profile.Rules[0];
        Assert.Equal(new RuleId("ISO_19005_1", "6.1.2", 1), first.Id);
        Assert.Equal(@"headerOffset == 0 && /%PDF-\d\.\d/.test(header)", first.Test);
        Assert.Equal("header", first.Error!.Arguments[0].Expression);
        Assert.Equal("PDF 1.4 Reference", first.References[0].Specification);
        Assert.NotEmpty(profile.GetRulesByObject("CosDocument"));
        Assert.Empty(profile.GetRulesByObject("cosdocument"));
    }

    [Fact]
    public void RejectsUnknownProfileAndDoesNotHideMalformedXml()
    {
        Assert.Equal(15, BuiltInProfiles.Names.Count);
        Assert.Throws<ArgumentException>(() => BuiltInProfiles.Read("pdfa-1b"));
        using var malformed = new MemoryStream(Encoding.UTF8.GetBytes("<profile>"));
        Assert.Throws<XmlException>(() => ProfileReader.Read(malformed));
        Assert.True(malformed.CanRead);
    }

    [Fact]
    public void RejectsDtdAndWrongRoot()
    {
        using var dtd = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE profile [<!ENTITY x 'test'>]><profile>&x;</profile>"));
        Assert.Throws<XmlException>(() => ProfileReader.Read(dtd));
        using var wrong = new MemoryStream(Encoding.UTF8.GetBytes("<profile/>"));
        Assert.Throws<InvalidDataException>(() => ProfileReader.Read(wrong));
    }

    [Fact]
    public void ReadsCustomFieldsAndLeavesStreamOpen()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile" flavour="CUSTOM">
              <details creator="creator" created="2020-01-01T00:00:00Z"><name>name</name><description>description</description></details>
              <hash>hash</hash>
              <rules>
                <rule object="Object" deferred="true" tags="a,b">
                  <id specification="SPEC" clause="1.2" testNumber="42" />
                  <description>description</description><test>value == null</test>
                  <error><message>message</message><arguments><argument name="named">value</argument></arguments></error>
                  <references><reference specification="standard" clause="3"/></references>
                </rule>
              </rules>
              <variables><variable object="Object" name="var"><defaultValue>null</defaultValue><value>value</value></variable></variables>
            </profile>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var profile = ProfileReader.Read(stream);
        Assert.True(stream.CanRead);
        Assert.Equal(new ProfileDetails("name", "description", "creator", "2020-01-01T00:00:00Z"), profile.Details);
        Assert.Equal("hash", profile.Hash);
        var rule = Assert.Single(profile.GetRulesByObject("Object"));
        Assert.True(rule.Deferred);
        Assert.Equal("a,b", rule.Tags);
        Assert.Equal(new ErrorArgument("value", "named"), Assert.Single(rule.Error!.Arguments));
        Assert.Equal(new RuleReference("standard", "3"), Assert.Single(rule.References));
        Assert.Equal(new Variable("var", "Object", "null", "value"), Assert.Single(profile.GetVariablesByObject("Object")));
    }

    [Fact]
    public void MissingRuleIdentityIsReported()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile">
              <rules><rule object="Object"><test>true</test></rule></rules>
            </profile>
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        Assert.Throws<InvalidDataException>(() => ProfileReader.Read(stream));
    }

    [Theory]
    [InlineData("PDFA_1_B", "PDF/A-1b validation profile")]
    [InlineData("PDFUA_2", null)]
    [InlineData("WTPDF_1_0_REUSE", null)]
    public void ReadsProfilesByExactFlavour(string flavour, string? name)
    {
        var profile = BuiltInProfiles.ReadByFlavour(flavour);
        Assert.Equal(flavour, profile.Flavour);
        if (name is not null)
            Assert.Equal(name, profile.Details!.Name);
    }

    [Fact]
    public void RejectsUnknownFlavour()
    {
        Assert.Throws<KeyNotFoundException>(() => BuiltInProfiles.ReadByFlavour("pdfa_1_b"));
        Assert.Throws<ArgumentNullException>(() => BuiltInProfiles.ReadByFlavour(null!));
    }

    [Fact]
    public void EveryBundledFlavourResolvesToItsProfile()
    {
        foreach (var name in BuiltInProfiles.Names)
        {
            var profile = BuiltInProfiles.Read(name);
            Assert.NotNull(profile.Flavour);
            var selected = BuiltInProfiles.ReadByFlavour(profile.Flavour);
            Assert.Equal(profile.Details, selected.Details);
            Assert.Equal(profile.Rules.Count, selected.Rules.Count);
        }
    }
}
