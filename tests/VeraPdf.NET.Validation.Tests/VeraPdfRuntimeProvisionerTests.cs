using Microsoft.Extensions.Options;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;

namespace VeraPdf.NET.Validation.Tests;

public class VeraPdfRuntimeProvisionerTests
{
    [Fact]
    public async Task EnsureRuntimeAsync_Should_Fail_When_JavaChecksum_Does_Not_Match()
    {
        var rootPath = FindRepositoryRoot();
        var validationProjectPath = Path.Combine(rootPath, "src", "VeraPdf.NET.Validation");
        var runtimeRootPath = Path.Combine(Path.GetTempPath(), "VeraPdf.NET", "checksum-tests", Guid.NewGuid().ToString("N"));

        try
        {
            var options = Options.Create(new VeraPdfRuntimeOptions
            {
                VeraPdfZipPath = Path.Combine(validationProjectPath, "veraPDF.zip"),
                JavaZipPath = Path.Combine(validationProjectPath, "Java.zip"),
                JavaZipSha256 = "deadbeef",
                RuntimeRootPath = runtimeRootPath
            });

            var provisioner = new VeraPdfRuntimeProvisioner(options);

            await Assert.ThrowsAsync<InvalidDataException>(() => provisioner.EnsureRuntimeAsync(CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(runtimeRootPath))
            {
                Directory.Delete(runtimeRootPath, recursive: true);
            }
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, "VeraPdfNet.slnx");
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test execution directory.");
    }
}
