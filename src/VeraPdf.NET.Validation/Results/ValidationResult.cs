namespace VeraPdf.NET.Validation.Results;

public sealed class ValidationResult
{
    public IReadOnlyList<RuleResult> Results { get; }

    public bool IsCompliant => Results.All(r => r.Passed);

    public ValidationResult(IReadOnlyList<RuleResult> results)
    {
        if (results.Any(r => r.ObjectOrder != long.MaxValue || r.RuleOrder != int.MaxValue))
        {
            Results = results
                .OrderBy(r => r.ObjectOrder)
                .ThenBy(r => r.RuleOrder)
                .ThenBy(r => r.RuleId, StringComparer.Ordinal)
                .ToList();

            return;
        }

        Results = results;
    }
}
