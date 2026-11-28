using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpCompositeArrayMutationTests
{
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";

    [Theory]
    [InlineData(XmpNodeKind.Structure)]
    [InlineData(XmpNodeKind.UnorderedArray)]
    [InlineData(XmpNodeKind.OrderedArray)]
    [InlineData(XmpNodeKind.AlternativeArray)]
    public void AppendsAndRoundTripsEachCompositeItemKind(XmpNodeKind itemKind)
    {
        var registry = new XmpSchemaRegistry();
        var empty = XmpPacket.Create();
        var result = empty.WithAppendedArrayItemComposite(registry, Xmp, "Items", itemKind, XmpNodeKind.OrderedArray);
        Assert.Empty(empty.Properties);
        var item = result.GetArrayItem(registry, Xmp, "Items", 1)!;
        Assert.Equal(itemKind, item.Kind);
        Assert.Empty(item.Children);
        Assert.Equal("", item.NamespaceUri);
        Assert.Equal("", item.Name);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        Assert.Equal(itemKind, XmpPacket.Parse(output).GetArrayItem(registry, Xmp, "Items", 1)!.Kind);
    }

    [Fact]
    public void BuildsStructuresAndNestedArraysFromEmptyPacket()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithAppendedArrayItemComposite(
            registry, Xmp, "Rows", XmpNodeKind.Structure, XmpNodeKind.UnorderedArray);
        var result = packet.WithPropertyString(registry, Xmp, "Rows[1]/xmp:Key", "first")
            .WithCompositeProperty(registry, Xmp, "Rows[1]/xmp:Values", XmpNodeKind.OrderedArray)
            .WithAppendedArrayItemComposite(registry, Xmp, "Rows[xmp:Key='first']/xmp:Values", XmpNodeKind.UnorderedArray)
            .WithAppendedArrayItemString(registry, Xmp, "Rows[1]/xmp:Values[1]", "nested");
        Assert.Empty(packet.GetArrayItem(registry, Xmp, "Rows", 1)!.Children);
        Assert.Equal("nested", result.GetArrayItem(registry, Xmp, "Rows[1]/xmp:Values[1]", 1)!.Value);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        Assert.Equal("nested", roundTrip.GetArrayItem(registry, Xmp, "Rows[xmp:Key='first']/xmp:Values[1]", 1)!.Value);
    }

    [Fact]
    public void InsertAndReplaceRespectIndexesAndDiscardOldItems()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create()
            .WithAppendedArrayItemString(registry, Xmp, "Items", "one", XmpNodeKind.OrderedArray)
            .WithAppendedArrayItemString(registry, Xmp, "Items", "two")
            .WithPropertyString(registry, Xmp, "Items[2]/?xmp:Source", "qualifier");
        var originalFirst = packet.GetArrayItem(registry, Xmp, "Items", 1);
        var result = packet.WithInsertedArrayItemComposite(registry, Xmp, "Items", 1, XmpNodeKind.Structure)
            .WithInsertedArrayItemComposite(registry, Xmp, "Items", -1, XmpNodeKind.OrderedArray)
            .WithArrayItemComposite(registry, Xmp, "Items", 3, XmpNodeKind.UnorderedArray)
            .WithPropertyString(registry, Xmp, "Items[1]/xmp:Name", "new");
        Assert.Equal(new[] { XmpNodeKind.Structure, XmpNodeKind.Literal, XmpNodeKind.UnorderedArray, XmpNodeKind.OrderedArray },
            result.GetProperty(Xmp, "Items")!.Children.Select(item => item.Kind));
        Assert.Same(originalFirst, result.GetArrayItem(registry, Xmp, "Items", 2));
        Assert.Empty(result.GetArrayItem(registry, Xmp, "Items", 3)!.Qualifiers);
        Assert.Equal("qualifier", Assert.Single(packet.GetArrayItem(registry, Xmp, "Items", 2)!.Qualifiers).Value);
        Assert.Equal("new", result.GetPropertyString(registry, Xmp, "Items[1]/xmp:Name"));
    }

    [Fact]
    public void ResolvesQualifiedArrayAndPreservesPacketInformation()
    {
        const string rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        var packet = XmpPacket.ParseDiscovered($"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:Qualified rdf:parseType='Resource'>" +
            "<rdf:value>value</rdf:value><p:Extra><rdf:Bag/></p:Extra></p:Qualified></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithAppendedArrayItemComposite(registry, Xmp, "Qualified/?xmp:Extra", XmpNodeKind.Structure)
            .WithPropertyString(registry, Xmp, "Qualified/?xmp:Extra[1]/xmp:Name", "nested");
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        Assert.Empty(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra")!.Children);
        Assert.Equal("nested", result.GetPropertyString(registry, Xmp, "Qualified/?xmp:Extra[1]/xmp:Name"));
    }

    [Fact]
    public void CreatesNestedArrayAndUsesDirectAlias()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure);
        var result = packet.WithAppendedArrayItemComposite(registry, Xmp, "Root/xmp:Items", XmpNodeKind.Structure,
                XmpNodeKind.OrderedArray)
            .WithAppendedArrayItemComposite(registry, Xmp, "Authors", XmpNodeKind.Structure, XmpNodeKind.OrderedArray)
            .WithArrayItemComposite(registry, Xmp, "Authors", -1, XmpNodeKind.UnorderedArray);
        Assert.Equal(XmpNodeKind.Structure, result.GetArrayItem(registry, Xmp, "Root/xmp:Items", 1)!.Kind);
        Assert.Equal(XmpNodeKind.UnorderedArray, result.GetArrayItem(registry,
            "http://purl.org/dc/elements/1.1/", "creator", 1)!.Kind);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Items"));
    }

    [Fact]
    public void RejectsInvalidKindsPathsAndBounds()
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create().WithCompositeProperty(registry, Xmp, "Items", XmpNodeKind.OrderedArray)
            .WithPropertyString(registry, Xmp, "Simple", "value");
        Assert.Throws<ArgumentNullException>(() => packet.WithArrayItemComposite(null!, Xmp, "Items", 1, XmpNodeKind.Structure));
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Items", XmpNodeKind.Literal)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemComposite(registry, Xmp, "Items", 1, (XmpNodeKind)99)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Missing", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Items", XmpNodeKind.Structure, XmpNodeKind.UnorderedArray)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Items", XmpNodeKind.Structure, XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithArrayItemComposite(registry, Xmp, "Items", -1, XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemComposite(registry, Xmp, "Items", 2, XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemComposite(registry, Xmp, "Missing", 1, XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Simple", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Missing/xmp:Items", XmpNodeKind.Structure, XmpNodeKind.OrderedArray)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemComposite(registry, Xmp, "Items[1]", XmpNodeKind.Structure, XmpNodeKind.OrderedArray)).Error);
        Assert.Empty(packet.GetProperty(Xmp, "Items")!.Children);
    }
}
