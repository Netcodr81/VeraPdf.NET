using System.Xml.Serialization;

namespace VeraPdf.Library.Components;

[XmlRoot("logs")]
public sealed class LogsSummaryXmlAdapter
{
    [XmlAttribute("logsCount")]
    public int LogsCount { get; set; }

    [XmlElement("logMessage")]
    public List<LogXmlAdapter> Logs { get; set; } = [];

    public static LogsSummaryXmlAdapter FromSummary(LogsSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);
        return new LogsSummaryXmlAdapter
        {
            LogsCount = summary.LogsCount,
            Logs = summary.Logs.Select(LogXmlAdapter.FromLog).ToList()
        };
    }

    public LogsSummary ToSummary()
    {
        ArgumentNullException.ThrowIfNull(Logs);
        return LogsSummary.Create(LogsCount, Logs.Select(log =>
        {
            ArgumentNullException.ThrowIfNull(log);
            return log.ToLog();
        }));
    }
}
