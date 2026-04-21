using Shouldly;
using VeraPdf.NET.Validation.Reporting;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Parity;

public class NormalizedOutputContractTests
{
    [Fact]
    public void Normalize_Should_Exclude_Raw_Target_Object_And_Expose_Deterministic_Fields()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage { Number = 1 },
                new TestPage { Number = 2 }
            }
        };

        var validator = TestHelper.CreateValidator("Number > 0", typeof(TestPage));

        var result = validator.Validate(doc);

        var normalized = ValidationResultNormalizer.Normalize(result);

        normalized.TotalRulesEvaluated.ShouldBe(2);
        normalized.FailedRules.ShouldBe(0);
        normalized.IsCompliant.ShouldBeTrue();
        normalized.Results.All(r => !string.IsNullOrWhiteSpace(r.TargetType)).ShouldBeTrue();
    }

    [Fact]
    public void GenerateNormalized_Should_Produce_Contract_With_Aggregate_Fields()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage { Number = 1 },
                new TestPage { Number = -1 }
            }
        };

        var validator = TestHelper.CreateValidator("Number > 0", typeof(TestPage));
        var result = validator.Validate(doc);

        var generator = new JsonReportGenerator();
        var json = generator.GenerateNormalized(result);

        json.ShouldContain("\"totalRulesEvaluated\"");
        json.ShouldContain("\"failedRules\"");
        json.ShouldContain("\"targetType\"");
        json.ShouldNotContain("\"target\"");
    }
}
