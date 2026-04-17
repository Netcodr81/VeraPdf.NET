using System.Text.Json;
using VeraPdf.NET.Validation.Results;

namespace VeraPdf.NET.Validation.Reporting;

public sealed class JsonReportGenerator
{
    /// <summary>
    /// Serializes validation results to JSON.
    /// Replaces veraPDF XML reporting.
    /// </summary>
    public string Generate(ValidationResult result)
    {
        return JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}