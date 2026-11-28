using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpCompositeCreationTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Theory]
    [InlineData(XmpNodeKind.Structure)]
    [InlineData(XmpNodeKind.UnorderedArray)]
    [InlineData(XmpNodeKind.OrderedArray)]
    [InlineData(XmpNodeKind.AlternativeArray)]
    public void CreatesEmptyCompositeAndRoundTrips(XmpNodeKind kind)
    {
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.Create();
        var result = packet.WithCompositeProperty(registry, Xmp, "Container", kind);
        Assert.Null(packet.About);
        Assert.Null(packet.PacketHeader);
        Assert.Empty(packet.Properties);
        Assert.Null(packet.GetProperty(Xmp, "Container"));
        Assert.Equal(kind, result.GetProperty(Xmp, "Container")!.Kind);
        Assert.Empty(result.GetProperty(Xmp, "Container")!.Children);
        Assert.Same(result, result.WithCompositeProperty(registry, Xmp, "Container", kind));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var parsed = XmpPacket.Parse(output);
        Assert.Equal(kind, parsed.GetProperty(Xmp, "Container")!.Kind);
        Assert.Empty(parsed.GetProperty(Xmp, "Container")!.Children);
    }

    [Fact]
    public void BuildsNestedMetadataWithoutMutatingOriginalPacket()
    {
        var registry = new XmpSchemaRegistry();
        var initial = XmpPacket.Create().WithPropertyString(registry, Xmp, "Keep", "original");
        var result = initial.WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure)
            .WithCompositeProperty(registry, Xmp, "Root/xmp:Nested", XmpNodeKind.Structure)
            .WithPropertyString(registry, Xmp, "Root/xmp:Nested/xmp:Name", "name")
            .WithCompositeProperty(registry, Xmp, "Root/xmp:Nested/xmp:Items", XmpNodeKind.OrderedArray)
            .WithAppendedArrayItemString(registry, Xmp, "Root/xmp:Nested/xmp:Items", "item");
        Assert.Null(initial.GetProperty(Xmp, "Root"));
        Assert.Same(initial.GetProperty(Xmp, "Keep"), result.GetProperty(Xmp, "Keep"));
        Assert.Equal("name", result.GetPropertyString(registry, Xmp, "Root/xmp:Nested/xmp:Name"));
        Assert.Equal("item", result.GetArrayItem(registry, Xmp, "Root/xmp:Nested/xmp:Items", 1)!.Value);
        Assert.Same(result, result.WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var parsed = XmpPacket.Parse(output);
        Assert.Equal("name", parsed.GetPropertyString(registry, Xmp, "Root/xmp:Nested/xmp:Name"));
        Assert.Equal("item", parsed.GetArrayItem(registry, Xmp, "Root/xmp:Nested/xmp:Items", 1)!.Value);
    }

    [Fact]
    public void ResolvesAliasesAndSelectedStructuresWithoutLosingMetadata()
    {
        var xml = $"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:Rows><rdf:Bag><rdf:li rdf:parseType='Resource'>" +
            "<p:Key>one</p:Key></rdf:li><rdf:li rdf:parseType='Resource'><p:Key>two</p:Key>" +
            "</rdf:li></rdf:Bag></p:Rows></rdf:Description></rdf:RDF>";
        var registry = new XmpSchemaRegistry();
        var packet = XmpPacket.ParseDiscovered(xml);
        var first = packet.GetArrayItem(registry, Xmp, "Rows", 1);
        var result = packet.WithCompositeProperty(registry, Xmp, "Rows[xmp:Key='two']/xmp:Data", XmpNodeKind.Structure)
            .WithCompositeProperty(registry, Xmp, "Title", XmpNodeKind.AlternativeArray)
            .WithLocalizedText(registry, Xmp, "Title", null, "en", "title");
        Assert.Same(first, result.GetArrayItem(registry, Xmp, "Rows", 1));
        Assert.Equal(XmpNodeKind.Structure, result.GetPropertyByPath(registry, Xmp, "Rows[2]/xmp:Data")!.Kind);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Rows[2]/xmp:Data"));
        Assert.Equal("title", result.GetLocalizedText(registry, Dc, "title", null, "en")!.Value);
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
    }

    [Fact]
    public void CreatesFieldsWithinStructuredQualifiersAndKeepsExistingSelectedNodes()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>" +
            "<p:Qualified rdf:parseType='Resource'><rdf:value>value</rdf:value>" +
            "<p:Extra rdf:parseType='Resource'><p:Keep>keep</p:Keep></p:Extra></p:Qualified>" +
            "<p:Rows><rdf:Bag><rdf:li rdf:parseType='Resource'><p:Key>one</p:Key></rdf:li></rdf:Bag></p:Rows>" +
            "</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var packet = XmpPacket.Parse(input);
        var registry = new XmpSchemaRegistry();
        var result = packet.WithCompositeProperty(registry, Xmp, "Qualified/?xmp:Extra/xmp:Data", XmpNodeKind.Structure);
        Assert.Equal(XmpNodeKind.Structure, result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Data")!.Kind);
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep"));
        Assert.Same(result, result.WithCompositeProperty(registry, Xmp, "Rows[1]", XmpNodeKind.Structure));
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Data"));
    }

    [Fact]
    public void RejectsUnsupportedTargetsAndNeverClearsExistingContent()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>" +
            "<p:Simple>value</p:Simple><p:Root rdf:parseType='Resource'><p:Field>v</p:Field></p:Root>" +
            "<p:Items><rdf:Seq><rdf:li>one</rdf:li></rdf:Seq></p:Items></rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var packet = XmpPacket.Parse(input);
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithCompositeProperty(null!, Xmp, "Root", XmpNodeKind.Structure));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, "", "Root", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Literal)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Root", (XmpNodeKind)99)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Simple", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Items", XmpNodeKind.UnorderedArray)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Missing/xmp:Child", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Items[2]", XmpNodeKind.Structure)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithCompositeProperty(registry, Xmp, "Simple/?xmp:Composite", XmpNodeKind.Structure)).Error);
        Assert.Same(packet, packet.WithCompositeProperty(registry, Xmp, "Root", XmpNodeKind.Structure));
        Assert.Equal("v", packet.GetPropertyString(registry, Xmp, "Root/xmp:Field"));
    }
}
