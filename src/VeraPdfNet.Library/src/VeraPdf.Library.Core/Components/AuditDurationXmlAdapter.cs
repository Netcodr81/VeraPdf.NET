using System.Xml.Serialization;

namespace VeraPdf.Library.Components;

[XmlRoot("duration")]
public sealed class AuditDurationXmlAdapter
{
    [XmlAttribute("start")]
    public long Start { get; set; }

    [XmlAttribute("finish")]
    public long Finish { get; set; }

    [XmlText]
    public string Duration { get; set; } = string.Empty;

    public static AuditDurationXmlAdapter FromAuditDuration(AuditDuration duration)
    {
        ArgumentNullException.ThrowIfNull(duration);
        return new AuditDurationXmlAdapter
        {
            Start = duration.Start,
            Finish = duration.Finish,
            Duration = duration.Duration
        };
    }

    public AuditDuration ToAuditDuration() => AuditDuration.FromValues(Start, Finish);
}
