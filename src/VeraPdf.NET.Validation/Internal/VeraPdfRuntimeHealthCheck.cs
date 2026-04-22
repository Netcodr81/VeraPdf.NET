using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VeraPdf.NET.Validation.Internal;

/// <summary>
/// Health check that verifies runtime provisioning can resolve executable veraPDF and Java paths.
/// </summary>
internal sealed class VeraPdfRuntimeHealthCheck(IVeraPdfRuntimeProvisioner runtimeProvisioner) : IHealthCheck
{
    private readonly IVeraPdfRuntimeProvisioner _runtimeProvisioner = runtimeProvisioner;

    /// <summary>
    /// Performs runtime availability checks and returns a health status result.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">Token used to cancel health check execution.</param>
    /// <returns>A healthy result when runtime provisioning succeeds; otherwise unhealthy with exception details.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var runtime = await _runtimeProvisioner.EnsureRuntimeAsync(cancellationToken);
            return HealthCheckResult.Healthy("veraPDF runtime available.", new Dictionary<string, object>
            {
                ["veraPdfExecutable"] = runtime.VeraPdfExecutablePath,
                ["javaHome"] = runtime.JavaHomePath
            });
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("veraPDF runtime unavailable.", ex);
        }
    }
}
