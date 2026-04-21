using Shouldly;
using VeraPdf.NET.Validation.Profiles.Converters;

namespace VeraPdf.NET.Validation.Tests.Parity;

public class ProfileConversionFidelityTests
{
    [Fact]
    public void Convert_Should_Parse_Namespaced_Profile_Name_Rule_Id_And_Test()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <profile xmlns="http://www.verapdf.org/ValidationProfile" flavour="PDFA_1_B">
              <details>
                <name>ISO 19005-1:2005 - 6.1.2 File header - PDF header</name>
              </details>
              <rules>
                <rule object="CosDocument">
                  <id specification="ISO_19005_1" clause="6.1.2" testNumber="1"/>
                  <test>headerOffset == 0 &amp;&amp; /%PDF-\d\.\d/.test(header)</test>
                </rule>
              </rules>
            </profile>
            """;

        var profile = VeraPdfXmlProfileConverter.Convert(xml);

        profile.Name.ShouldBe("ISO 19005-1:2005 - 6.1.2 File header - PDF header");
        profile.Rules.Count.ShouldBe(1);

        var rule = profile.Rules[0];
        rule.Id.ShouldBe("ISO_19005_1:6.1.2:1");
        rule.TargetTypeName.ShouldBe("CosDocument");
        rule.Test.ShouldBe("headerOffset == 0 && /%PDF-\\d\\.\\d/.test(header)");
    }

    [Fact]
    public void Convert_Should_Use_Flavour_As_Profile_Name_When_Details_Name_Is_Missing()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile" flavour="PDFA_2_U">
              <rules>
                <rule object="PDPage">
                  <id specification="ISO_19005_2" clause="6.1.1" testNumber="2"/>
                  <test>pageNumber &gt;= 1</test>
                </rule>
              </rules>
            </profile>
            """;

        var profile = VeraPdfXmlProfileConverter.Convert(xml);

        profile.Name.ShouldBe("PDFA_2_U");
        profile.Rules.Count.ShouldBe(1);
        profile.Rules[0].Id.ShouldBe("ISO_19005_2:6.1.1:2");
    }

    [Fact]
    public void Convert_Should_Fallback_To_Rule_Attribute_Id_When_Id_Element_Is_Absent()
    {
        const string xml = """
            <profile xmlns="http://www.verapdf.org/ValidationProfile" flavour="PDFA_1_B">
              <rules>
                <rule id="custom-rule-id" object="CosDocument">
                  <test>headerOffset == 0</test>
                </rule>
              </rules>
            </profile>
            """;

        var profile = VeraPdfXmlProfileConverter.Convert(xml);

        profile.Rules.Count.ShouldBe(1);
        profile.Rules[0].Id.ShouldBe("custom-rule-id");
        profile.Rules[0].TargetTypeName.ShouldBe("CosDocument");
        profile.Rules[0].Test.ShouldBe("headerOffset == 0");
    }
}
