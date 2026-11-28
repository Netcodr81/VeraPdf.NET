using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpUriMutationTests
{
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Fact]
    public void CreatesAndRoundTripsUrisAtSeveralPaths()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure)
            .WithPropertyString(registry, Xmp, "Root/xmp:Name", "name")
            .WithAppendedArrayItemString(registry, Xmp, "Items", "one", XmpNodeKind.OrderedArray);
        var result = packet.WithPropertyUri(registry, Xmp, "Link", "urn:test?a=1&b=2")
            .WithPropertyUri(registry, Xmp, "Root/xmp:Link", "relative/path")
            .WithPropertyUri(registry, Xmp, "Root/xmp:Name/?xmp:Source", "urn:source")
            .WithPropertyUri(registry, Xmp, "Items[1]", "urn:item");
        Assert.Null(packet.GetProperty(Xmp, "Link"));
        Assert.Equal(XmpNodeKind.Literal, packet.GetArrayItem(registry, Xmp, "Items", 1)!.Kind);
        Assert.Equal(XmpNodeKind.Uri, result.GetArrayItem(registry, Xmp, "Items", 1)!.Kind);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        AssertUri(roundTrip, registry, "Link", "urn:test?a=1&b=2");
        AssertUri(roundTrip, registry, "Root/xmp:Link", "relative/path");
        AssertUri(roundTrip, registry, "Root/xmp:Name/?xmp:Source", "urn:source");
        AssertUri(roundTrip, registry, "Items[1]", "urn:item");
    }

    [Fact]
    public void PreservesQualifiersWhenConvertingAndChangingExistingUri()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithPropertyString(registry, Xmp, "Link", "urn:old")
            .WithPropertyString(registry, Xmp, "Link/?xmp:Source", "note")
            .WithPropertyString(registry, Xmp, "Link/?xml:lang", "fr");
        var oldQualifier = packet.GetQualifier(registry, Xmp, "Link", Xmp, "Source");
        var result = packet.WithPropertyUri(registry, Xmp, "Link", "urn:old");
        AssertUri(result, registry, "Link", "urn:old");
        Assert.Same(oldQualifier, result.GetQualifier(registry, Xmp, "Link", Xmp, "Source"));
        Assert.Equal("fr", result.GetProperty(Xmp, "Link")!.Language);
        Assert.Same(result, result.WithPropertyUri(registry, Xmp, "Link", "urn:old"));
        var updated = result.WithPropertyUri(registry, Xmp, "Link", "urn:new");
        AssertUri(updated, registry, "Link", "urn:new");
        AssertUri(result.WithPropertyString(registry, Xmp, "Link", "urn:string"), registry, "Link", "urn:string");
        Assert.Equal(XmpNodeKind.Literal, packet.GetProperty(Xmp, "Link")!.Kind);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(updated, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        AssertUri(roundTrip, registry, "Link", "urn:new");
        Assert.Equal("fr", roundTrip.GetProperty(Xmp, "Link")!.Language);
        Assert.Equal("note", roundTrip.GetQualifier(registry, Xmp, "Link", Xmp, "Source")!.Value);
    }

    [Fact]
    public void ResolvesAliasesAndNormalizesControlCharacters()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithAppendedArrayItemString(
            registry, Dc, "creator", "author", XmpNodeKind.OrderedArray);
        var result = packet.WithPropertyUri(registry, Xmp, "Author", "urn:one\u0001two");
        AssertUri(result, registry, "Author", "urn:one two");
        Assert.Equal("author", packet.GetArrayItem(registry, Dc, "creator", 1)!.Value);
        Assert.Equal(XmpNodeKind.Uri, result.GetArrayItem(registry, Dc, "creator", 1)!.Kind);
    }

    [Fact]
    public void RejectsUnsupportedTargetsAndMissingPaths()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithPropertyString(registry, Xmp, "Simple", "value")
            .WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure)
            .WithPropertyString(registry, Xmp, "Simple/?xml:lang", "en");
        Assert.Throws<ArgumentNullException>(() => packet.WithPropertyUri(null!, Xmp, "Simple", "urn:x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Simple", null!)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Root", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Missing/xmp:Link", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Simple/?xml:lang", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Root/?xml:lang", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Simple/?xmp:Missing/xmp:Field", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyUri(registry, Xmp, "Simple/xmp:Field", "urn:x")).Error);
        Assert.Equal("en", packet.GetProperty(Xmp, "Simple")!.Language);
    }

    private static void AssertUri(XmpPacket packet, XmpSchemaRegistry registry, string path, string value)
    {
        var node = packet.GetPropertyByPath(registry, Xmp, path)!;
        Assert.Equal(XmpNodeKind.Uri, node.Kind);
        Assert.Equal(value, node.Value);
    }
}
