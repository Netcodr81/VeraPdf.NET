using Microsoft.Extensions.Logging.Abstractions;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;
using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation.Tests;

public class VeraPdfValidationIntegrationTests
{
    private static readonly string FixturesPath = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    [Fact]
    public async Task ValidateAsync_WithRealRuntime_Should_Return_Json_Report_For_Structural_Pdf()
    {
        var (service, runtimeRootPath) = CreateRealService();

        try
        {
            await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));
            var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

            Assert.NotNull(report.ParserPrecheck);
            Assert.True(report.ParserPrecheck!.Success);
            Assert.Single(report.Reports);
            Assert.Equal("PDF-A", report.Reports[0].Standard);
            Assert.NotEqual("{}", report.Reports[0].JsonReport.Trim());
            Assert.Contains("jobs", report.Reports[0].JsonReport, StringComparison.OrdinalIgnoreCase);
            Assert.NotNull(report.Reports[0].ParsedReport);
            Assert.NotEmpty(report.Reports[0].ParsedReport!.Jobs);
        }
        finally
        {
            CleanupRuntime(runtimeRootPath);
        }
    }

    [Fact]
    public async Task ValidateAsync_WithRealRuntime_Should_ShortCircuit_Invalid_Pdf_Before_Cli()
    {
        var (service, runtimeRootPath) = CreateRealService();

        try
        {
            await using var stream = File.OpenRead(Path.Combine(FixturesPath, "invalid.pdf"));
            var report = await service.ValidateAsync(stream, "invalid.pdf", ValidationStandard.PdfA);

            Assert.NotNull(report.ParserPrecheck);
            Assert.False(report.ParserPrecheck!.Success);
            Assert.Single(report.Errors);
            Assert.Equal(ValidationErrorCode.InvalidPdf, report.Errors[0].Code);
            Assert.Empty(report.Reports);
        }
        finally
        {
            CleanupRuntime(runtimeRootPath);
        }
    }

    private static (VeraPdfValidationService service, string runtimeRootPath) CreateRealService()
    {
        var rootPath = FindRepositoryRoot();
        var validationProjectPath = Path.Combine(rootPath, "src", "VeraPdf.NET.Validation");
        var runtimeRootPath = Path.Combine(Path.GetTempPath(), "VeraPdf.NET", "integration", Guid.NewGuid().ToString("N"));

        var options = Microsoft.Extensions.Options.Options.Create(new VeraPdfRuntimeOptions
        {
            VeraPdfZipPath = Path.Combine(validationProjectPath, "veraPDF.zip"),
            JavaZipPath = Path.Combine(validationProjectPath, "Java.zip"),
            RuntimeRootPath = runtimeRootPath,
            ProcessTimeout = TimeSpan.FromMinutes(2)
        });

        var runtimeProvisioner = new VeraPdfRuntimeProvisioner(options);
        var processRunner = new DefaultProcessRunner();

        var service = new VeraPdfValidationService(new Parser.PdfParser(), runtimeProvisioner, processRunner, options, NullLogger<VeraPdfValidationService>.Instance);
        return (service, runtimeRootPath);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "VeraPdfNet.slnx");
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test execution directory.");
    }

    private static void CleanupRuntime(string runtimeRootPath)
    {
        if (Directory.Exists(runtimeRootPath))
        {
            try
            {
                Directory.Delete(runtimeRootPath, recursive: true);
            }
            catch
            {
                // ignore cleanup failures in test runs
            }
        }
    }
}
