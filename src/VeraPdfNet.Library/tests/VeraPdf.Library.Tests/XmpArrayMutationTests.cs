using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpArrayMutationTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Fact]
    public void InsertsBeforeItemsAndAppendsWithoutMutatingOriginal()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Items><rdf:Seq>" +
            "<rdf:li>one</rdf:li><rdf:li>two</rdf:li></rdf:Seq></p:Items><p:Keep>keep</p:Keep></p:Root>");
        var registry = new XmpSchemaRegistry();
        var before = packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Items")!;
        var result = packet.WithInsertedArrayItemString(registry, Xmp, "Root/xmp:Items", 1, "zero");
        result = result.WithInsertedArrayItemString(registry, Xmp, "Root/xmp:Items", 3, "middle");
        result = result.WithInsertedArrayItemString(registry, Xmp, "Root/xmp:Items", 5, "tail");
        result = result.WithInsertedArrayItemString(registry, Xmp, "Root/xmp:Items", -1, "end\u0001");
        Assert.Equal(new[] { "zero", "one", "middle", "two", "tail", "end " },
            result.GetPropertyByPath(registry, Xmp, "Root/xmp:Items")!.Children.Select(x => x.Value));
        Assert.Equal(new[] { "one", "two" }, before.Children.Select(x => x.Value));
        Assert.Same(before.Children[0], result.GetPropertyByPath(registry, Xmp, "Root/xmp:Items")!.Children[1]);
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"));
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        Assert.Equal(6, XmpPacket.Parse(output).CountArrayItems(registry, Xmp, "Root/xmp:Items"));
    }

    [Fact]
    public void ReplacesWholeItemsIncludingCompositeAndQualifiedItems()
    {
        var packet = Parse("<dc:title><rdf:Alt><rdf:li xml:lang='fr'>titre</rdf:li></rdf:Alt></dc:title>" +
            "<p:Items><rdf:Bag><rdf:li rdf:parseType='Resource'><p:Key>old</p:Key></rdf:li>" +
            "<rdf:li>last</rdf:li></rdf:Bag></p:Items>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithArrayItemString(registry, Xmp, "Items", 1, "simple");
        result = result.WithArrayItemString(registry, Xmp, "Items", -1, "new last");
        result = result.WithArrayItemString(registry, Dc, "title", -1, "no language");
        Assert.Equal(XmpNodeKind.Literal, result.GetArrayItem(registry, Xmp, "Items", 1)!.Kind);
        Assert.Empty(result.GetArrayItem(registry, Xmp, "Items", 1)!.Children);
        Assert.Equal("new last", result.GetArrayItem(registry, Xmp, "Items", 2)!.Value);
        Assert.Null(result.GetArrayItem(registry, Dc, "title", 1)!.Language);
        Assert.Equal("fr", packet.GetArrayItem(registry, Dc, "title", 1)!.Language);
        Assert.Equal(XmpNodeKind.Structure, packet.GetArrayItem(registry, Xmp, "Items", 1)!.Kind);
    }

    [Fact]
    public void InsertsIntoEmptyArrayAndResolvesArraySelectors()
    {
        var packet = Parse("<p:Empty><rdf:Bag/></p:Empty><p:Outer><rdf:Seq>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>a</p:Key><p:Items><rdf:Seq/></p:Items></rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>b</p:Key><p:Items><rdf:Seq/></p:Items></rdf:li>" +
            "</rdf:Seq></p:Outer>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithInsertedArrayItemString(registry, Xmp, "Empty", -1, "first");
        result = result.WithInsertedArrayItemString(registry, Xmp, "Outer[xmp:Key='b']/xmp:Items", 1, "selected");
        Assert.Equal("first", result.GetArrayItem(registry, Xmp, "Empty", 1)!.Value);
        Assert.Equal("selected", result.GetArrayItem(registry, Xmp, "Outer[2]/xmp:Items", 1)!.Value);
        Assert.Equal(0, result.CountArrayItems(registry, Xmp, "Outer[1]/xmp:Items"));
    }

    [Fact]
    public void UpdatesArrayInsideQualifierAndPreservesPacketHeader()
    {
        var packet = XmpPacket.ParseDiscovered($"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><p:Qualified rdf:parseType='Resource'><rdf:value>value</rdf:value>" +
            "<p:Extra><rdf:Bag><rdf:li>before</rdf:li></rdf:Bag></p:Extra></p:Qualified></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithInsertedArrayItemString(registry, Xmp, "Qualified/?xmp:Extra", -1, "after");
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        Assert.NotNull(result.PacketHeader);
        Assert.Equal(new[] { "before", "after" },
            result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra")!.Children.Select(x => x.Value));
        Assert.Single(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra")!.Children);
    }

    [Theory]
    [InlineData(XmpNodeKind.UnorderedArray)]
    [InlineData(XmpNodeKind.OrderedArray)]
    [InlineData(XmpNodeKind.AlternativeArray)]
    public void AppendsAndCreatesSpecifiedArrayForms(XmpNodeKind kind)
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Keep>keep</p:Keep></p:Root>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithAppendedArrayItemString(registry, Xmp, "Items", "one\u0001", kind);
        result = result.WithAppendedArrayItemString(registry, Xmp, "Items", "one ", kind);
        result = result.WithAppendedArrayItemString(registry, Xmp, "Root/xmp:Items", "nested", kind);
        Assert.Equal(kind, result.GetProperty(Xmp, "Items")!.Kind);
        Assert.Equal(new[] { "one ", "one " }, result.GetProperty(Xmp, "Items")!.Children.Select(x => x.Value));
        Assert.Equal(kind, result.GetPropertyByPath(registry, Xmp, "Root/xmp:Items")!.Kind);
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"));
        Assert.Null(packet.GetProperty(Xmp, "Items"));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        Assert.Equal(kind, roundTrip.GetProperty(Xmp, "Items")!.Kind);
        Assert.Equal(2, roundTrip.CountArrayItems(registry, Xmp, "Items"));
    }

    [Fact]
    public void AppendsIntoExistingSelectedArrayAndCreatesFieldOfSelectedStructure()
    {
        var packet = Parse("<p:Outer><rdf:Bag><rdf:li rdf:parseType='Resource'>" +
            "<p:Key>one</p:Key><p:Items><rdf:Seq><rdf:li>old</rdf:li></rdf:Seq></p:Items></rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>two</p:Key></rdf:li></rdf:Bag></p:Outer>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithAppendedArrayItemString(registry, Xmp, "Outer[xmp:Key='one']/xmp:Items", "new");
        result = result.WithAppendedArrayItemString(registry, Xmp, "Outer[xmp:Key='two']/xmp:Items",
            "created", XmpNodeKind.OrderedArray);
        Assert.Equal(new[] { "old", "new" },
            result.GetPropertyByPath(registry, Xmp, "Outer[1]/xmp:Items")!.Children.Select(x => x.Value));
        Assert.Equal("created", result.GetArrayItem(registry, Xmp, "Outer[2]/xmp:Items", 1)!.Value);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Outer[2]/xmp:Items"));
    }

    [Fact]
    public void AppendReportsMissingKindsAndUnsupportedPaths()
    {
        var packet = Parse("<p:Simple>value</p:Simple><p:Items><rdf:Seq/></p:Items>" +
            "<p:Root rdf:parseType='Resource'/>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithAppendedArrayItemString(null!, Xmp, "Items", "x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Items", null!)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Items", "x", XmpNodeKind.Literal)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Missing", "x")).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Items", "x", XmpNodeKind.UnorderedArray)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Simple", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Missing/xmp:Items", "x", XmpNodeKind.OrderedArray)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Items[1]", "x", XmpNodeKind.OrderedArray)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithAppendedArrayItemString(registry, Xmp, "Root/?xmp:Items", "x", XmpNodeKind.OrderedArray)).Error);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Items"));
    }

    [Fact]
    public void AppendCreatesFieldInStructuredQualifierAndPreservesPacketHeader()
    {
        var packet = XmpPacket.ParseDiscovered($"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:Qualified rdf:parseType='Resource'>" +
            "<rdf:value>value</rdf:value><p:Extra rdf:parseType='Resource'><p:Keep>keep</p:Keep></p:Extra>" +
            "</p:Qualified></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithAppendedArrayItemString(registry, Xmp, "Qualified/?xmp:Extra/xmp:Items",
            "item", XmpNodeKind.UnorderedArray);
        Assert.Equal("item", result.GetArrayItem(registry, Xmp, "Qualified/?xmp:Extra/xmp:Items", 1)!.Value);
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep"));
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Items"));
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        Assert.Equal(packet.About, result.About);
    }

    [Fact]
    public void ReportsInvalidInputsAndBounds()
    {
        var packet = Parse("<p:Empty><rdf:Bag/></p:Empty><p:Simple>value</p:Simple>" +
            "<p:Items><rdf:Seq><rdf:li>one</rdf:li></rdf:Seq></p:Items>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithArrayItemString(null!, Xmp, "Items", 1, "x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithArrayItemString(registry, Xmp, "Items", 1, null!)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithArrayItemString(registry, Xmp, "Missing", 1, "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemString(registry, Xmp, "Simple", 1, "x")).Error);
        foreach (var index in new[] { -2, 0, 2, int.MaxValue })
            Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithArrayItemString(registry, Xmp, "Items", index, "x")).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithArrayItemString(registry, Xmp, "Empty", -1, "x")).Error);
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => packet.WithInsertedArrayItemString(registry, Xmp, "Empty", 2, "x")).Error);
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:dc='{Dc}'><rdf:Description rdf:about='subject'>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
