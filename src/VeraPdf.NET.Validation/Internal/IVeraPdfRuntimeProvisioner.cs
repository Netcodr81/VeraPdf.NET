namespace VeraPdf.NET.Validation.Internal;

internal interface IVeraPdfRuntimeProvisioner
{
    Task<VeraPdfRuntime> EnsureRuntimeAsync(CancellationToken cancellationToken);
}
