using Shouldly;
using VeraPdf.NET.Validation.Reporting;

namespace VeraPdf.NET.Validation.Tests.Parity;

public class ParityDifferentialHarnessTests
{
    [Fact]
    public void CompareOutputs_Should_Report_Match_For_Equivalent_Outputs()
    {
        var expected = new NormalizedValidationOutput
        {
            IsCompliant = true,
            TotalRulesEvaluated = 2,
            FailedRules = 0,
            Results =
            [
                new NormalizedRuleOutput
                {
                    RuleId = "r1",
                    Passed = true,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 0
                },
                new NormalizedRuleOutput
                {
                    RuleId = "r2",
                    Passed = true,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 1
                }
            ]
        };

        var actual = new NormalizedValidationOutput
        {
            IsCompliant = true,
            TotalRulesEvaluated = 2,
            FailedRules = 0,
            Results =
            [
                new NormalizedRuleOutput
                {
                    RuleId = "r1",
                    Passed = true,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 0
                },
                new NormalizedRuleOutput
                {
                    RuleId = "r2",
                    Passed = true,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 1
                }
            ]
        };

        var result = ParityDifferentialHarness.CompareOutputs(expected, actual);

        result.IsMatch.ShouldBeTrue();
        result.Differences.Count.ShouldBe(0);
    }

    [Fact]
    public void CompareOutputs_Should_Report_Differences_For_Mismatches()
    {
        var expected = new NormalizedValidationOutput
        {
            IsCompliant = true,
            TotalRulesEvaluated = 1,
            FailedRules = 0,
            Results =
            [
                new NormalizedRuleOutput
                {
                    RuleId = "r1",
                    Passed = true,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 0
                }
            ]
        };

        var actual = new NormalizedValidationOutput
        {
            IsCompliant = false,
            TotalRulesEvaluated = 1,
            FailedRules = 1,
            Results =
            [
                new NormalizedRuleOutput
                {
                    RuleId = "r1",
                    Passed = false,
                    TargetType = "TestPage",
                    ObjectOrder = 0,
                    RuleOrder = 0
                }
            ]
        };

        var result = ParityDifferentialHarness.CompareOutputs(expected, actual);

        result.IsMatch.ShouldBeFalse();
        result.Differences.Count.ShouldBeGreaterThan(0);
        result.Differences.Any(d => d.Contains("IsCompliant mismatch", StringComparison.Ordinal)).ShouldBeTrue();
        result.Differences.Any(d => d.Contains("Rule pass mismatch", StringComparison.Ordinal)).ShouldBeTrue();
    }
}
