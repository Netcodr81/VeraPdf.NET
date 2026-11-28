using VeraPdf.Library.Profiles;
using VeraPdf.Library.Results;

namespace VeraPdf.Library.Tests;

public class ValidationResultTests
{
    [Theory]
    [InlineData("root/(123 abcd)/child", "root/(abcd)/child")]
    [InlineData("(12 abcd)/(99 efgh)", "(abcd)/(abcd)")]
    [InlineData("(1 abc)/(2 abcde)", "(1 abc)/(abcde)")]
    [InlineData(null, null)]
    public void NormalizesLocationsLikeReference(string? input, string? expected)
    {
        Assert.Equal(expected, Location.FromValues("level", input).Context);
    }

    [Fact]
    public void StripPassedAssertionsPreservesExplicitResultAndAggregateCounts()
    {
        var profile = BuiltInProfiles.Read("PDFA-1B");
        var id = profile.Rules[0].Id;
        var arguments = new List<EvaluatedErrorArgument> { new("header", null, "value") };
        var passed = new TestAssertion(1, id, AssertionStatus.Passed, null, null, null, null, arguments);
        var failed = new TestAssertion(2, id, AssertionStatus.Failed, "failure", null, "ctx", "error", null);
        var unknown = new TestAssertion(3, id, AssertionStatus.Unknown, null, null, null, null, null);
        var assertions = new List<TestAssertion> { passed, failed, unknown };
        var counts = new Dictionary<RuleId, int> { [id] = 4 };
        var result = new ValidationResult(profile, assertions, false, 10, JobEndStatus.Timeout, counts);
        assertions.Clear();
        arguments.Clear();
        counts.Clear();

        var filtered = result.WithoutPassedAssertions();
        Assert.Equal(3, result.Assertions.Count);
        Assert.Single(passed.ErrorArguments!);
        Assert.Equal(new[] { failed, unknown }, filtered.Assertions);
        Assert.Equal(10, filtered.TotalAssertions);
        Assert.False(filtered.IsCompliant);
        Assert.Equal(JobEndStatus.Timeout, filtered.EndStatus);
        Assert.Equal(4, filtered.FailedChecks![id]);
        Assert.Equal(profile.Flavour, filtered.Flavour);
    }

    [Fact]
    public void DoesNotInferComplianceFromAnEmptyOrPartialList()
    {
        var profile = BuiltInProfiles.Read("PDFA-1B");
        var result = new ValidationResult(profile, [], false, 5, JobEndStatus.Cancelled);
        Assert.False(result.IsCompliant);
        Assert.Equal(5, result.TotalAssertions);
        Assert.Null(result.FailedChecks);
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValidationResult(profile, [], false, -1, JobEndStatus.Normal));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValidationResult(profile, [], false, 0, (JobEndStatus)99));
    }
}
