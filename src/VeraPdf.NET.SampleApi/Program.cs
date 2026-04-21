using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using VeraPdf.NET.SampleApi.Jobs;
using VeraPdf.NET.Validation;
using VeraPdf.NET.Validation.Models;

const long MaxPdfSizeBytes = 25 * 1024 * 1024;
var apiTimeout = TimeSpan.FromMinutes(2);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxPdfSizeBytes;
});

builder.Services.Configure<ValidationJobStoreOptions>(builder.Configuration.GetSection("ValidationJobs"));

builder.Services.AddOpenApi();
builder.Services.AddVeraPdfValidation(builder.Configuration);
builder.Services.AddVeraPdfValidationHealthChecks();
builder.Services.AddSingleton<IValidationJobStore>(sp =>
{
    var options = sp.GetRequiredService<IOptions<ValidationJobStoreOptions>>().Value;
    return options.Provider.Equals("File", StringComparison.OrdinalIgnoreCase)
        ? new FileValidationJobStore(options.FileStorePath)
        : new InMemoryValidationJobStore();
});
builder.Services.AddSingleton<IValidationJobQueue, ValidationJobQueue>();
builder.Services.AddHostedService<ValidationJobProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");

app.MapPost("/api/validation", async (
    [FromForm] IFormFile pdf,
    [FromForm] bool pdfA,
    [FromForm] bool pdfUa,
    [FromForm] bool wcag22,
    [FromForm] string? pdfAArgs,
    [FromForm] string? pdfUaArgs,
    [FromForm] string? wcag22Args,
    [FromForm] string? wcag22PolicyPath,
    IVeraPdfValidationService validationService,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    var logger = loggerFactory.CreateLogger("ValidationEndpoint");

    if (!TryValidatePdf(pdf, out var validationError))
    {
        return validationError!;
    }

    var standards = ResolveStandards(pdfA, pdfUa, wcag22);
    var executionOptions = BuildExecutionOptions(pdfAArgs, pdfUaArgs, wcag22Args, wcag22PolicyPath);

    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(apiTimeout);

        await using var stream = pdf.OpenReadStream();
        var report = await validationService.ValidateAsync(
            stream,
            pdf.FileName,
            standards,
            executionOptions,
            timeoutCts.Token);

        if (report.Errors.Count > 0)
        {
            var firstError = report.Errors[0];
            logger.LogWarning("Validation request for {FileName} returned error code {ErrorCode}", pdf.FileName, firstError.Code);
            return firstError.Code == ValidationErrorCode.InvalidPdf
                ? Results.UnprocessableEntity(report)
                : CreateProblem(
                    StatusCodes.Status500InternalServerError,
                    firstError.Code.ToString().ToUpperInvariant(),
                    firstError.Message,
                    firstError.Target);
        }

        return Results.Ok(report);
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        logger.LogWarning("Validation timed out for {FileName}", pdf.FileName);
        return CreateProblem(StatusCodes.Status504GatewayTimeout, "PROCESS_TIMEOUT", "Validation timed out.", "validation");
    }
})
.DisableAntiforgery();

app.Run();

static ValidationExecutionOptions? BuildExecutionOptions(
    string? pdfAArgs,
    string? pdfUaArgs,
    string? wcag22Args,
    string? wcag22PolicyPath)
{
    var hasProfileOverrides = !string.IsNullOrWhiteSpace(pdfAArgs)
                              || !string.IsNullOrWhiteSpace(pdfUaArgs)
                              || !string.IsNullOrWhiteSpace(wcag22Args);

    var hasPolicyOverride = !string.IsNullOrWhiteSpace(wcag22PolicyPath);
    if (!hasProfileOverrides && !hasPolicyOverride)
    {
        return null;
    }

    return new ValidationExecutionOptions
    {
        Wcag22PolicyFilePathOverride = hasPolicyOverride ? wcag22PolicyPath : null,
        ProfileOverrides = hasProfileOverrides
            ? new ValidationProfileOverrides
            {
                PdfAArguments = pdfAArgs,
                PdfUaArguments = pdfUaArgs,
                Wcag22Arguments = wcag22Args
            }
            : null
    };
}

static ValidationStandard ResolveStandards(bool pdfA, bool pdfUa, bool wcag22)
{
    var standards = ValidationStandard.None;
    if (pdfA) standards |= ValidationStandard.PdfA;
    if (pdfUa) standards |= ValidationStandard.PdfUa;
    if (wcag22) standards |= ValidationStandard.Wcag22;

    return standards == ValidationStandard.None ? ValidationStandard.All : standards;
}

static bool TryValidatePdf(IFormFile? pdf, out IResult? result)
{
    if (pdf is null || pdf.Length == 0)
    {
        result = CreateProblem(StatusCodes.Status400BadRequest, "INVALID_INPUT", "A PDF file is required.", "pdf");
        return false;
    }

    if (pdf.Length > MaxPdfSizeBytes)
    {
        result = CreateProblem(StatusCodes.Status413PayloadTooLarge, "INVALID_INPUT", $"PDF exceeds max upload size of {MaxPdfSizeBytes} bytes.", "pdf");
        return false;
    }

    if (!IsSupportedContentType(pdf.ContentType))
    {
        result = CreateProblem(StatusCodes.Status415UnsupportedMediaType, "INVALID_INPUT", "Only PDF content types are supported.", "pdf.contentType");
        return false;
    }

    result = null;
    return true;
}

static bool IsSupportedContentType(string? contentType)
{
    if (string.IsNullOrWhiteSpace(contentType))
    {
        return false;
    }

    return contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
           || contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
}

static IResult CreateProblem(int status, string code, string detail, string? target)
{
    return Results.Problem(
        statusCode: status,
        title: code,
        detail: detail,
        extensions: new Dictionary<string, object?>
        {
            ["code"] = code,
            ["target"] = target
        });
}

public partial class Program;
