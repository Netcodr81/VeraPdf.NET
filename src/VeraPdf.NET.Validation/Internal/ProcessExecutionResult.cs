namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Captures the outcome of an external process execution.
/// </summary>
/// <param name="ExitCode">The process exit code, or -1 when process startup/timing failed.</param>
/// <param name="StdOut">Captured standard output text.</param>
/// <param name="StdErr">Captured standard error text.</param>
/// <param name="TimedOut">Indicates whether execution exceeded the configured timeout.</param>
internal sealed record ProcessExecutionResult(int ExitCode, string StdOut, string StdErr, bool TimedOut);
