namespace VeraPdf.NET.Validation.Results;

public sealed class RuleResult
{
    public string RuleId { get; }
    public bool Passed { get; }
    public object Target { get; }

    public RuleResult(string ruleId, bool passed, object target)
    {
        RuleId = ruleId;
        Passed = passed;
        Target = target;
    }
}
