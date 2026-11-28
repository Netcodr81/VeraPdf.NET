using System.Xml.Linq;
using VeraPdf.Library.Components;
using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class ComponentDetailsXmlAdapterTests
{
    [Fact]
    public void AdapterRoundTripsComponentAttributesAndElements()
    {
        var details = ComponentDetails.FromValues(
            new Uri("urn:verapdf:component:parser"),
            "parser",
            "1.2.3",
            "veraPDF & contributors",
            "PDF parser");

        var xml = XmlSerialiser.ToXml(
            ComponentDetailsXmlAdapter.FromComponentDetails(details),
            format: false,
            fragment: true);
        var element = XElement.Parse(xml);
        var restored = XmlSerialiser.TypeFromXml<ComponentDetailsXmlAdapter>(xml).ToComponentDetails();

        Assert.Equal("componentDetails", element.Name.LocalName);
        Assert.Equal("urn:verapdf:component:parser", (string?)element.Attribute("id"));
        Assert.Equal("parser", (string?)element.Attribute("name"));
        Assert.Equal("1.2.3", (string?)element.Attribute("version"));
        Assert.Equal("veraPDF & contributors", (string?)element.Element("provider"));
        Assert.Equal("PDF parser", (string?)element.Element("description"));
        Assert.Equal(details, restored);
    }

    [Fact]
    public void AdapterRejectsInvalidComponentId()
    {
        var adapter = new ComponentDetailsXmlAdapter
        {
            Id = "http://[invalid",
            Name = "parser",
            Version = "1.2.3",
            Provider = "veraPDF",
            Description = "PDF parser"
        };

        Assert.Throws<FormatException>(() => adapter.ToComponentDetails());
    }
}
