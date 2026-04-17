using VeraPdf.NET.Validation.Results;

namespace VeraPdf.NET.Validation.Execution;

/// <summary>
/// Thread-safe collector for validation results.
/// 
/// Uses thread-local buffers to avoid contention,
/// then merges at the end.
/// </summary>
internal sealed class RuleResultCollector
{
    private readonly ThreadLocal<List<RuleResult>> _local =
        new(() => new List<RuleResult>(), trackAllValues: true);

    /// <summary>
    /// Adds a result to the current thread's buffer.
    /// </summary>
    public void Add(RuleResult result)
    {
        _local.Value!.Add(result);
    }

    /// <summary>
    /// Merges all thread-local results into a single list.
    /// </summary>
    public List<RuleResult> ToList()
    {
        return _local.Values.SelectMany(x => x).ToList();
    }
}