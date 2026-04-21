namespace VeraPdf.NET.SampleApi.Jobs;

public interface IValidationJobStore
{
    ValidationJobRecord Add(ValidationJobRecord record);

    bool TryGet(string jobId, out ValidationJobRecord? record);

    void Update(ValidationJobRecord record);
}
