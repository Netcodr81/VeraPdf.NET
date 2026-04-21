namespace VeraPdf.NET.Validation.Results;

public sealed class RuleResult
{
    public string RuleId { get; }
    public bool Passed { get; }
    public object Target { get; }

    /// <summary>
    /// Object traversal order used for deterministic output ordering.
    /// </summary>
    public long ObjectOrder { get; }

    /// <summary>
    /// Rule order for the current object used for deterministic output ordering.
    /// </summary>
    public int RuleOrder { get; }

    public RuleResult(string ruleId, bool passed, object target, long objectOrder = long.MaxValue, int ruleOrder = int.MaxValue)
    {
        RuleId = ruleId;
        Passed = passed;
        Target = target;
        ObjectOrder = objectOrder;
        RuleOrder = ruleOrder;
    }
}
