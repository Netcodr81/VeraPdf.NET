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

    /// <summary>
    /// Serializes normalized output contract JSON for deterministic diffing.
    /// </summary>
    public string GenerateNormalized(ValidationResult result)
    {
        var normalized = ValidationResultNormalizer.Normalize(result);

        return GenerateNormalized(normalized);
    }

    /// <summary>
    /// Serializes normalized output contract JSON for deterministic diffing.
    /// </summary>
    public string GenerateNormalized(NormalizedValidationOutput output)
    {
        return JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}