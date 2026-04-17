namespace VeraPdf.NET.Validation.Results;

public sealed class ValidationResult
{
    public IReadOnlyList<RuleResult> Results { get; }

    public bool IsCompliant => Results.All(r => r.Passed);

    public ValidationResult(IReadOnlyList<RuleResult> results)
    {
        Results = results;
    }
}
