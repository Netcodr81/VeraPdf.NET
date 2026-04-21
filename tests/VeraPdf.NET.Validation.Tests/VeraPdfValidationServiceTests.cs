using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using VeraPdf.NET.Parser;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;
using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation.Tests;

public class VeraPdfValidationServiceTests
{
    private static readonly string FixturesPath = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    [Fact]
    public async Task ValidateAsync_Should_Pass_For_Valid_Pdf()
    {
        var service = CreateService();
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

        Assert.NotNull(report.ParserPrecheck);
        Assert.True(report.ParserPrecheck!.Success);
        Assert.True(report.Passed);
        Assert.Single(report.Reports);
        Assert.Equal("PDF-A", report.Reports[0].Standard);
        Assert.Equal(ValidationErrorCode.None, report.Reports[0].ErrorCode);
        Assert.NotNull(report.Reports[0].ParsedReport);
    }

    [Fact]
    public async Task ValidateAsync_Should_Fail_For_Invalid_Pdf()
    {
        var processRunner = new FakeProcessRunner();
        var service = CreateService(processRunner: processRunner);
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "invalid.pdf"));

        var report = await service.ValidateAsync(stream, "invalid.pdf", ValidationStandard.PdfA);

        Assert.NotNull(report.ParserPrecheck);
        Assert.False(report.ParserPrecheck!.Success);
        Assert.False(report.Passed);
        Assert.Empty(report.Reports);
        Assert.Single(report.Errors);
        Assert.Equal(ValidationErrorCode.InvalidPdf, report.Errors[0].Code);
        Assert.Empty(processRunner.Arguments);
        Assert.Contains(report.ParserPrecheck.Diagnostics, d => d.Severity == "Error");
    }

    [Fact]
    public async Task ValidateAsync_Should_Return_Combined_Report_For_All_Selected_Standards()
    {
        var service = CreateService();
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.All);

        Assert.NotNull(report.ParserPrecheck);
        Assert.True(report.ParserPrecheck!.Success);
        Assert.True(report.Passed);
        Assert.Equal(3, report.Reports.Count);
        Assert.Contains(report.Reports, r => r.Standard == "PDF-A");
        Assert.Contains(report.Reports, r => r.Standard == "PDF-UA");
        Assert.Contains(report.Reports, r => r.Standard == "WCAG 2.2");
    }

    [Fact]
    public async Task ValidateAsync_Should_Use_Verified_VeraPdf_Flavours()
    {
        var processRunner = new FakeProcessRunner();
        var service = CreateService(processRunner: processRunner);
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        _ = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.All);

        Assert.Contains(processRunner.Arguments, a => a.Contains("--flavour 0", StringComparison.Ordinal));
        Assert.Contains(processRunner.Arguments, a => a.Contains("--flavour ua1", StringComparison.Ordinal));
        Assert.Contains(processRunner.Arguments, a => a.Contains("--flavour wt1a", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ValidateAsync_Should_Set_ProcessFailed_ErrorCode_When_Cli_Exits_NonZero()
    {
        var processRunner = new FakeProcessRunner
        {
            ForceFailure = true
        };

        var service = CreateService(processRunner: processRunner);
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

        Assert.Single(report.Reports);
        Assert.Equal(ValidationErrorCode.ProcessFailed, report.Reports[0].ErrorCode);
        Assert.False(report.Reports[0].Passed);
    }

    [Fact]
    public async Task ValidateAsync_Should_Set_ProcessTimeout_ErrorCode_When_Cli_Times_Out()
    {
        var processRunner = new FakeProcessRunner
        {
            ForceTimeout = true
        };

        var service = CreateService(processRunner: processRunner);
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

        Assert.Single(report.Reports);
        Assert.Equal(ValidationErrorCode.ProcessTimeout, report.Reports[0].ErrorCode);
        Assert.False(report.Reports[0].Passed);
        Assert.True(report.Reports[0].TimedOut);
    }

    [Fact]
    public async Task ValidateAsync_Should_Return_RuntimeUnavailable_Error_When_Runtime_Provisioning_Fails()
    {
        var service = CreateService(runtimeProvisioner: new FailingRuntimeProvisioner());
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

        Assert.Single(report.Errors);
        Assert.Equal(ValidationErrorCode.RuntimeUnavailable, report.Errors[0].Code);
        Assert.Empty(report.Reports);
    }

    [Fact]
    public async Task ValidateAsync_Should_Return_InvalidInput_When_MaxConcurrentValidations_Is_Not_Positive()
    {
        var service = CreateService(runtimeOptions: new VeraPdfRuntimeOptions
        {
            MaxConcurrentValidations = 0
        });

        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));
        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA);

        Assert.Single(report.Errors);
        Assert.Equal(ValidationErrorCode.InvalidInput, report.Errors[0].Code);
        Assert.Equal("MaxConcurrentValidations", report.Errors[0].Target);
    }

    [Fact]
    public async Task ValidateAsync_Should_Populate_Normalized_Summary()
    {
        var service = CreateService();
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var report = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.All);

        Assert.Equal(3, report.Summary.TotalStandardsChecked);
        Assert.Equal(3, report.Summary.PassedStandards);
        Assert.Equal(0, report.Summary.FailedStandards);
        Assert.False(report.Summary.HasParserErrors);
        Assert.Empty(report.Summary.FailedStandardsList);
    }

    [Fact]
    public async Task ValidateAsync_Should_Use_PerRequest_Profile_Overrides()
    {
        var processRunner = new FakeProcessRunner();
        var service = CreateService(processRunner: processRunner);
        await using var stream = File.OpenRead(Path.Combine(FixturesPath, "valid.pdf"));

        var options = new ValidationExecutionOptions
        {
            ProfileOverrides = new ValidationProfileOverrides
            {
                PdfAArguments = "--format json --flavour 2b"
            }
        };

        _ = await service.ValidateAsync(stream, "valid.pdf", ValidationStandard.PdfA, options);

        Assert.Contains(processRunner.Arguments, a => a.Contains("--flavour 2b", StringComparison.Ordinal));
    }

    private static VeraPdfValidationService CreateService(
        VeraPdfRuntimeOptions? runtimeOptions = null,
        IVeraPdfRuntimeProvisioner? runtimeProvisioner = null,
        FakeProcessRunner? processRunner = null)
    {
        var options = Options.Create(runtimeOptions ?? new VeraPdfRuntimeOptions());
        runtimeProvisioner ??= new FakeRuntimeProvisioner();
        processRunner ??= new FakeProcessRunner();

        return new VeraPdfValidationService(new PdfParser(), runtimeProvisioner, processRunner, options, NullLogger<VeraPdfValidationService>.Instance);
    }

    private sealed class FakeRuntimeProvisioner : IVeraPdfRuntimeProvisioner
    {
        public Task<VeraPdfRuntime> EnsureRuntimeAsync(CancellationToken cancellationToken)
            => Task.FromResult(new VeraPdfRuntime("verapdf", "fake-java", Path.GetTempPath()));
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public List<string> Arguments { get; } = [];

        public bool ForceFailure { get; init; }

        public bool ForceTimeout { get; init; }

        public Task<ProcessExecutionResult> RunAsync(
            string fileName,
            string arguments,
            string workingDirectory,
            IReadOnlyDictionary<string, string> environment,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Arguments.Add(arguments);

            if (ForceTimeout)
            {
                return Task.FromResult(new ProcessExecutionResult(-1, string.Empty, "Timeout", true));
            }

            if (ForceFailure)
            {
                return Task.FromResult(new ProcessExecutionResult(1, "{\"report\":{\"jobs\":[{\"validationResult\":[{\"compliant\":false}]}]}}", "Failed", false));
            }

            var pdfPath = arguments.Split('"', StringSplitOptions.RemoveEmptyEntries).Last();
            var bytes = File.ReadAllBytes(pdfPath);
            var isPdf = bytes.Length >= 5
                        && bytes[0] == (byte)'%'
                        && bytes[1] == (byte)'P'
                        && bytes[2] == (byte)'D'
                        && bytes[3] == (byte)'F'
                        && bytes[4] == (byte)'-';

            if (isPdf)
            {
                return Task.FromResult(new ProcessExecutionResult(0, "{\"report\":{\"jobs\":[{\"validationResult\":[{\"compliant\":true}]}]}}", string.Empty, false));
            }

            return Task.FromResult(new ProcessExecutionResult(1, "{\"report\":{\"jobs\":[{\"validationResult\":[{\"compliant\":false}]}]}}", "Invalid PDF", false));
        }
    }

    private sealed class FailingRuntimeProvisioner : IVeraPdfRuntimeProvisioner
    {
        public Task<VeraPdfRuntime> EnsureRuntimeAsync(CancellationToken cancellationToken)
            => throw new FileNotFoundException("Missing runtime archive");
    }
}
