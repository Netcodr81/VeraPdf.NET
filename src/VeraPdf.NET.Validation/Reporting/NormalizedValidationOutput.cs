namespace VeraPdf.NET.Validation.Reporting;

public sealed class NormalizedValidationOutput
{
    public bool IsCompliant { get; init; }

    public int TotalRulesEvaluated { get; init; }

    public int FailedRules { get; init; }

    public IReadOnlyList<NormalizedRuleOutput> Results { get; init; } = [];
}

public sealed class NormalizedRuleOutput
{
    public string RuleId { get; init; } = string.Empty;

    public bool Passed { get; init; }

    public string TargetType { get; init; } = string.Empty;

    public long ObjectOrder { get; init; }

    public int RuleOrder { get; init; }
}
