namespace VeraPdf.NET.Validation.Profiles;

public sealed class RuleDefinition
{
    public string Id { get; init; } = default!;

    /// <summary>
    /// Original string from XML/JSON profile.
    /// </summary>
    public string TargetTypeName { get; init; } = default!;

    /// <summary>
    /// Resolved CLR type used during validation.
    /// </summary>
    public Type TargetType { get; set; } = default!;

    public string Test { get; init; } = default!;
}