using System.Text.Json;
using Shouldly;
using VeraPdf.NET.Validation.Reporting;

namespace VeraPdf.NET.Validation.Tests.Parity;

public sealed class ParityCorpusComparisonIntegrationTests
{
    [Fact]
    public void CompareManifest_Should_Match_Single_Staged_Case_EndToEnd()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "verapdf-parity", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);

        try
        {
            var pdfDir = Path.Combine(tempRoot, "pdf");
            var javaDir = Path.Combine(tempRoot, "java-results");
            Directory.CreateDirectory(pdfDir);
            Directory.CreateDirectory(javaDir);

            var pdfPath = Path.Combine(pdfDir, "case-001.pdf");
            File.WriteAllText(pdfPath, "%PDF-1.7\n1 0 obj\n<<>>\nendobj\n%%EOF");

            var expectedOutput = new NormalizedValidationOutput
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
                        TargetType = "CosDocument",
                        ObjectOrder = 0,
                        RuleOrder = 0
                    }
                ]
            };

            var javaResultPath = Path.Combine(javaDir, "case-001.json");
            File.WriteAllText(
                javaResultPath,
                JsonSerializer.Serialize(expectedOutput, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

            var manifest = new
            {
                version = 1,
                javaValidatorVersion = "mock-java-1.0",
                generatedUtc = DateTime.UtcNow,
                cases = new[]
                {
                    new
                    {
                        id = "case-001",
                        profileId = "pdfa-1b",
                        pdfPath = "pdf/case-001.pdf",
                        javaResultPath = "java-results/case-001.json"
                    }
                }
            };

            var manifestPath = Path.Combine(tempRoot, "parity-baseline.manifest.json");
            File.WriteAllText(
                manifestPath,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));

            var result = ParityDifferentialHarness.CompareManifest(
                manifestPath,
                _ => expectedOutput);

            result.TotalCases.ShouldBe(1);
            result.MatchedCases.ShouldBe(1);
            result.CaseResults.ContainsKey("case-001").ShouldBeTrue();
            result.CaseResults["case-001"].IsMatch.ShouldBeTrue();
            result.CaseResults["case-001"].Differences.Count.ShouldBe(0);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
                Directory.Delete(tempRoot, recursive: true);
        }
    }
}
