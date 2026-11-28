using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VeraPdf.Library.Core;

[XmlRoot("releaseDetails")]
public sealed class ReleaseDetails : IEquatable<ReleaseDetails>, IXmlSerializable
{
    public const string ApplicationPropertiesRoot = "org/verapdf/release/";
    public const string PropertiesExtension = "properties";
    public const string LibraryDetailsResource = ApplicationPropertiesRoot + "library." + PropertiesExtension;

    private const string RawDateFormat = "${maven.build.timestamp.format}";
    private const string VersionProperty = "verapdf.release.version";
    private const string DateProperty = "verapdf.release.date";
    private const string DateFormatProperty = "verapdf.date.format";
    private const string IdProperty = "verapdf.project.id";
    private const string Rights = "Developed and released by the veraPDF Consortium.\n" +
        "Funded by the PREFORMA project.\n" +
        "Released under the GNU General Public License v3\n" +
        "and the Mozilla Public License v2 or later.\n";

    private static readonly object RegistryLock = new();
    private static readonly ReleaseDetails Default = new("name", "version", DateTimeOffset.UtcNow);
    private static readonly Dictionary<string, ReleaseDetails> RegisteredDetails = CreateInitialDetails();

    public ReleaseDetails() : this("name", "version", DateTimeOffset.UtcNow)
    {
    }

    private ReleaseDetails(string id, string version, DateTimeOffset buildDate)
    {
        Id = id;
        Version = version;
        BuildDate = buildDate;
    }

    [XmlIgnore]
    public string Id { get; private set; }

    [XmlIgnore]
    public string Version { get; private set; }

    [XmlIgnore]
    public DateTimeOffset BuildDate { get; private set; }

    public static ReleaseDetails DefaultInstance() => Default;

    public static string RightsStatement() => Rights;

    public static ReleaseDetails GetInstance()
    {
        lock (RegistryLock)
        {
            return RegisteredDetails.Values.FirstOrDefault() ?? Default;
        }
    }

    public static IReadOnlyCollection<string> GetIds()
    {
        lock (RegistryLock)
        {
            return Array.AsReadOnly(RegisteredDetails.Keys.ToArray());
        }
    }

    public static ReleaseDetails ById(string? id)
    {
        if (id is null)
        {
            return Default;
        }

        lock (RegistryLock)
        {
            return RegisteredDetails.TryGetValue(id, out var details) ? details : Default;
        }
    }

    public static IReadOnlyCollection<ReleaseDetails> GetDetails()
    {
        lock (RegistryLock)
        {
            return Array.AsReadOnly(RegisteredDetails.Values.ToArray());
        }
    }

    public static ReleaseDetails AddDetailsFromResource(string resourceName)
    {
        ArgumentException.ThrowIfNullOrEmpty(resourceName);
        var manifestName = resourceName.Replace('/', '.').Replace('\\', '.');
        using var stream = typeof(ReleaseDetails).Assembly.GetManifestResourceStream(manifestName);
        return stream is null ? Default : AddDetailsFromProperties(stream);
    }

    public static ReleaseDetails AddDetailsFromProperties(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = new StreamReader(source, leaveOpen: true);
        var properties = new Dictionary<string, string>(StringComparer.Ordinal);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed[0] is '#' or '!')
            {
                continue;
            }

            var separator = trimmed.IndexOfAny(['=', ':']);
            if (separator <= 0)
            {
                throw new FormatException($"Invalid release properties line: {line}");
            }

            properties[trimmed[..separator].Trim()] = trimmed[(separator + 1)..].Trim();
        }

        var id = RequiredProperty(properties, IdProperty);
        var version = RequiredProperty(properties, VersionProperty);
        var dateFormat = RequiredProperty(properties, DateFormatProperty);
        var buildDate = dateFormat == RawDateFormat
            ? DateTimeOffset.UtcNow
            : ParseBuildDate(RequiredProperty(properties, DateProperty), dateFormat);
        var details = new ReleaseDetails(id, version, buildDate);

        lock (RegistryLock)
        {
            RegisteredDetails[id] = details;
        }

        return details;
    }

    public bool Equals(ReleaseDetails? other) =>
        other is not null && Id == other.Id && Version == other.Version && BuildDate.Equals(other.BuildDate);

    public override bool Equals(object? obj) => obj is ReleaseDetails other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Id, Version, BuildDate);

    public override string ToString() =>
        $"ReleaseDetails [id={Id}, version={Version}, buildDate={BuildDate:O}]";

    public XmlSchema? GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        reader.MoveToContent();
        var id = reader.GetAttribute("id") ?? throw new XmlException("Missing release id attribute.");
        var version = reader.GetAttribute("version") ?? throw new XmlException("Missing release version attribute.");
        var buildDateText = reader.GetAttribute("buildDate") ??
            throw new XmlException("Missing release buildDate attribute.");
        var buildDate = DateTimeOffset.Parse(buildDateText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (reader.IsEmptyElement)
        {
            reader.Read();
        }
        else
        {
            reader.ReadStartElement();
            reader.MoveToContent();
            reader.ReadEndElement();
        }

        Id = id;
        Version = version;
        BuildDate = buildDate;
    }

    public void WriteXml(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteAttributeString("id", Id);
        writer.WriteAttributeString("version", Version);
        writer.WriteAttributeString("buildDate", BuildDate.ToString("O", CultureInfo.InvariantCulture));
    }

    private static Dictionary<string, ReleaseDetails> CreateInitialDetails()
    {
        var assembly = typeof(ReleaseDetails).Assembly;
        var id = assembly.GetName().Name ?? "VeraPdf.Library";
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "version";
        return new Dictionary<string, ReleaseDetails>(StringComparer.Ordinal)
        {
            [id] = new ReleaseDetails(id, version, DateTimeOffset.UtcNow)
        };
    }

    private static string RequiredProperty(IReadOnlyDictionary<string, string> properties, string key) =>
        properties.TryGetValue(key, out var value)
            ? value
            : throw new FormatException($"Missing required release property '{key}'.");

    private static DateTimeOffset ParseBuildDate(string value, string format)
    {
        var parsed = DateTime.ParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
        return new DateTimeOffset(DateTime.SpecifyKind(parsed, DateTimeKind.Local));
    }
}
