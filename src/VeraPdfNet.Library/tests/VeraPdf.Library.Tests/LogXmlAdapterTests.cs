using System.Xml.Linq;
using VeraPdf.Library.Components;
using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class LogXmlAdapterTests
{
    [Fact]
    public void LogAdapterRoundTripsJavaXmlAttributesAndText()
    {
        var log = Log.FromValues(3, "WARNING", "First line\nSecond line");

        var xml = XmlSerialiser.ToXml(LogXmlAdapter.FromLog(log), format: false, fragment: true);
        var element = XElement.Parse(xml);
        var restored = XmlSerialiser.TypeFromXml<LogXmlAdapter>(xml).ToLog();

        Assert.Equal("logMessage", element.Name.LocalName);
        Assert.Equal("3", (string?)element.Attribute("occurrences"));
        Assert.Equal("WARNING", (string?)element.Attribute("level"));
        Assert.Equal("First line\nSecond line", element.Value);
        Assert.Equal(log.Occurrences, restored.Occurrences);
        Assert.Equal(log.Level, restored.Level);
        Assert.Equal(log.Message, restored.Message);
    }

    [Fact]
    public void SummaryAdapterRoundTripsCollectionAndLogCount()
    {
        var summary = LogsSummary.Create(4,
        [
            Log.FromValues(3, "WARNING", "Repeated"),
            Log.FromValues(1, "ERROR", "Failed")
        ]);

        var xml = XmlSerialiser.ToXml(
            LogsSummaryXmlAdapter.FromSummary(summary),
            format: false,
            fragment: true);
        var element = XElement.Parse(xml);
        var restored = XmlSerialiser.TypeFromXml<LogsSummaryXmlAdapter>(xml).ToSummary();

        Assert.Equal("logs", element.Name.LocalName);
        Assert.Equal("4", (string?)element.Attribute("logsCount"));
        Assert.Equal(2, element.Elements("logMessage").Count());
        Assert.Equal(summary.LogsCount, restored.LogsCount);
        Assert.Collection(
            restored.Logs.OrderBy(log => log.Level, StringComparer.Ordinal),
            log =>
            {
                Assert.Equal(1, log.Occurrences);
                Assert.Equal("ERROR", log.Level);
                Assert.Equal("Failed", log.Message);
            },
            log =>
            {
                Assert.Equal(3, log.Occurrences);
                Assert.Equal("WARNING", log.Level);
                Assert.Equal("Repeated", log.Message);
            });
    }

    [Fact]
    public void SummaryAdapterRejectsInvalidLogValuesWhenConverted()
    {
        var invalid = new LogsSummaryXmlAdapter
        {
            LogsCount = 1,
            Logs = [new LogXmlAdapter { Occurrences = 0, Level = "WARNING", Message = "invalid" }]
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => invalid.ToSummary());
    }
}
