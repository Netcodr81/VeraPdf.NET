using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using VeraPdf.Library.Core;

namespace VeraPdf.Library.Tests;

public class ReleaseDetailsTests
{
    [Fact]
    public void PropertiesCreateAndRegisterReleaseDetails()
    {
        const string properties = "verapdf.project.id=verapdf-test\n" +
            "verapdf.release.version=0.0.0-TEST\n" +
            "verapdf.release.date=2011-07-31\n" +
            "verapdf.date.format=yyyy-MM-dd\n";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(properties));

        var details = ReleaseDetails.AddDetailsFromProperties(source);

        Assert.Equal("verapdf-test", details.Id);
        Assert.Equal("0.0.0-TEST", details.Version);
        var localMidnight = new DateTime(2011, 7, 31, 0, 0, 0, DateTimeKind.Local);
        Assert.Equal(new DateTimeOffset(localMidnight), details.BuildDate);
        Assert.Same(details, ReleaseDetails.ById("verapdf-test"));
        Assert.Contains("verapdf-test", ReleaseDetails.GetIds());
        Assert.Contains(details, ReleaseDetails.GetDetails());
    }

    [Fact]
    public void DefaultAndUnknownIdsUseStableFallback()
    {
        Assert.Same(ReleaseDetails.DefaultInstance(), ReleaseDetails.DefaultInstance());
        Assert.Same(ReleaseDetails.DefaultInstance(), ReleaseDetails.ById("missing-release"));
        Assert.Same(ReleaseDetails.DefaultInstance(), ReleaseDetails.ById(null));
    }

    [Fact]
    public void ReadsEmbeddedLibraryReleaseProperties()
    {
        var details = ReleaseDetails.AddDetailsFromResource(ReleaseDetails.LibraryDetailsResource);

        Assert.Equal("VeraPdf.Library.Core", details.Id);
        Assert.Equal("0.1.0-preview", details.Version);
        Assert.Same(details, ReleaseDetails.ById(details.Id));
    }

    [Fact]
    public void EqualityUsesIdVersionAndBuildDate()
    {
        using var firstSource = PropertiesStream("same", "1.2.3", "2024-01-02");
        using var secondSource = PropertiesStream("same", "1.2.3", "2024-01-02");

        var first = ReleaseDetails.AddDetailsFromProperties(firstSource);
        var second = ReleaseDetails.AddDetailsFromProperties(secondSource);

        Assert.NotSame(first, second);
        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void XmlSerializationRoundTripsReleaseDetails()
    {
        using var source = PropertiesStream("xml-test", "2.3.4", "2025-03-04");
        var details = ReleaseDetails.AddDetailsFromProperties(source);
        var serializer = new XmlSerializer(typeof(ReleaseDetails));
        using var writer = new StringWriter();
        serializer.Serialize(writer, details);

        using var reader = new StringReader(writer.ToString());
        var result = Assert.IsType<ReleaseDetails>(serializer.Deserialize(reader));

        Assert.NotSame(details, result);
        Assert.Equal(details, result);
    }

    [Fact]
    public void XmlSerialiserSupportsStringAndStreamAndFragmentOutput()
    {
        using var source = PropertiesStream("serializer-test", "3.4.5", "2025-03-04");
        var details = ReleaseDetails.AddDetailsFromProperties(source);
        var xml = XmlSerialiser.ToXml(details, format: true, fragment: true);
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var fromString = XmlSerialiser.TypeFromXml<ReleaseDetails>(xml);
        var fromStream = XmlSerialiser.TypeFromXml<ReleaseDetails>(input);
        using var output = new MemoryStream();
        XmlSerialiser.ToXml(details, output, format: false, fragment: false);

        Assert.DoesNotContain("<?xml", xml, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(details, fromString);
        Assert.Equal(details, fromStream);
        Assert.True(output.Length > 0);
    }

    [Fact]
    public void XmlSerialiserRejectsDtdInput()
    {
        const string xml = "<!DOCTYPE releaseDetails [<!ENTITY x 'unsafe'>]>" +
            "<releaseDetails id='x' version='1' buildDate='2025-03-04T00:00:00Z'/>";

        var exception = Assert.Throws<InvalidOperationException>(
            () => XmlSerialiser.TypeFromXml<ReleaseDetails>(xml));
        Assert.IsType<XmlException>(exception.InnerException);
    }

    [Fact]
    public void XmlSerialiserSupportsTextReadersWritersAndFiles()
    {
        using var source = PropertiesStream("file-test", "4.5.6", "2025-03-04");
        var details = ReleaseDetails.AddDetailsFromProperties(source);
        var xml = XmlSerialiser.ToXml(details, format: false, fragment: true);
        using var textReader = new StringReader(xml);
        var fromReader = XmlSerialiser.TypeFromXml<ReleaseDetails>(textReader);
        Assert.Equal(-1, textReader.Peek());

        using var textWriter = new StringWriter();
        XmlSerialiser.ToXml(details, textWriter, format: true, fragment: true);
        textWriter.Write("!");
        Assert.EndsWith("!", textWriter.ToString());

        var path = Path.GetTempFileName();
        try
        {
            var file = new FileInfo(path);
            XmlSerialiser.ToXml(details, file, format: true, fragment: false);
            var fromFile = XmlSerialiser.TypeFromXml<ReleaseDetails>(file);
            Assert.Equal(details, fromReader);
            Assert.Equal(details, fromFile);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlSerialiserRoundTripsCollectionWithCustomRoot()
    {
        using var firstSource = PropertiesStream("collection-one", "1.0.0", "2025-01-02");
        using var secondSource = PropertiesStream("collection-two", "2.0.0", "2025-02-03");
        var first = ReleaseDetails.AddDetailsFromProperties(firstSource);
        var second = ReleaseDetails.AddDetailsFromProperties(secondSource);
        var xml = XmlSerialiser.ToXml("releaseDetailsList", new[] { first, second }, format: false, fragment: true);
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        var fromString = XmlSerialiser.CollectionFromXml<ReleaseDetails>(xml);
        var fromStream = XmlSerialiser.CollectionFromXml<ReleaseDetails>(input);

        Assert.Equal(new[] { first, second }, fromString);
        Assert.Equal(new[] { first, second }, fromStream);
        Assert.Contains("<releaseDetailsList>", xml, StringComparison.Ordinal);
    }

    [Fact]
    public void XmlSerialiserSupportsDerivedCollectionItemsWhenKnownTypesAreProvided()
    {
        CollectionItemBase[] items = [new CollectionItemBase { Name = "base" },
            new DerivedCollectionItem { Name = "derived", Detail = "extra" }];
        var xml = XmlSerialiser.ToXml("items", items, format: false, fragment: true);

        var result = XmlSerialiser.CollectionFromXml<CollectionItemBase>(xml, typeof(DerivedCollectionItem));

        Assert.Equal(2, result.Count);
        Assert.Equal("base", result[0].Name);
        var derived = Assert.IsType<DerivedCollectionItem>(result[1]);
        Assert.Equal("derived", derived.Name);
        Assert.Equal("extra", derived.Detail);
    }

    [Fact]
    public void XmlSerialiserReadsEmptyCollection()
    {
        Assert.Empty(XmlSerialiser.CollectionFromXml<ReleaseDetails>("<releases/>"));
    }

    [Fact]
    public void XmlSerialiserSupportsCollectionReadersWritersAndFiles()
    {
        using var source = PropertiesStream("collection-file", "5.6.7", "2025-03-04");
        var details = ReleaseDetails.AddDetailsFromProperties(source);
        var xml = XmlSerialiser.ToXml("releases", new[] { details }, format: false, fragment: true);
        using var textReader = new StringReader(xml);
        var fromReader = XmlSerialiser.CollectionFromXml<ReleaseDetails>(textReader);
        Assert.Equal(-1, textReader.Peek());

        using var textWriter = new StringWriter();
        XmlSerialiser.ToXml("releases", new[] { details }, textWriter, format: true, fragment: true);
        textWriter.Write("!");
        Assert.EndsWith("!", textWriter.ToString());

        var path = Path.GetTempFileName();
        try
        {
            var file = new FileInfo(path);
            XmlSerialiser.ToXml("releases", new[] { details }, file, format: true, fragment: false);
            var fromFile = XmlSerialiser.CollectionFromXml<ReleaseDetails>(file);
            Assert.Equal(new[] { details }, fromReader);
            Assert.Equal(new[] { details }, fromFile);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void XmlSerialiserGeneratesSchemaForAnObject()
    {
        var schema = XmlSerialiser.Schema(new ReleaseDetails());
        using var reader = XmlReader.Create(new StringReader(schema));
        var document = XDocument.Load(reader);

        Assert.Equal("schema", document.Root?.Name.LocalName);
        Assert.Contains(document.Root!.Elements(), element =>
            element.Name.LocalName == "element" && (string?)element.Attribute("name") == "releaseDetails");
    }

    [Fact]
    public void XmlSerialiserGeneratesSeparateSchemasForMultipleNamespaces()
    {
        var schemas = XmlSerialiser.SchemaDocuments(new MultiNamespaceSchemaRoot());

        Assert.Contains("urn:release:root", schemas.Keys);
        Assert.Contains("urn:release:child", schemas.Keys);
        Assert.All(schemas.Values, schema =>
        {
            using var reader = XmlReader.Create(new StringReader(schema));
            Assert.Equal("schema", XDocument.Load(reader).Root?.Name.LocalName);
        });
    }

    [Fact]
    public void RejectsMissingPropertiesAndInvalidDates()
    {
        using var missing = new MemoryStream(Encoding.UTF8.GetBytes("verapdf.project.id=missing\n"));
        using var invalidDate = PropertiesStream("invalid-date", "1", "not-a-date");

        Assert.Throws<FormatException>(() => ReleaseDetails.AddDetailsFromProperties(missing));
        Assert.Throws<FormatException>(() => ReleaseDetails.AddDetailsFromProperties(invalidDate));
    }

    private static MemoryStream PropertiesStream(string id, string version, string date) =>
        new(Encoding.UTF8.GetBytes(
            $"verapdf.project.id={id}\n" +
            $"verapdf.release.version={version}\n" +
            $"verapdf.release.date={date}\n" +
            "verapdf.date.format=yyyy-MM-dd\n"));
}

[XmlRoot("release", Namespace = "urn:release:root")]
public sealed class MultiNamespaceSchemaRoot
{
    [XmlElement("metadata", Namespace = "urn:release:child")]
    public MultiNamespaceSchemaChild Metadata { get; set; } = new();
}

[XmlType(Namespace = "urn:release:child")]
public sealed class MultiNamespaceSchemaChild
{
    public string Name { get; set; } = string.Empty;
}

[XmlRoot("baseItem")]
public class CollectionItemBase
{
    public string Name { get; set; } = string.Empty;
}

[XmlRoot("derivedItem")]
public sealed class DerivedCollectionItem : CollectionItemBase
{
    public string Detail { get; set; } = string.Empty;
}
