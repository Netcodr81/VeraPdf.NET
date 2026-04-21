using System.Text.Json;

namespace VeraPdf.NET.Validation.Tests.Parity;

internal static class ParityBaselineFixtureLoader
{
    public static ParityBaselineManifest LoadManifest(string manifestAbsolutePath)
    {
        if (string.IsNullOrWhiteSpace(manifestAbsolutePath))
            throw new ArgumentException("Manifest path is required.", nameof(manifestAbsolutePath));

        if (!Path.IsPathRooted(manifestAbsolutePath))
            throw new ArgumentException("Manifest path must be absolute.", nameof(manifestAbsolutePath));

        if (!File.Exists(manifestAbsolutePath))
            throw new FileNotFoundException("Parity baseline manifest file was not found.", manifestAbsolutePath);

        using var stream = File.OpenRead(manifestAbsolutePath);

        var manifest = JsonSerializer.Deserialize<ParityBaselineManifest>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        return manifest ?? throw new InvalidOperationException("Parity baseline manifest is empty or invalid.");
    }

    public static string ResolveCasePdfPath(string manifestAbsolutePath, ParityBaselineCase @case)
    {
        return ResolvePathFromManifestDirectory(manifestAbsolutePath, @case.PdfPath);
    }

    public static string ResolveCaseJavaResultPath(string manifestAbsolutePath, ParityBaselineCase @case)
    {
        return ResolvePathFromManifestDirectory(manifestAbsolutePath, @case.JavaResultPath);
    }

    private static string ResolvePathFromManifestDirectory(string manifestAbsolutePath, string pathFromManifest)
    {
        if (string.IsNullOrWhiteSpace(pathFromManifest))
            throw new ArgumentException("Relative path from manifest cannot be empty.", nameof(pathFromManifest));

        if (Path.IsPathRooted(pathFromManifest))
            return pathFromManifest;

        var manifestDirectory = Path.GetDirectoryName(manifestAbsolutePath)
            ?? throw new InvalidOperationException("Unable to resolve manifest directory.");

        return Path.GetFullPath(Path.Combine(manifestDirectory, pathFromManifest));
    }
}
