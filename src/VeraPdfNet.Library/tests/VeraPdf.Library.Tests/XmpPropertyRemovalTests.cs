using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpPropertyRemovalTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Fact]
    public void RemovesTopLevelPropertyPreservingSubjectAndUnchangedNodes()
    {
        var packet = XmpPacket.ParseDiscovered($"<?xpacket begin='data'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:One>one</p:One><p:Two>two</p:Two></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithoutProperty(registry, Xmp, "One");
        Assert.Null(result.GetProperty(Xmp, "One"));
        Assert.Equal("one", packet.GetProperty(Xmp, "One")!.Value);
        Assert.Same(packet.GetProperty(Xmp, "Two"), result.GetProperty(Xmp, "Two"));
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        Assert.Same(result, result.WithoutProperty(registry, Xmp, "Missing"));
    }

    [Fact]
    public void RemovesNestedFieldsAndArrayItemsWithoutPruningParents()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Field>v</p:Field>" +
            "<p:Array><rdf:Seq><rdf:li>one</rdf:li><rdf:li>two</rdf:li><rdf:li>three</rdf:li></rdf:Seq></p:Array>" +
            "</p:Root>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithoutProperty(registry, Xmp, "Root/xmp:Array[2]");
        Assert.Equal(new[] { "one", "three" }, result.GetPropertyByPath(registry, Xmp, "Root/xmp:Array")!.Children.Select(x => x.Value));
        Assert.Equal(3, packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Array")!.Children.Count);
        result = result.WithoutProperty(registry, Xmp, "Root/xmp:Field");
        Assert.Null(result.GetPropertyByPath(registry, Xmp, "Root/xmp:Field"));
        result = result.WithoutProperty(registry, Xmp, "Root/xmp:Array[last()]");
        result = result.WithoutProperty(registry, Xmp, "Root/xmp:Array[1]");
        Assert.Empty(result.GetPropertyByPath(registry, Xmp, "Root/xmp:Array")!.Children);
        result = result.WithoutProperty(registry, Xmp, "Root/xmp:Array");
        Assert.Empty(result.GetProperty(Xmp, "Root")!.Children);
        Assert.Same(result, result.WithoutProperty(registry, Xmp, "Root/xmp:Missing"));
    }

    [Fact]
    public void RemovesLanguageAndNamedQualifiersIndependently()
    {
        var packet = Parse("<p:Qualified rdf:parseType='Resource'><rdf:value xml:lang='en'>v</rdf:value>" +
            "<p:Source>note</p:Source><p:Detail>detail</p:Detail></p:Qualified>");
        var registry = new XmpSchemaRegistry();
        var noLanguage = packet.WithoutProperty(registry, Xmp, "Qualified/?xml:lang");
        Assert.Null(noLanguage.GetProperty(Xmp, "Qualified")!.Language);
        Assert.Equal("en", packet.GetProperty(Xmp, "Qualified")!.Language);
        Assert.Equal(2, noLanguage.GetProperty(Xmp, "Qualified")!.Qualifiers.Count);
        var noSource = noLanguage.WithoutProperty(registry, Xmp, "Qualified/?xmp:Source");
        Assert.Single(noSource.GetProperty(Xmp, "Qualified")!.Qualifiers);
        Assert.Equal("detail", noSource.GetQualifier(registry, Xmp, "Qualified", Xmp, "Detail")!.Value);
        Assert.Same(noSource, noSource.WithoutProperty(registry, Xmp, "Qualified/?xml:lang"));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(noSource, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output).GetProperty(Xmp, "Qualified")!;
        Assert.Null(roundTrip.Language);
        Assert.Equal("v", roundTrip.Value);
        Assert.Equal("detail", Assert.Single(roundTrip.Qualifiers).Value);
    }

    [Fact]
    public void ResolvesAliasesAndSelectorsForRemoval()
    {
        var packet = Parse("<dc:creator><rdf:Seq><rdf:li>first</rdf:li><rdf:li>second</rdf:li></rdf:Seq></dc:creator>" +
            "<dc:title><rdf:Alt><rdf:li xml:lang='x-default'>default</rdf:li>" +
            "<rdf:li xml:lang='fr'>titre</rdf:li></rdf:Alt></dc:title>" +
            "<p:Structs><rdf:Bag><rdf:li rdf:parseType='Resource'><p:Key>one</p:Key></rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>two</p:Key></rdf:li></rdf:Bag></p:Structs>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithoutProperty(registry, Xmp, "Author");
        Assert.Equal("second", Assert.Single(result.GetProperty(Dc, "creator")!.Children).Value);
        result = result.WithoutProperty(registry, "http://ns.adobe.com/pdf/1.3/", "Title");
        Assert.Equal("fr", Assert.Single(result.GetProperty(Dc, "title")!.Children).Language);
        result = result.WithoutProperty(registry, Xmp, "Structs[xmp:Key='two']");
        Assert.Single(result.GetProperty(Xmp, "Structs")!.Children);
        Assert.Equal("one", result.GetProperty(Xmp, "Structs")!.Children[0].Children[0].Value);
    }

    [Fact]
    public void RemovesNestedQualifierValuesAndLanguageFromArrayItems()
    {
        var packet = Parse("<p:Qualified rdf:parseType='Resource'><rdf:value>value</rdf:value>" +
            "<p:Extra rdf:parseType='Resource'><p:Field>field</p:Field><p:Keep>keep</p:Keep></p:Extra>" +
            "</p:Qualified><dc:title><rdf:Alt><rdf:li xml:lang='fr'>titre</rdf:li></rdf:Alt></dc:title>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithoutProperty(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field");
        Assert.Null(result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field"));
        Assert.Equal("keep", result.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Keep")!.Value);
        result = result.WithoutProperty(registry, Dc, "title[1]/?xml:lang");
        Assert.Null(result.GetProperty(Dc, "title")!.Children[0].Language);
        Assert.Equal("fr", packet.GetProperty(Dc, "title")!.Children[0].Language);
        Assert.Equal("field", packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Extra/xmp:Field")!.Value);
    }

    [Fact]
    public void ReportsInvalidInputAndInvalidTraversal()
    {
        var packet = Parse("<p:Simple>value</p:Simple><p:Array><rdf:Bag/></p:Array>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithoutProperty(null!, Xmp, "Simple"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithoutProperty(registry, "", "Simple")).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithoutProperty(registry, Xmp, "")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithoutProperty(registry, Xmp, "Simple/xmp:Field")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithoutProperty(registry, Xmp, "Array[0]")).Error);
        Assert.Same(packet, packet.WithoutProperty(registry, Xmp, "Array[1]"));
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:dc='{Dc}'><rdf:Description>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
