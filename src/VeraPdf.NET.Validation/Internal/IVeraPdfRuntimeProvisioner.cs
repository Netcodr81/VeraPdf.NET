namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Resolves and prepares the runtime dependencies required to execute veraPDF.
/// </summary>
internal interface IVeraPdfRuntimeProvisioner
{
    /// <summary>
    /// Ensures runtime artifacts are available and returns the resolved executable/runtime paths.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel provisioning.</param>
    /// <returns>A resolved runtime descriptor used for process execution.</returns>
    Task<VeraPdfRuntime> EnsureRuntimeAsync(CancellationToken cancellationToken);
}
