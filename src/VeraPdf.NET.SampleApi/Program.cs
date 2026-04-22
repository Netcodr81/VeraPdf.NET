using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

using VeraPdf.NET.Validation;
using VeraPdf.NET.Validation.Models;

const long MaxPdfSizeBytes = 25 * 1024 * 1024;
var apiTimeout = TimeSpan.FromMinutes(2);

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxPdfSizeBytes;
});

builder.Services.AddOpenApi();
builder.Services.AddVeraPDFNet();
builder.Services.AddVeraPdfValidationHealthChecks();
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
    IVeraPdfValidationService validationService,
    ILoggerFactory loggerFactory,
    [FromForm] bool usePdfAValidation = false,
    [FromForm] bool usePdfUAValidation = false,
    [FromForm] bool useWcag22Validation = false,
    CancellationToken cancellationToken = default) =>
{
    var logger = loggerFactory.CreateLogger("ValidationEndpoint");

    var inputValidation = ValidationRequestHelpers.ValidatePdfInput(pdf?.Length, pdf?.ContentType, MaxPdfSizeBytes);
    if (!inputValidation.IsValid)
    {
        var error = inputValidation.Error!;
        return CreateProblem(
            inputValidation.StatusCode ?? StatusCodes.Status400BadRequest,
            error.Code.ToString().ToUpperInvariant(),
            error.Message,
            error.Target);
    }

    var standards = ValidationRequestHelpers.ResolveStandards(usePdfAValidation, usePdfUAValidation, useWcag22Validation);
    var executionOptions = (
        PdfAArgs: (string?)null,
        PdfUaArgs: (string?)null,
        Wcag22Args: (string?)null,
        Wcag22PolicyPath: (string?)null).ToValidationExecutionOptions();

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
