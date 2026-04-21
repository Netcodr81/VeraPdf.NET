using VeraPdf.NET.Validation.Results;

namespace VeraPdf.NET.Validation.Reporting;

public static class ValidationResultNormalizer
{
    public static NormalizedValidationOutput Normalize(ValidationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var normalizedResults = result.Results
            .Select(r => new NormalizedRuleOutput
            {
                RuleId = r.RuleId,
                Passed = r.Passed,
                TargetType = r.Target.GetType().Name,
                ObjectOrder = r.ObjectOrder,
                RuleOrder = r.RuleOrder
            })
            .ToList();

        return new NormalizedValidationOutput
        {
            IsCompliant = result.IsCompliant,
            TotalRulesEvaluated = normalizedResults.Count,
            FailedRules = normalizedResults.Count(r => !r.Passed),
            Results = normalizedResults
        };
    }
}
