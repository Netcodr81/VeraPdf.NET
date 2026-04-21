using System.IO.Compression;
using Microsoft.Extensions.Options;
using VeraPdf.NET.Validation.Configuration;

namespace VeraPdf.NET.Validation.Internal;

internal sealed class VeraPdfRuntimeProvisioner(IOptions<VeraPdfRuntimeOptions> options) : IVeraPdfRuntimeProvisioner
{
    private readonly VeraPdfRuntimeOptions _options = options.Value;
    private readonly SemaphoreSlim _sync = new(1, 1);
    private VeraPdfRuntime? _cached;

    public async Task<VeraPdfRuntime> EnsureRuntimeAsync(CancellationToken cancellationToken)
    {
        if (_cached is not null)
        {
            return _cached;
        }

        await _sync.WaitAsync(cancellationToken);
        try
        {
            if (_cached is not null)
            {
                return _cached;
            }

            var runtimeRoot = _options.RuntimeRootPath;
            var javaRoot = Path.Combine(runtimeRoot, "java");
            var veraRoot = Path.Combine(runtimeRoot, "verapdf");

            ExtractIfNeeded(_options.JavaZipPath, javaRoot, _options.JavaZipSha256);
            ExtractIfNeeded(_options.VeraPdfZipPath, veraRoot, _options.VeraPdfZipSha256);

            var javaHome = ResolveJavaHome(javaRoot);
            var veraExecutable = ResolveVeraPdfExecutable(veraRoot);
            EnsureExecutablePermissions(veraExecutable);

            _cached = new VeraPdfRuntime(veraExecutable, javaHome, Path.GetDirectoryName(veraExecutable) ?? runtimeRoot);
            return _cached;
        }
        finally
        {
            _sync.Release();
        }
    }

    private static void ExtractIfNeeded(string zipPath, string destination, string? expectedSha256)
    {
        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException("Required runtime archive was not found.", zipPath);
        }

        VerifyArchiveChecksum(zipPath, expectedSha256);

        var markerFile = Path.Combine(destination, ".extracted");
        if (File.Exists(markerFile))
        {
            return;
        }

        if (Directory.Exists(destination))
        {
            Directory.Delete(destination, recursive: true);
        }

        Directory.CreateDirectory(destination);
        ZipFile.ExtractToDirectory(zipPath, destination, overwriteFiles: true);
        File.WriteAllText(markerFile, DateTimeOffset.UtcNow.ToString("O"));
    }

    private static void VerifyArchiveChecksum(string archivePath, string? expectedSha256)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256))
        {
            return;
        }

        using var stream = File.OpenRead(archivePath);
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(stream));

        if (!hash.Equals(expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Archive checksum mismatch for '{archivePath}'.");
        }
    }

    private static string ResolveJavaHome(string javaRoot)
    {
        var binDirs = Directory.Exists(javaRoot)
            ? Directory.GetDirectories(javaRoot, "bin", SearchOption.AllDirectories)
            : [];

        var javaBin = binDirs.FirstOrDefault();
        if (javaBin is null)
        {
            throw new DirectoryNotFoundException($"Unable to locate Java bin directory under '{javaRoot}'.");
        }

        return Directory.GetParent(javaBin)?.FullName
               ?? throw new DirectoryNotFoundException("Unable to resolve Java home path.");
    }

    private static string ResolveVeraPdfExecutable(string veraRoot)
    {
        if (!Directory.Exists(veraRoot))
        {
            throw new DirectoryNotFoundException($"veraPDF runtime directory '{veraRoot}' does not exist.");
        }

        var candidateNames = OperatingSystem.IsWindows()
            ? new[] { "verapdf.bat", "verapdf.exe" }
            : new[] { "verapdf", "verapdf.sh" };

        foreach (var candidateName in candidateNames)
        {
            var candidatePath = Directory.GetFiles(veraRoot, candidateName, SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(candidatePath))
            {
                return candidatePath;
            }
        }

        throw new FileNotFoundException($"Unable to locate a veraPDF executable in '{veraRoot}'.");
    }

    private static void EnsureExecutablePermissions(string filePath)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var currentMode = File.GetUnixFileMode(filePath);
            var executeMask = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
            File.SetUnixFileMode(filePath, currentMode | executeMask);
        }
        catch (PlatformNotSupportedException)
        {
            // ignored for unsupported platforms
        }
        catch (UnauthorizedAccessException)
        {
            // ignored if host disallows chmod; process start will surface actionable error later
        }
    }
}
