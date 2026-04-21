using System.Xml.Linq;

namespace VeraPdf.NET.Validation.Profiles.Converters;

public static class VeraPdfXmlProfileConverter
{
    /// <summary>
    /// Converts a veraPDF XML profile into your JSON-compatible ValidationProfile.
    /// </summary>
    public static ValidationProfile Convert(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidOperationException("Invalid profile XML: missing root element.");
        var ns = root.Name.Namespace;

        var rules = root
            .Descendants(ns + "rule")
            .Select(r => new RuleDefinition
            {
                Id = ResolveRuleId(r, ns),
                TargetTypeName = r.Attribute("object")?.Value ?? throw new InvalidOperationException("Missing rule target object."),
                Test = r.Element(ns + "test")?.Value?.Trim() ?? throw new InvalidOperationException("Missing test expression.")
            })
            .ToList();

        var name = root
            .Element(ns + "details")?
            .Element(ns + "name")?
            .Value?
            .Trim();

        return new ValidationProfile
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? root.Attribute("flavour")?.Value ?? "veraPDF"
                : name,
            Rules = rules
        };
    }

    private static string ResolveRuleId(XElement ruleElement, XNamespace ns)
    {
        var idElement = ruleElement.Element(ns + "id");

        if (idElement == null)
        {
            return ruleElement.Attribute("id")?.Value
                ?? throw new InvalidOperationException("Missing rule id.");
        }

        var specification = idElement.Attribute("specification")?.Value;
        var clause = idElement.Attribute("clause")?.Value;
        var testNumber = idElement.Attribute("testNumber")?.Value;

        if (!string.IsNullOrWhiteSpace(specification) &&
            !string.IsNullOrWhiteSpace(clause) &&
            !string.IsNullOrWhiteSpace(testNumber))
        {
            return $"{specification}:{clause}:{testNumber}";
        }

        var fallback = string.Join(
            ":",
            new[] { specification, clause, testNumber }.Where(v => !string.IsNullOrWhiteSpace(v)));

        return string.IsNullOrWhiteSpace(fallback)
            ? throw new InvalidOperationException("Missing rule id attributes.")
            : fallback;
    }
}