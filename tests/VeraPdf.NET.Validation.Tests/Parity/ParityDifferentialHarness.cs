using System.Text.Json;
using VeraPdf.NET.Validation.Reporting;

namespace VeraPdf.NET.Validation.Tests.Parity;

public static class ParityDifferentialHarness
{
    public static ParityDifferentialManifestResult CompareManifest(
        string manifestAbsolutePath,
        Func<ParityBaselineCase, NormalizedValidationOutput> actualOutputFactory)
    {
        if (actualOutputFactory == null)
            throw new ArgumentNullException(nameof(actualOutputFactory));

        var manifest = ParityBaselineFixtureLoader.LoadManifest(manifestAbsolutePath);

        var caseResults = new Dictionary<string, ParityDifferentialComparisonResult>(StringComparer.Ordinal);

        foreach (var @case in manifest.Cases)
        {
            var expectedPath = ParityBaselineFixtureLoader.ResolveCaseJavaResultPath(manifestAbsolutePath, @case);
            var expected = LoadNormalizedOutput(expectedPath);
            var actual = actualOutputFactory(@case);

            caseResults[@case.Id] = CompareOutputs(expected, actual);
        }

        return new ParityDifferentialManifestResult
        {
            TotalCases = manifest.Cases.Count,
            MatchedCases = caseResults.Count(kvp => kvp.Value.IsMatch),
            CaseResults = caseResults
        };
    }

    public static ParityDifferentialComparisonResult CompareOutputs(
        NormalizedValidationOutput expected,
        NormalizedValidationOutput actual)
    {
        var differences = new List<string>();

        if (expected.IsCompliant != actual.IsCompliant)
            differences.Add($"IsCompliant mismatch: expected={expected.IsCompliant}, actual={actual.IsCompliant}");

        if (expected.TotalRulesEvaluated != actual.TotalRulesEvaluated)
            differences.Add($"TotalRulesEvaluated mismatch: expected={expected.TotalRulesEvaluated}, actual={actual.TotalRulesEvaluated}");

        if (expected.FailedRules != actual.FailedRules)
            differences.Add($"FailedRules mismatch: expected={expected.FailedRules}, actual={actual.FailedRules}");

        var expectedMap = expected.Results.ToDictionary(ResultKey, r => r, StringComparer.Ordinal);
        var actualMap = actual.Results.ToDictionary(ResultKey, r => r, StringComparer.Ordinal);

        foreach (var expectedKey in expectedMap.Keys)
        {
            if (!actualMap.TryGetValue(expectedKey, out var actualRule))
            {
                differences.Add($"Missing rule result in actual output: {expectedKey}");
                continue;
            }

            var expectedRule = expectedMap[expectedKey];
            if (expectedRule.Passed != actualRule.Passed)
                differences.Add($"Rule pass mismatch for {expectedKey}: expected={expectedRule.Passed}, actual={actualRule.Passed}");

            if (!string.Equals(expectedRule.TargetType, actualRule.TargetType, StringComparison.Ordinal))
                differences.Add($"Rule target type mismatch for {expectedKey}: expected={expectedRule.TargetType}, actual={actualRule.TargetType}");
        }

        foreach (var actualKey in actualMap.Keys)
        {
            if (!expectedMap.ContainsKey(actualKey))
                differences.Add($"Unexpected rule result in actual output: {actualKey}");
        }

        return new ParityDifferentialComparisonResult
        {
            IsMatch = differences.Count == 0,
            Differences = differences,
            Expected = expected,
            Actual = actual
        };
    }

    private static string ResultKey(NormalizedRuleOutput rule)
    {
        return $"{rule.ObjectOrder}:{rule.RuleOrder}:{rule.RuleId}";
    }

    private static NormalizedValidationOutput LoadNormalizedOutput(string absolutePath)
    {
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("Expected normalized baseline output file not found.", absolutePath);

        using var stream = File.OpenRead(absolutePath);

        var output = JsonSerializer.Deserialize<NormalizedValidationOutput>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return output ?? throw new InvalidOperationException($"Normalized output at '{absolutePath}' is invalid.");
    }
}
