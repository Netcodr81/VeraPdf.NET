namespace VeraPdf.NET.SampleApi.Jobs;

public sealed class ValidationJobStoreOptions
{
    public string Provider { get; set; } = "InMemory";

    public string FileStorePath { get; set; } = Path.Combine(Path.GetTempPath(), "VeraPdf.NET", "jobs");
}
