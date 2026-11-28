using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpUriArrayMutationTests
{
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Theory]
    [InlineData(XmpNodeKind.UnorderedArray)]
    [InlineData(XmpNodeKind.OrderedArray)]
    [InlineData(XmpNodeKind.AlternativeArray)]
    public void CreatesInsertsAndReplacesUriItemsAndRoundTrips(XmpNodeKind kind)
    {
        var registry = new XmpSchemaRegistry();
        var empty = XmpPacket.Create();
        var original = empty.WithAppendedArrayItemString(registry, Xmp, "Items", "old", kind)
            .WithPropertyString(registry, Xmp, "Items[1]/?xmp:Source", "source");
        var old = original.GetArrayItem(registry, Xmp, "Items", 1)!;
        var result = original.WithInsertedArrayItemUri(registry, Xmp, "Items", 1, "urn:first\u0001")
            .WithInsertedArrayItemUri(registry, Xmp, "Items", -1, "urn:last")
            .WithArrayItemUri(registry, Xmp, "Items", 2, "relative");

        Assert.Empty(empty.Properties);
        Assert.Equal("old", old.Value);
        Assert.Equal("source", Assert.Single(old.Qualifiers).Value);
        Assert.Equal(new[] { "urn:first ", "relative", "urn:last" },
            result.GetProperty(Xmp, "Items")!.Children.Select(item => item.Value));
        Assert.All(result.GetProperty(Xmp, "Items")!.Children, item =>
        {
            Assert.Equal(XmpNodeKind.Uri, item.Kind);
            Assert.Empty(item.Qualifiers);
        });

        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var parsed = XmpPacket.Parse(output);
        Assert.Equal(kind, parsed.GetProperty(Xmp, "Items")!.Kind);
        Assert.Equal(new[] { "urn:first ", "relative", "urn:last" },
            parsed.GetProperty(Xmp, "Items")!.Children.Select(item => item.Value));
        Assert.All(parsed.GetProperty(Xmp, "Items")!.Children, item => Assert.Equal(XmpNodeKind.Uri, item.Kind));
    }

    [Fact]
    public void AppendsIntoNestedSelectedAndAliasedArrays()
    {
        const string rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.ParseDiscovered($"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:Rows><rdf:Bag><rdf:li rdf:parseType='Resource'>" +
            "<p:Key>one</p:Key><p:Keep>keep</p:Keep></rdf:li></rdf:Bag></p:Rows></rdf:Description></rdf:RDF>");
        var result = packet.WithAppendedArrayItemUri(registry, Xmp, "Rows[xmp:Key='one']/xmp:Links",
                "urn:one", XmpNodeKind.OrderedArray)
            .WithAppendedArrayItemUri(registry, Xmp, "Rows[1]/xmp:Links", "urn:two")
            .WithAppendedArrayItemUri(registry, Xmp, "Authors", "urn:author", XmpNodeKind.OrderedArray);

        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Rows[1]/xmp:Links"));
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Rows[1]/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Rows[1]/xmp:Keep"));
        Assert.Equal("urn:two", result.GetArrayItem(registry, Xmp, "Rows[1]/xmp:Links", -1)!.Value);
        Assert.Equal(XmpNodeKind.Uri, result.GetArrayItem(registry, Dc, "creator", 1)!.Kind);
    }

    [Fact]
    public void ReplacesCompositeAndLocalizedItemsWithoutPreservingContents()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create()
            .WithAppendedArrayItemComposite(registry, Xmp, "Items", XmpNodeKind.Structure, XmpNodeKind.AlternativeArray)
            .WithPropertyString(registry, Xmp, "Items[1]/xmp:Field", "value")
            .WithAppendedArrayItemString(registry, Xmp, "Items", "localized")
            .WithPropertyString(registry, Xmp, "Items[2]/?xml:lang", "fr");
        var result = packet.WithArrayItemUri(registry, Xmp, "Items", 1, "urn:first")
            .WithArrayItemUri(registry, Xmp, "Items", -1, "urn:last");
        Assert.Empty(result.GetArrayItem(registry, Xmp, "Items", 1)!.Children);
        Assert.Null(result.GetArrayItem(registry, Xmp, "Items", 2)!.Language);
        Assert.Equal("fr", packet.GetArrayItem(registry, Xmp, "Items", 2)!.Language);
    }

    [Fact]
    public void RejectsInvalidValuesPathsKindsAndIndexes()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithCompositeProperty(registry, Xmp, "Items", XmpNodeKind.OrderedArray)
            .WithPropertyString(registry, Xmp, "Simple", "value");
        Assert.Throws<ArgumentNullException>(() => packet.WithArrayItemUri(null!, Xmp, "Items", 1, "urn:x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithArrayItemUri(registry, Xmp, "Items", 1, null!)).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemUri(registry, Xmp, "Items", null!)).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithArrayItemUri(registry, Xmp, "Items", -1, "urn:x")).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemUri(registry, Xmp, "Items", 2, "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemUri(registry, Xmp, "Missing", 1, "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemUri(registry, Xmp, "Simple", "urn:x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemUri(registry, Xmp, "Missing/xmp:Items", "urn:x", XmpNodeKind.OrderedArray)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemUri(registry, Xmp, "Missing", "urn:x")).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemUri(registry, Xmp, "Items", "urn:x", XmpNodeKind.UnorderedArray)).Error);
        Assert.Empty(packet.GetProperty(Xmp, "Items")!.Children);
    }
}
