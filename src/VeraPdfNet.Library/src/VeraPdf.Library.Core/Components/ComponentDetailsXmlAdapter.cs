using System.Xml.Serialization;

namespace VeraPdf.Library.Components;

[XmlRoot("componentDetails")]
public sealed class ComponentDetailsXmlAdapter
{
    [XmlAttribute("id")]
    public string Id { get; set; } = string.Empty;

    [XmlAttribute("name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute("version")]
    public string Version { get; set; } = string.Empty;

    [XmlElement("provider")]
    public string Provider { get; set; } = string.Empty;

    [XmlElement("description")]
    public string Description { get; set; } = string.Empty;

    public static ComponentDetailsXmlAdapter FromComponentDetails(ComponentDetails details)
    {
        ArgumentNullException.ThrowIfNull(details);
        return new ComponentDetailsXmlAdapter
        {
            Id = details.Id?.OriginalString ?? string.Empty,
            Name = details.Name ?? string.Empty,
            Version = details.Version ?? string.Empty,
            Provider = details.Provider ?? string.Empty,
            Description = details.Description ?? string.Empty
        };
    }

    public ComponentDetails ToComponentDetails()
    {
        if (!Uri.TryCreate(Id, UriKind.RelativeOrAbsolute, out var id))
        {
            throw new FormatException($"Invalid component id URI: {Id}");
        }

        return ComponentDetails.FromValues(id, Name, Version, Provider, Description);
    }
}
