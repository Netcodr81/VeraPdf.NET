using System.Xml.Linq;

namespace VeraPdf.NET.RuleSets.Extraction;

/// <summary>
/// Extracts raw rules from veraPDF XML.
/// Handles nested structures and inconsistencies.
/// </summary>
public static class VeraPdfRuleExtractor
{
    public static List<RawRule> Extract(string xml)
    {
        var doc = XDocument.Parse(xml);

        var rules = new List<RawRule>();

        foreach (var r in doc.Descendants())
        {
            if (r.Name.LocalName != "rule")
                continue;

            var id = r.Attribute("id")?.Value;
            var obj = r.Attribute("object")?.Value;

            var test = r.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "test")
                ?.Value?.Trim();

            if (id == null || obj == null || test == null)
                continue;

            rules.Add(new RawRule
            {
                Id = id,
                Object = obj,
                Test = test
            });
        }

        return rules;
    }
}

public sealed class RawRule
{
    public string Id { get; set; } = default!;
    public string Object { get; set; } = default!;
    public string Test { get; set; } = default!;
}