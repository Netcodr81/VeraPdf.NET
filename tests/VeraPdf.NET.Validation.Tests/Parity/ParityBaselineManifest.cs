using System.Text.Json.Serialization;

namespace VeraPdf.NET.Validation.Tests.Parity;

/// <summary>
/// Defines the corpus and baseline Java output files used for parity validation.
/// </summary>
public sealed class ParityBaselineManifest
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("javaValidatorVersion")]
    public string JavaValidatorVersion { get; init; } = string.Empty;

    [JsonPropertyName("generatedUtc")]
    public DateTime GeneratedUtc { get; init; }

    [JsonPropertyName("cases")]
    public IReadOnlyList<ParityBaselineCase> Cases { get; init; } = [];
}

public sealed class ParityBaselineCase
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("profileId")]
    public string ProfileId { get; init; } = string.Empty;

    [JsonPropertyName("pdfPath")]
    public string PdfPath { get; init; } = string.Empty;

    [JsonPropertyName("javaResultPath")]
    public string JavaResultPath { get; init; } = string.Empty;
}
