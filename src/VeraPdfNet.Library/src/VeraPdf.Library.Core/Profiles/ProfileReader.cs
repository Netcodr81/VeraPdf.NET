using System.Xml;
using System.Xml.Linq;

namespace VeraPdf.Library.Profiles;

/// <summary>Reads veraPDF profile XML without executing rules or validating a PDF.</summary>
public static class ProfileReader
{
    private static readonly XNamespace ProfileNamespace = "http://www.verapdf.org/ValidationProfile";

    /// <summary>Reads a profile from a stream, leaving the stream open.</summary>
    public static ValidationProfile Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            CloseInput = false
        });
        var root = XDocument.Load(reader).Root;
        if (root?.Name != ProfileNamespace + "profile")
            throw new InvalidDataException("Expected a veraPDF validation profile root element.");

        var details = root.Element(ProfileNamespace + "details");
        return new ValidationProfile(
            (string?)root.Attribute("flavour"),
            details is null ? null : new ProfileDetails(
                ElementValue(details, "name"), ElementValue(details, "description"),
                (string?)details.Attribute("creator"), (string?)details.Attribute("created")),
            ElementValue(root, "hash"),
            AsReadOnly(root.Element(ProfileNamespace + "rules")?.Elements(ProfileNamespace + "rule")
                .Select(ReadRule) ?? []),
            AsReadOnly(root.Element(ProfileNamespace + "variables")?.Elements(ProfileNamespace + "variable")
                .Select(ReadVariable) ?? []));
    }

    private static Rule ReadRule(XElement element)
    {
        var id = element.Element(ProfileNamespace + "id")
            ?? throw new InvalidDataException("A profile rule has no id.");
        var number = (int?)id.Attribute("testNumber")
            ?? throw new InvalidDataException("A profile rule id has no test number.");
        var error = element.Element(ProfileNamespace + "error");
        return new Rule(
            new RuleId(RequiredAttribute(id, "specification"), RequiredAttribute(id, "clause"), number),
            RequiredAttribute(element, "object"),
            (bool?)element.Attribute("deferred"),
            (string?)element.Attribute("tags"),
            ElementValue(element, "description"),
            ElementValue(element, "test"),
            error is null ? null : new ErrorDetails(
                ElementValue(error, "message"),
                AsReadOnly(error.Element(ProfileNamespace + "arguments")?.Elements(ProfileNamespace + "argument")
                    .Select(argument => new ErrorArgument(argument.Value, (string?)argument.Attribute("name"))) ?? [])),
            AsReadOnly(element.Element(ProfileNamespace + "references")?.Elements(ProfileNamespace + "reference")
                .Select(reference => new RuleReference(
                    (string?)reference.Attribute("specification"), (string?)reference.Attribute("clause"))) ?? []));
    }

    private static Variable ReadVariable(XElement element) =>
        new(RequiredAttribute(element, "name"), RequiredAttribute(element, "object"),
            ElementValue(element, "defaultValue"), ElementValue(element, "value"));

    private static string RequiredAttribute(XElement element, string name) =>
        (string?)element.Attribute(name) ?? throw new InvalidDataException($"Missing '{name}' on '{element.Name.LocalName}'.");

    private static string? ElementValue(XElement element, string name) =>
        (string?)element.Element(ProfileNamespace + name);

    private static IReadOnlyList<T> AsReadOnly<T>(IEnumerable<T> values) =>
        Array.AsReadOnly(values.ToArray());
}
