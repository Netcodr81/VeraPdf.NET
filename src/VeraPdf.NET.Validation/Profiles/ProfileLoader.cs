using System.Text.Json;
using VeraPdf.NET.Validation.Infrastructure;
using VeraPdf.NET.Validation.Profiles.Converters;

namespace VeraPdf.NET.Validation.Profiles;

public static class ProfileLoader
{
    /// <summary>
    /// Loads profile from JSON.
    /// </summary>
    public static ValidationProfile LoadJson(string json)
    {
        var profile = JsonSerializer.Deserialize<ValidationProfile>(json)!;

        ResolveTypes(profile);

        return profile;
    }

    /// <summary>
    /// Loads and converts veraPDF XML profile directly.
    /// </summary>
    public static ValidationProfile LoadFromXml(string xml)
    {
        var profile = VeraPdfXmlProfileConverter.Convert(xml);

        ResolveTypes(profile);

        return profile;
    }

    /// <summary>
    /// Resolves all rule TargetTypeName → CLR Type.
    /// </summary>
    private static void ResolveTypes(ValidationProfile profile)
    {
        foreach (var rule in profile.Rules)
        {
            rule.TargetType = TypeMapper.MapType(rule.TargetTypeName);
        }
    }
}