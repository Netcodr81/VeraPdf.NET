namespace VeraPdf.NET.Validation.Abstractions;

/// <summary>
/// Controls validation engine behavior.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>
    /// Enables parallel validation.
    /// </summary>
    public bool EnableParallel { get; init; } = true;

    /// <summary>
    /// Max degree of parallelism.
    /// Defaults to Environment.ProcessorCount.
    /// </summary>
    public int? MaxDegreeOfParallelism { get; init; }

    /// <summary>
    /// Stop validation after first failure (disables parallel benefit).
    /// </summary>
    public bool StopOnFirstFailure { get; init; } = false;
}