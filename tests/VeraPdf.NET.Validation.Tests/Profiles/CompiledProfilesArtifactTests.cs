using System.Text.Json;
using Shouldly;
using VeraPdf.NET.ValidationProfiles;

namespace VeraPdf.NET.Validation.Tests.Profiles;

public class CompiledProfilesArtifactTests
{
    private const string ResourceName = "VeraPdf.NET.ValidationProfiles.verapdf-profiles.compiled.json";

    [Fact]
    public void Compiled_Profiles_Artifact_Should_Be_Embedded()
    {
        var assembly = typeof(ProfileArtifactMarker).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName);

        stream.ShouldNotBeNull();
    }

    [Fact]
    public void Compiled_Profiles_Artifact_Should_Contain_Profile_Entries()
    {
        var assembly = typeof(ProfileArtifactMarker).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName);
        stream.ShouldNotBeNull();

        var manifest = JsonSerializer.Deserialize<CompiledManifest>(
            stream!,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        manifest.ShouldNotBeNull();
        manifest!.Profiles.Count.ShouldBeGreaterThan(0);
        manifest.Profiles[0].Id.ShouldNotBeNullOrWhiteSpace();
        manifest.Profiles[0].Xml.ShouldContain("<profile");
    }

    private sealed class CompiledManifest
    {
        public IReadOnlyList<CompiledProfile> Profiles { get; init; } = [];
    }

    private sealed class CompiledProfile
    {
        public string Id { get; init; } = string.Empty;
        public string Xml { get; init; } = string.Empty;
    }
}
