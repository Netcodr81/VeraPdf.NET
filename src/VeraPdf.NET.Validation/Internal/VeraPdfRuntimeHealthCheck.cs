using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace VeraPdf.NET.Validation.Internal;

internal sealed class VeraPdfRuntimeHealthCheck(IVeraPdfRuntimeProvisioner runtimeProvisioner) : IHealthCheck
{
    private readonly IVeraPdfRuntimeProvisioner _runtimeProvisioner = runtimeProvisioner;

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
