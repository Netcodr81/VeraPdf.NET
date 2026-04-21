namespace VeraPdf.NET.Validation.RuleSets.Loading;

internal sealed class CompiledProfileManifest
{
    public int Version { get; init; }
    public DateTime GeneratedUtc { get; init; }
    public IReadOnlyList<CompiledProfileEntry> Profiles { get; init; } = [];
}

internal sealed class CompiledProfileEntry
{
    public string Id { get; init; } = string.Empty;
    public string SourcePath { get; init; } = string.Empty;
    public string Xml { get; init; } = string.Empty;
}
