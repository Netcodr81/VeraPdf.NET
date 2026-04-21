using VeraPdf.NET.Validation;

namespace VeraPdf.NET.SampleApi.Jobs;

public sealed class ValidationJobProcessor(
    IValidationJobQueue jobQueue,
    IValidationJobStore jobStore,
    IVeraPdfValidationService validationService,
    ILogger<ValidationJobProcessor> logger) : BackgroundService
{
    private readonly IValidationJobQueue _jobQueue = jobQueue;
    private readonly IValidationJobStore _jobStore = jobStore;
    private readonly IVeraPdfValidationService _validationService = validationService;
    private readonly ILogger<ValidationJobProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string jobId;
            try
            {
                jobId = await _jobQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (!_jobStore.TryGet(jobId, out var record) || record is null)
            {
                continue;
            }

            record.Status = ValidationJobStatus.Running;
            _jobStore.Update(record);

            try
            {
                await using var stream = new MemoryStream(record.PdfBytes, writable: false);
                var report = await _validationService.ValidateAsync(
                    stream,
                    record.FileName,
                    record.Standards,
                    record.ExecutionOptions,
                    stoppingToken);

                record.Report = report;
                record.Status = report.Passed ? ValidationJobStatus.Completed : ValidationJobStatus.Failed;
                record.CompletedAtUtc = DateTimeOffset.UtcNow;
                _jobStore.Update(record);
            }
            catch (Exception ex)
            {
                record.Status = ValidationJobStatus.Failed;
                record.FailureMessage = ex.Message;
                record.CompletedAtUtc = DateTimeOffset.UtcNow;
                _jobStore.Update(record);
                _logger.LogError(ex, "Validation job {JobId} failed", jobId);
            }
        }
    }
}
