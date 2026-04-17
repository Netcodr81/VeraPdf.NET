using System.Collections.Concurrent;

namespace VeraPdf.NET.Validation.Execution;

/// <summary>
/// Converts object traversal into a partitioned enumerable
/// suitable for Parallel.ForEach.
/// </summary>
internal static class PartitionerHelper
{
    public static OrderablePartitioner<object> Create(IEnumerable<object> source)
    {
        return Partitioner.Create(source, EnumerablePartitionerOptions.NoBuffering);
    }
}