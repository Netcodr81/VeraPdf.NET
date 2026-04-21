using VeraPdf.NET.Validation;
using VeraPdf.NET.Validation.Models;

namespace VeraPdf.NET.SampleApi.Jobs;

public sealed class ValidationJobRecord
{
    public required string JobId { get; init; }

    public required string FileName { get; init; }

    public required ValidationStandard Standards { get; init; }

    public ValidationExecutionOptions? ExecutionOptions { get; init; }

    public required byte[] PdfBytes { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public ValidationJobStatus Status { get; set; } = ValidationJobStatus.Pending;

    public VeraPdfValidationReport? Report { get; set; }

    public string? FailureMessage { get; set; }
}
