using System.Collections.Concurrent;

namespace VeraPdf.NET.SampleApi.Jobs;

public sealed class InMemoryValidationJobStore : IValidationJobStore
{
    private readonly ConcurrentDictionary<string, ValidationJobRecord> _jobs = new(StringComparer.OrdinalIgnoreCase);

    public ValidationJobRecord Add(ValidationJobRecord record)
    {
        _jobs[record.JobId] = record;
        return record;
    }

    public bool TryGet(string jobId, out ValidationJobRecord? record)
    {
        var found = _jobs.TryGetValue(jobId, out var value);
        record = value;
        return found;
    }

    public void Update(ValidationJobRecord record)
    {
        _jobs[record.JobId] = record;
    }
}
