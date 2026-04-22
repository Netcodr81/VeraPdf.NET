namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Represents resolved runtime locations used to launch veraPDF with the correct Java environment.
/// </summary>
/// <param name="VeraPdfExecutablePath">Path to the resolved veraPDF executable/script.</param>
/// <param name="JavaHomePath">Path to the resolved Java home directory.</param>
/// <param name="WorkingDirectory">Working directory to use for process execution.</param>
internal sealed record VeraPdfRuntime(string VeraPdfExecutablePath, string JavaHomePath, string WorkingDirectory);
