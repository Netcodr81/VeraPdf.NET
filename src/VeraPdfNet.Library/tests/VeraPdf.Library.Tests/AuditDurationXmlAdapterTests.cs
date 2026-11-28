using System.Xml.Linq;
using VeraPdf.Library.Components;
using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class AuditDurationXmlAdapterTests
{
    [Fact]
    public void AdapterWritesStartAndFinishAttributesAndFormattedDurationText()
    {
        var duration = AuditDuration.FromValues(12_345, 67_890);

        var xml = XmlSerialiser.ToXml(
            AuditDurationXmlAdapter.FromAuditDuration(duration),
            format: false,
            fragment: true);
        var element = XElement.Parse(xml);

        Assert.Equal("duration", element.Name.LocalName);
        Assert.Equal("12345", (string?)element.Attribute("start"));
        Assert.Equal("67890", (string?)element.Attribute("finish"));
        Assert.Equal("00:00:55.545", element.Value);
    }

    [Fact]
    public void AdapterRestoresDurationFromTimestampAttributes()
    {
        const string xml = "<duration start=\"12345\" finish=\"67890\">ignored text</duration>";

        var restored = XmlSerialiser.TypeFromXml<AuditDurationXmlAdapter>(xml).ToAuditDuration();

        Assert.Equal(AuditDuration.FromValues(12_345, 67_890), restored);
    }

    [Fact]
    public void AdapterRejectsInvalidTimestampRanges()
    {
        var adapter = new AuditDurationXmlAdapter { Start = 10, Finish = 5, Duration = "00:00:00.000" };

        Assert.Throws<ArgumentException>(() => adapter.ToAuditDuration());
    }
}
