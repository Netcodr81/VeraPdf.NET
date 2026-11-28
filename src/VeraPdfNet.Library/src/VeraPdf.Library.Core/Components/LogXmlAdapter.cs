using System.Xml.Serialization;

namespace VeraPdf.Library.Components;

[XmlRoot("logMessage")]
public sealed class LogXmlAdapter
{
    [XmlAttribute("occurrences")]
    public int Occurrences { get; set; }

    [XmlAttribute("level")]
    public string Level { get; set; } = string.Empty;

    [XmlText]
    public string Message { get; set; } = string.Empty;

    public static LogXmlAdapter FromLog(Log log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return new LogXmlAdapter
        {
            Occurrences = log.Occurrences,
            Level = log.Level,
            Message = log.Message
        };
    }

    public Log ToLog() => Log.FromValues(Occurrences, Level, Message);
}
