namespace VeraPdf.NET.Validation.Profiles;

public sealed class ValidationProfile
{
    public string Name { get; init; } = default!;
    public IReadOnlyList<RuleDefinition> Rules { get; init; } = [];
}
