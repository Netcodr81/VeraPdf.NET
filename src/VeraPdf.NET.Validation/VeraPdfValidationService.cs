using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VeraPdf.NET.Parser;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;
using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.Validation;

internal sealed class VeraPdfValidationService(
    PdfParser pdfParser,
    IVeraPdfRuntimeProvisioner runtimeProvisioner,
    IProcessRunner processRunner,
    IOptions<VeraPdfRuntimeOptions> options,
    ILogger<VeraPdfValidationService> logger) : IVeraPdfValidationService
{
    private static readonly Meter Meter = new("VeraPdf.NET.Validation", "1.0.0");
    private static readonly Counter<long> ValidationRequests = Meter.CreateCounter<long>("verapdf.validation.requests");
    private static readonly Counter<long> ValidationFailures = Meter.CreateCounter<long>("verapdf.validation.failures");
    private static readonly Histogram<double> ValidationDurationMs = Meter.CreateHistogram<double>("verapdf.validation.duration.ms");
    private static readonly JsonSerializerOptions VeraPdfJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly PdfParser _pdfParser = pdfParser;
    private readonly IVeraPdfRuntimeProvisioner _runtimeProvisioner = runtimeProvisioner;
    private readonly IProcessRunner _processRunner = processRunner;
    private readonly VeraPdfRuntimeOptions _options = options.Value;
    private readonly ILogger<VeraPdfValidationService> _logger = logger;
    private readonly SemaphoreSlim _validationThrottle = new(Math.Max(1, options.Value.MaxConcurrentValidations), Math.Max(1, options.Value.MaxConcurrentValidations));

    public async Task<VeraPdfValidationReport> ValidateAsync(
        Stream pdfStream,
        string fileName,
        ValidationStandard standards,
        ValidationExecutionOptions? executionOptions = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        ValidationRequests.Add(1);
        var throttleEntered = false;

        try
        {
            if (_options.MaxConcurrentValidations <= 0)
            {
                return BuildFailureReport(fileName, standards, ValidationErrorCode.InvalidInput, "MaxConcurrentValidations must be greater than 0.", "MaxConcurrentValidations");
            }

            await _validationThrottle.WaitAsync(cancellationToken);
            throttleEntered = true;

            if (pdfStream is null)
            {
                throw new ArgumentNullException(nameof(pdfStream));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BuildFailureReport(fileName, standards, ValidationErrorCode.InvalidInput, "File name is required.", "fileName");
            }

            if (standards == ValidationStandard.None)
            {
                return BuildFailureReport(fileName, standards, ValidationErrorCode.InvalidInput, "At least one validation standard must be selected.", "standards");
            }

            _logger.LogInformation("Starting validation for {FileName} with standards {Standards}", fileName, standards);

            var pdfBytes = await ReadAllBytesAsync(pdfStream, cancellationToken);
            var parseResult = _pdfParser.Parse(pdfBytes, new PdfParseOptions
            {
                StrictHeaderCheck = false,
                RequireEofMarker = true,
                RequireStartXref = false
            });

            var parserPrecheck = new ParserPrecheckReport
            {
                Success = parseResult.Success,
                Diagnostics = parseResult.Diagnostics
                    .Select(d => new ParserDiagnosticReport
                    {
                        Code = d.Code,
                        Message = d.Message,
                        Severity = d.Severity.ToString()
                    })
                    .ToArray()
            };

            if (!parseResult.Success)
            {
                ValidationFailures.Add(1, new KeyValuePair<string, object?>("reason", ValidationErrorCode.InvalidPdf.ToString()));
                _logger.LogWarning("Parser precheck failed for {FileName}", fileName);

                return new VeraPdfValidationReport
                {
                    FileName = fileName,
                    RequestedStandards = standards,
                    GeneratedAtUtc = DateTimeOffset.UtcNow,
                    ParserPrecheck = parserPrecheck,
                    Errors =
                    [
                        new ValidationError
                        {
                            Code = ValidationErrorCode.InvalidPdf,
                            Message = "The PDF failed structural precheck and external standard validation was skipped.",
                            Target = "pdf"
                        }
                    ],
                    Reports = [],
                    Summary = CreateSummary(parserPrecheck, [],
                    [
                        new ValidationError
                        {
                            Code = ValidationErrorCode.InvalidPdf,
                            Message = "The PDF failed structural precheck and external standard validation was skipped.",
                            Target = "pdf"
                        }
                    ])
                };
            }

            VeraPdfRuntime runtime;
            try
            {
                runtime = await _runtimeProvisioner.EnsureRuntimeAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                ValidationFailures.Add(1, new KeyValuePair<string, object?>("reason", ValidationErrorCode.RuntimeUnavailable.ToString()));
                _logger.LogError(ex, "Runtime provisioning failed for {FileName}", fileName);

                return new VeraPdfValidationReport
                {
                    FileName = fileName,
                    RequestedStandards = standards,
                    GeneratedAtUtc = DateTimeOffset.UtcNow,
                    ParserPrecheck = parserPrecheck,
                    Errors =
                    [
                        new ValidationError
                        {
                            Code = ValidationErrorCode.RuntimeUnavailable,
                            Message = ex.Message,
                            Target = "runtime"
                        }
                    ],
                    Reports = [],
                    Summary = CreateSummary(parserPrecheck, [],
                    [
                        new ValidationError
                        {
                            Code = ValidationErrorCode.RuntimeUnavailable,
                            Message = ex.Message,
                            Target = "runtime"
                        }
                    ])
                };
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"verapdf-{Guid.NewGuid():N}.pdf");
            await File.WriteAllBytesAsync(tempFilePath, pdfBytes, cancellationToken);

            try
            {
                var reports = new List<StandardValidationReport>();

                foreach (var standard in ExpandStandards(standards))
                {
                    var arguments = BuildArguments(standard, tempFilePath, executionOptions);
                    var environment = BuildEnvironment(runtime.JavaHomePath);
                    var result = await _processRunner.RunAsync(
                        runtime.VeraPdfExecutablePath,
                        arguments,
                        runtime.WorkingDirectory,
                        environment,
                        _options.ProcessTimeout,
                        cancellationToken);

                    var errorCode = ResolveErrorCode(result);
                    if (errorCode != ValidationErrorCode.None)
                    {
                        ValidationFailures.Add(1, new KeyValuePair<string, object?>("reason", errorCode.ToString()));
                    }

                    var rawJsonReport = string.IsNullOrWhiteSpace(result.StdOut) ? "{}" : result.StdOut;
                    reports.Add(new StandardValidationReport
                    {
                        Standard = ToName(standard),
                        Passed = result.ExitCode == 0 && !result.TimedOut,
                        ExitCode = result.ExitCode,
                        ExitCodeDescription = DescribeVeraPdfExitCode(result.ExitCode, result.TimedOut),
                        TimedOut = result.TimedOut,
                        ErrorCode = errorCode,
                        ErrorCodeDescription = DescribeValidationErrorCode(errorCode),
                        JsonReport = rawJsonReport,
                        ParsedReport = ParseVeraPdfReport(rawJsonReport),
                        ErrorMessage = string.IsNullOrWhiteSpace(result.StdErr) ? null : result.StdErr
                    });
                }

                _logger.LogInformation("Completed validation for {FileName} with result {Passed}", fileName, reports.All(r => r.Passed));
                return new VeraPdfValidationReport
                {
                    FileName = fileName,
                    RequestedStandards = standards,
                    GeneratedAtUtc = DateTimeOffset.UtcNow,
                    ParserPrecheck = parserPrecheck,
                    Reports = reports,
                    Summary = CreateSummary(parserPrecheck, reports, [])
                };
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
        }
        catch (OperationCanceledException)
        {
            ValidationFailures.Add(1, new KeyValuePair<string, object?>("reason", ValidationErrorCode.ProcessTimeout.ToString()));
            _logger.LogWarning("Validation was cancelled for {FileName}", fileName);
            throw;
        }
        catch (Exception ex)
        {
            ValidationFailures.Add(1, new KeyValuePair<string, object?>("reason", ValidationErrorCode.InternalError.ToString()));
            _logger.LogError(ex, "Unhandled validation error for {FileName}", fileName);

            return new VeraPdfValidationReport
            {
                FileName = fileName,
                RequestedStandards = standards,
                GeneratedAtUtc = DateTimeOffset.UtcNow,
                Errors =
                [
                    new ValidationError
                    {
                        Code = ValidationErrorCode.InternalError,
                        Message = ex.Message,
                        Target = "validation"
                    }
                ],
                Reports = [],
                Summary = CreateSummary(null, [],
                [
                    new ValidationError
                    {
                        Code = ValidationErrorCode.InternalError,
                        Message = ex.Message,
                        Target = "validation"
                    }
                ])
            };
        }
        finally
        {
            if (throttleEntered)
            {
                _validationThrottle.Release();
            }

            stopwatch.Stop();
            ValidationDurationMs.Record(stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private static ValidationErrorCode ResolveErrorCode(ProcessExecutionResult result)
    {
        if (result.TimedOut)
        {
            return ValidationErrorCode.ProcessTimeout;
        }

        if (result.ExitCode != 0)
        {
            return ValidationErrorCode.ProcessFailed;
        }

        return ValidationErrorCode.None;
    }

    private static VeraPdfCliReport? ParseVeraPdfReport(string rawJsonReport)
    {
        if (string.IsNullOrWhiteSpace(rawJsonReport) || rawJsonReport == "{}")
        {
            return null;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<VeraPdfCliReportEnvelope>(rawJsonReport, VeraPdfJsonSerializerOptions);
            return envelope?.Report;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DescribeVeraPdfExitCode(int exitCode, bool timedOut)
    {
        if (timedOut)
        {
            return "Process execution timed out before veraPDF completed.";
        }

        return exitCode switch
        {
            0 => "All files valid.",
            1 => "Invalid files found.",
            7 => "Failed to parse one or more files.",
            8 => "Some PDFs are encrypted.",
            -1 => "Process failed to start or was terminated before completion.",
            _ => $"veraPDF returned exit code {exitCode}."
        };
    }

    private static string DescribeValidationErrorCode(ValidationErrorCode errorCode)
    {
        return errorCode switch
        {
            ValidationErrorCode.None => "No validation error occurred.",
            ValidationErrorCode.InvalidInput => "Input data or options were invalid.",
            ValidationErrorCode.InvalidPdf => "The document failed parser precheck and external validation was skipped.",
            ValidationErrorCode.RuntimeUnavailable => "Runtime dependencies are unavailable or could not be provisioned.",
            ValidationErrorCode.ProcessTimeout => "The external validator did not complete within the configured timeout.",
            ValidationErrorCode.ProcessFailed => "The external validator reported a non-zero exit code.",
            ValidationErrorCode.InternalError => "An unexpected internal validation error occurred.",
            _ => "Unknown validation error."
        };
    }

    private static VeraPdfValidationReport BuildFailureReport(
        string fileName,
        ValidationStandard standards,
        ValidationErrorCode errorCode,
        string message,
        string target)
    {
        return new VeraPdfValidationReport
        {
            FileName = string.IsNullOrWhiteSpace(fileName) ? "unknown.pdf" : fileName,
            RequestedStandards = standards,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Errors =
            [
                new ValidationError
                {
                    Code = errorCode,
                    Message = message,
                    Target = target
                }
            ],
            Reports = [],
            Summary = CreateSummary(null, [],
            [
                new ValidationError
                {
                    Code = errorCode,
                    Message = message,
                    Target = target
                }
            ])
        };
    }

    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return memoryStream.ToArray();
    }

    private string BuildArguments(ValidationStandard standard, string filePath, ValidationExecutionOptions? executionOptions)
    {
        var standardArgs = standard switch
        {
            ValidationStandard.PdfA => ResolvePdfAArguments(executionOptions),
            ValidationStandard.PdfUa => ResolvePdfUaArguments(executionOptions),
            ValidationStandard.Wcag22 => BuildWcagArguments(executionOptions),
            _ => throw new ArgumentOutOfRangeException(nameof(standard), standard, "Unsupported validation standard.")
        };

        return $"{standardArgs} \"{filePath}\"";
    }

    private string ResolvePdfAArguments(ValidationExecutionOptions? executionOptions)
    {
        var overrideValue = executionOptions?.ProfileOverrides?.PdfAArguments;
        return string.IsNullOrWhiteSpace(overrideValue) ? _options.PdfAArguments : overrideValue;
    }

    private string ResolvePdfUaArguments(ValidationExecutionOptions? executionOptions)
    {
        var overrideValue = executionOptions?.ProfileOverrides?.PdfUaArguments;
        return string.IsNullOrWhiteSpace(overrideValue) ? _options.PdfUaArguments : overrideValue;
    }

    private string BuildWcagArguments(ValidationExecutionOptions? executionOptions)
    {
        var overrideValue = executionOptions?.ProfileOverrides?.Wcag22Arguments;
        var standardArgs = string.IsNullOrWhiteSpace(overrideValue) ? _options.Wcag22Arguments : overrideValue;
        var policyFilePath = executionOptions?.Wcag22PolicyFilePathOverride ?? _options.Wcag22PolicyFilePath;

        if (string.IsNullOrWhiteSpace(policyFilePath))
        {
            return standardArgs;
        }

        if (!File.Exists(policyFilePath))
        {
            throw new FileNotFoundException("Configured WCAG policy file was not found.", policyFilePath);
        }

        if (standardArgs.Contains("--policyfile", StringComparison.OrdinalIgnoreCase))
        {
            return standardArgs;
        }

        return $"{standardArgs} --policyfile \"{policyFilePath}\"";
    }

    private static IReadOnlyDictionary<string, string> BuildEnvironment(string javaHomePath)
    {
        var javaBin = Path.Combine(javaHomePath, "bin");
        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

        return new Dictionary<string, string>
        {
            ["JAVA_HOME"] = javaHomePath,
            ["PATH"] = string.IsNullOrWhiteSpace(currentPath)
                ? javaBin
                : string.Concat(javaBin, Path.PathSeparator, currentPath)
        };
    }

    private static IEnumerable<ValidationStandard> ExpandStandards(ValidationStandard standards)
    {
        if (standards.HasFlag(ValidationStandard.PdfA))
        {
            yield return ValidationStandard.PdfA;
        }

        if (standards.HasFlag(ValidationStandard.PdfUa))
        {
            yield return ValidationStandard.PdfUa;
        }

        if (standards.HasFlag(ValidationStandard.Wcag22))
        {
            yield return ValidationStandard.Wcag22;
        }
    }

    private static string ToName(ValidationStandard standard) => standard switch
    {
        ValidationStandard.PdfA => "PDF-A",
        ValidationStandard.PdfUa => "PDF-UA",
        ValidationStandard.Wcag22 => "WCAG 2.2",
        _ => standard.ToString()
    };

    private static ValidationReportSummary CreateSummary(
        ParserPrecheckReport? parserPrecheck,
        IReadOnlyList<StandardValidationReport> reports,
        IReadOnlyList<ValidationError> errors)
    {
        var failedStandards = reports.Where(r => !r.Passed).Select(r => r.Standard).ToArray();

        return new ValidationReportSummary
        {
            TotalStandardsChecked = reports.Count,
            PassedStandards = reports.Count(r => r.Passed),
            FailedStandards = reports.Count(r => !r.Passed),
            HasParserErrors = parserPrecheck is not null && !parserPrecheck.Success,
            FailedStandardsList = failedStandards
        };
    }
}
