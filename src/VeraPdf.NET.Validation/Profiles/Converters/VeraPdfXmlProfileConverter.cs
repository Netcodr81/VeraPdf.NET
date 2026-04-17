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

        var rules = doc.Descendants("rule")
            .Select(r => new RuleDefinition
            {
                Id = r.Attribute("id")?.Value ?? throw new Exception("Missing rule id"),

                // Map veraPDF object → your interface naming convention
                TargetTypeName = MapType(r.Attribute("object")?.Value),

                Test = r.Element("test")?.Value?.Trim()
                       ?? throw new Exception("Missing test expression")
            })
            .ToList();

        return new ValidationProfile
        {
            Name = doc.Root?.Attribute("name")?.Value ?? "veraPDF",
            Rules = rules
        };
    }

    /// <summary>
    /// Maps veraPDF type names to your .NET model interfaces.
    /// Example: PDPage → IPDPage
    /// </summary>
    private static string MapType(string? veraType)
    {
        return veraType switch
        {
            "PDPage" => "IPDPage",
            "PDDocument" => "IPDDocument",
            "PDAnnot" => "IPDAnnot",
            _ => $"I{veraType}"
        };
    }
}