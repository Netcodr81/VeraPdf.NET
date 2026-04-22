using System.Diagnostics;

namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Executes external processes used by validation, including the veraPDF CLI invocation.
/// </summary>
internal sealed class DefaultProcessRunner : IProcessRunner
{
    /// <summary>
    /// Runs a process with redirected output streams, timeout handling, and cancellation support.
    /// </summary>
    /// <param name="fileName">The executable path to start.</param>
    /// <param name="arguments">The command-line arguments for the process.</param>
    /// <param name="workingDirectory">The working directory used for process execution.</param>
    /// <param name="environment">Environment variable overrides required by the process.</param>
    /// <param name="timeout">Maximum execution time before forcible termination.</param>
    /// <param name="cancellationToken">Token used to cancel execution.</param>
    /// <returns>A process execution result containing exit code, output streams, and timeout state.</returns>
    public async Task<ProcessExecutionResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string> environment,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var pair in environment)
        {
            process.StartInfo.Environment[pair.Key] = pair.Value;
        }

        if (!process.Start())
        {
            return new ProcessExecutionResult(-1, string.Empty, "Failed to start process.", false);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                    await process.WaitForExitAsync(CancellationToken.None);
                }
            }
            catch
            {
                // ignored
            }

            return new ProcessExecutionResult(-1, string.Empty, "veraPDF process timed out.", true);
        }

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        return new ProcessExecutionResult(process.ExitCode, stdOut, stdErr, false);
    }
}
