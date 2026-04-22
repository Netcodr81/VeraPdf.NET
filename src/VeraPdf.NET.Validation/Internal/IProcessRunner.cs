namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Abstracts external process execution for validator CLI invocations.
/// </summary>
internal interface IProcessRunner
{
    /// <summary>
    /// Executes an external process and captures output, errors, and timeout status.
    /// </summary>
    /// <param name="fileName">The executable file path to run.</param>
    /// <param name="arguments">The command-line arguments supplied to the executable.</param>
    /// <param name="workingDirectory">The working directory for process execution.</param>
    /// <param name="environment">Environment variable overrides applied to the process.</param>
    /// <param name="timeout">The maximum allowed execution duration.</param>
    /// <param name="cancellationToken">Token used to cancel process execution.</param>
    /// <returns>A result object containing process status and captured output streams.</returns>
    Task<ProcessExecutionResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string> environment,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}
