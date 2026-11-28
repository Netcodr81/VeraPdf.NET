using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpPropertyUpdateTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Fact]
    public void CreatesAndUpdatesTopLevelPropertiesWithoutChangingOriginal()
    {
        var packet = Parse("<p:Existing>before</p:Existing><p:Keep>keep</p:Keep>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyString(registry, Xmp, "New", "first\u0001second\tline");
        Assert.Equal("first second\tline", result.GetProperty(Xmp, "New")!.Value);
        Assert.Null(packet.GetProperty(Xmp, "New"));
        Assert.Same(packet.GetProperty(Xmp, "Keep"), result.GetProperty(Xmp, "Keep"));
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        result = result.WithPropertyString(registry, Xmp, "Existing", "after");
        Assert.Equal("after", result.GetProperty(Xmp, "Existing")!.Value);
        Assert.Equal("before", packet.GetProperty(Xmp, "Existing")!.Value);
        Assert.Same(result, result.WithPropertyString(registry, Xmp, "Existing", "after"));
    }

    [Fact]
    public void CreatesFieldsAndQualifiersAndUpdatesNestedValues()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Field>before</p:Field>" +
            "<p:Array><rdf:Seq><rdf:li>first</rdf:li><rdf:li>second</rdf:li></rdf:Seq></p:Array></p:Root>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyString(registry, Xmp, "Root/xmp:Added", "new");
        result = result.WithPropertyString(registry, Xmp, "Root/xmp:Field/?xmp:Source", "note");
        result = result.WithPropertyString(registry, Xmp, "Root/xmp:Field/?xmp:Source", "updated");
        result = result.WithPropertyString(registry, Xmp, "Root/xmp:Array[last()]", "changed");
        Assert.Equal("new", result.GetPropertyByPath(registry, Xmp, "Root/xmp:Added")!.Value);
        Assert.Equal("updated", result.GetPropertyByPath(registry, Xmp, "Root/xmp:Field/?xmp:Source")!.Value);
        Assert.Equal("changed", result.GetPropertyByPath(registry, Xmp, "Root/xmp:Array[2]")!.Value);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Added"));
        Assert.Equal("second", packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Array[2]")!.Value);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        Assert.Equal("updated", roundTrip.GetPropertyByPath(registry, Xmp, "Root/xmp:Field/?xmp:Source")!.Value);
    }

    [Fact]
    public void UpdatesAliasesAndLanguageQualifiers()
    {
        var packet = Parse("<dc:creator><rdf:Seq><rdf:li>author</rdf:li></rdf:Seq></dc:creator>" +
            "<dc:title><rdf:Alt><rdf:li xml:lang='x-default'>title</rdf:li>" +
            "<rdf:li>other</rdf:li></rdf:Alt></dc:title>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyString(registry, Xmp, "Author", "new author");
        result = result.WithPropertyString(registry, "http://ns.adobe.com/pdf/1.3/", "Title", "new title");
        result = result.WithPropertyString(registry, Dc, "title[2]/?xml:lang", "fr_ca");
        Assert.Equal("new author", result.GetProperty(Dc, "creator")!.Children[0].Value);
        Assert.Equal("new title", result.GetProperty(Dc, "title")!.Children[0].Value);
        Assert.Equal("fr-CA", result.GetProperty(Dc, "title")!.Children[1].Language);
        Assert.Null(packet.GetProperty(Dc, "title")!.Children[1].Language);
        Assert.Same(result, result.WithPropertyString(registry, Dc, "title[2]/?xml:lang", "fr_CA"));
    }

    [Fact]
    public void UpdatesSelectorsAndFieldsInsideStructuredQualifiers()
    {
        var packet = Parse("<p:Qualified rdf:parseType='Resource'><rdf:value>value</rdf:value>" +
            "<p:Extra rdf:parseType='Resource'><p:Field>before</p:Field><p:Keep>keep</p:Keep></p:Extra>" +
            "</p:Qualified><p:Structs><rdf:Bag>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>one</p:Key><p:Value>old</p:Value></rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>two</p:Key><p:Value>old</p:Value></rdf:li>" +
            "</rdf:Bag></p:Structs>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyString(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field", "after");
        result = result.WithPropertyString(registry, Xmp, "Qualified/?xmp:Extra/xmp:New", "created");
        result = result.WithPropertyString(registry, Xmp, "Structs[xmp:Key='two']/xmp:Value", "selected");
        Assert.Equal("after", result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field")!.Value);
        Assert.Equal("created", result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:New")!.Value);
        Assert.Equal("keep", result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep")!.Value);
        Assert.Equal("selected", result.GetPropertyByPath(registry, Xmp, "Structs[xmp:Key='two']/xmp:Value")!.Value);
        Assert.Equal("old", result.GetPropertyByPath(registry, Xmp, "Structs[xmp:Key='one']/xmp:Value")!.Value);
        Assert.Equal("before", packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field")!.Value);
    }

    [Fact]
    public void RejectsUnsupportedTargetsAndMissingIntermediateNodes()
    {
        var packet = Parse("<p:Simple>value</p:Simple><p:Root rdf:parseType='Resource'/>" +
            "<p:Array><rdf:Bag><rdf:li>value</rdf:li></rdf:Bag></p:Array>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithPropertyString(null!, Xmp, "Simple", "x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Simple", null!)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Root", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Missing/xmp:Field", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Simple/xmp:Field", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Array[2]", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Simple/?xml:lang/xmp:Field", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyString(registry, Xmp, "Author", "x")).Error);
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:dc='{Dc}'><rdf:Description rdf:about='subject'>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
