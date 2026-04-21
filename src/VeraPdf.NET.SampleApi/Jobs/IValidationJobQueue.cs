namespace VeraPdf.NET.SampleApi.Jobs;

public interface IValidationJobQueue
{
    ValueTask EnqueueAsync(string jobId, CancellationToken cancellationToken = default);

    ValueTask<string> DequeueAsync(CancellationToken cancellationToken);
}
