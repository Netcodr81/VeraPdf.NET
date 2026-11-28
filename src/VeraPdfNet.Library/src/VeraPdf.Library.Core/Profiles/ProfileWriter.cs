using System.Text;
using System.Xml;

namespace VeraPdf.Library.Profiles;

/// <summary>Writes the profile fields represented by <see cref="ValidationProfile"/>.</summary>
public static class ProfileWriter
{
    private const string Namespace = "http://www.verapdf.org/ValidationProfile";

    /// <summary>Writes UTF-8 XML without closing the destination stream.</summary>
    public static void Write(ValidationProfile profile, Stream stream, bool indent = true)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(stream);
        using var writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = indent,
            CloseOutput = false,
            NewLineHandling = NewLineHandling.Entitize
        });
        writer.WriteStartDocument();
        Start(writer, "profile");
        Attribute(writer, "flavour", profile.Flavour);

        if (profile.Details is { } details)
        {
            Start(writer, "details");
            Attribute(writer, "creator", details.Creator);
            Attribute(writer, "created", details.Created);
            Element(writer, "name", details.Name);
            Element(writer, "description", details.Description);
            writer.WriteEndElement();
        }

        Element(writer, "hash", profile.Hash);
        Start(writer, "rules");
        foreach (var rule in profile.Rules)
            WriteRule(writer, rule);
        writer.WriteEndElement();
        Start(writer, "variables");
        foreach (var variable in profile.Variables)
        {
            Start(writer, "variable");
            Attribute(writer, "name", variable.Name);
            Attribute(writer, "object", variable.Object);
            Element(writer, "defaultValue", variable.DefaultValue);
            Element(writer, "value", variable.Value);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static void WriteRule(XmlWriter writer, Rule rule)
    {
        Start(writer, "rule");
        Attribute(writer, "object", rule.Object);
        Attribute(writer, "deferred", rule.Deferred is { } deferred ? XmlConvert.ToString(deferred) : null);
        Attribute(writer, "tags", rule.Tags);
        Start(writer, "id");
        Attribute(writer, "specification", rule.Id.Specification);
        Attribute(writer, "clause", rule.Id.Clause);
        Attribute(writer, "testNumber", XmlConvert.ToString(rule.Id.TestNumber));
        writer.WriteEndElement();
        Element(writer, "description", rule.Description);
        Element(writer, "test", rule.Test);
        if (rule.Error is { } error)
        {
            Start(writer, "error");
            Element(writer, "message", error.Message);
            Start(writer, "arguments");
            foreach (var argument in error.Arguments)
            {
                Start(writer, "argument");
                Attribute(writer, "name", argument.Name);
                writer.WriteString(argument.Expression);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        Start(writer, "references");
        foreach (var reference in rule.References)
        {
            Start(writer, "reference");
            Attribute(writer, "specification", reference.Specification);
            Attribute(writer, "clause", reference.Clause);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }

    private static void Start(XmlWriter writer, string name) => writer.WriteStartElement(name, Namespace);

    private static void Attribute(XmlWriter writer, string name, string? value)
    {
        if (value is not null)
            writer.WriteAttributeString(name, value);
    }

    private static void Element(XmlWriter writer, string name, string? value)
    {
        if (value is not null)
            writer.WriteElementString(name, Namespace, value);
    }
}
