namespace VeraPdf.NET.Validation.Internal;

internal sealed record ProcessExecutionResult(int ExitCode, string StdOut, string StdErr, bool TimedOut);
