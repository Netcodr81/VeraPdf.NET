using System.Collections.Concurrent;
using System.Text.Json;

namespace VeraPdf.NET.SampleApi.Jobs;

public sealed class FileValidationJobStore : IValidationJobStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly string _rootPath;
    private readonly ConcurrentDictionary<string, object> _locks = new(StringComparer.OrdinalIgnoreCase);

    public FileValidationJobStore(string rootPath)
    {
        _rootPath = rootPath;
        Directory.CreateDirectory(_rootPath);
    }

    public ValidationJobRecord Add(ValidationJobRecord record)
    {
        Write(record);
        return record;
    }

    public bool TryGet(string jobId, out ValidationJobRecord? record)
    {
        var path = GetPath(jobId);
        if (!File.Exists(path))
        {
            record = null;
            return false;
        }

        var json = File.ReadAllText(path);
        record = JsonSerializer.Deserialize<ValidationJobRecord>(json, JsonOptions);
        return record is not null;
    }

    public void Update(ValidationJobRecord record)
    {
        Write(record);
    }

    private void Write(ValidationJobRecord record)
    {
        var gate = _locks.GetOrAdd(record.JobId, _ => new object());
        lock (gate)
        {
            var path = GetPath(record.JobId);
            var json = JsonSerializer.Serialize(record, JsonOptions);
            File.WriteAllText(path, json);
        }
    }

    private string GetPath(string jobId)
        => Path.Combine(_rootPath, $"{jobId}.json");
}
