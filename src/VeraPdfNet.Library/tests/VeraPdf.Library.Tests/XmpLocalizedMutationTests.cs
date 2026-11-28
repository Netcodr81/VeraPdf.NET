using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpLocalizedMutationTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";

    [Fact]
    public void CreatesDefaultAndSpecificItemsAndPreservesPacket()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Keep>keep</p:Keep></p:Root>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Labels", null, "en_us", "hello\u0001");
        result = result.WithLocalizedText(registry, Xmp, "Root/xmp:Labels", null, "x-default", "nested");
        AssertItems(result, registry, "Labels", ("x-default", "hello "), ("en-US", "hello "));
        AssertItems(result, registry, "Root/xmp:Labels", ("x-default", "nested"));
        Assert.Null(packet.GetProperty(Xmp, "Labels"));
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"));
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        AssertItems(XmpPacket.Parse(output), registry, "Labels", ("x-default", "hello "), ("en-US", "hello "));
    }

    [Fact]
    public void UpdatesSpecificAndMatchingDefaultOnly()
    {
        var packet = Parse("<p:Labels><rdf:Alt><rdf:li xml:lang='fr'>old</rdf:li>" +
            "<rdf:li xml:lang='x-default'>old</rdf:li><rdf:li xml:lang='de'>other</rdf:li></rdf:Alt></p:Labels>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Labels", null, "fr", "new");
        AssertItems(result, registry, "Labels", ("x-default", "new"), ("fr", "new"), ("de", "other"));
        AssertItems(packet, registry, "Labels", ("fr", "old"), ("x-default", "old"), ("de", "other"));
        result = result.WithLocalizedText(registry, Xmp, "Labels", null, "de", "changed");
        AssertItems(result, registry, "Labels", ("x-default", "new"), ("fr", "new"), ("de", "changed"));
    }

    [Fact]
    public void UpdatingDefaultSynchronizesOnlyMatchingValues()
    {
        var packet = Parse("<p:Labels><rdf:Alt><rdf:li xml:lang='x-default'>old</rdf:li>" +
            "<rdf:li xml:lang='fr'>old</rdf:li><rdf:li xml:lang='de'>other</rdf:li></rdf:Alt></p:Labels>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Labels", null, "x-default", "new");
        AssertItems(result, registry, "Labels", ("x-default", "new"), ("fr", "new"), ("de", "other"));
    }

    [Fact]
    public void SingleAndMultipleGenericMatchesFollowDifferentPaths()
    {
        var packet = Parse("<p:One><rdf:Alt><rdf:li xml:lang='x-default'>old</rdf:li>" +
            "<rdf:li xml:lang='en-US'>old</rdf:li></rdf:Alt></p:One>" +
            "<p:Many><rdf:Alt><rdf:li xml:lang='x-default'>default</rdf:li>" +
            "<rdf:li xml:lang='en-US'>us</rdf:li><rdf:li xml:lang='en-GB'>gb</rdf:li></rdf:Alt></p:Many>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "One", "en", "en-CA", "new");
        result = result.WithLocalizedText(registry, Xmp, "Many", "en", "en-CA", "canada");
        AssertItems(result, registry, "One", ("x-default", "new"), ("en-US", "new"));
        AssertItems(result, registry, "Many", ("x-default", "default"), ("en-US", "us"),
            ("en-GB", "gb"), ("en-CA", "canada"));
    }

    [Fact]
    public void DefaultAndFirstItemFallbacksCreateSpecificItem()
    {
        var packet = Parse("<p:Default><rdf:Alt><rdf:li xml:lang='x-default'>old</rdf:li></rdf:Alt></p:Default>" +
            "<p:First><rdf:Alt><rdf:li xml:lang='fr'>bonjour</rdf:li></rdf:Alt></p:First>" +
            "<p:Empty><rdf:Alt/></p:Empty>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Default", null, "de", "new");
        result = result.WithLocalizedText(registry, Xmp, "First", null, "de", "hallo");
        result = result.WithLocalizedText(registry, Xmp, "Empty", null, "fr", "bonjour");
        AssertItems(result, registry, "Default", ("x-default", "new"), ("de", "new"));
        AssertItems(result, registry, "First", ("fr", "bonjour"), ("de", "hallo"));
        AssertItems(result, registry, "Empty", ("x-default", "bonjour"), ("fr", "bonjour"));
    }

    [Fact]
    public void HandlesMissingDefaultAndMultipleExistingItems()
    {
        var packet = Parse("<p:Single><rdf:Alt><rdf:li xml:lang='en-US'>old</rdf:li></rdf:Alt></p:Single>" +
            "<p:Default><rdf:Alt><rdf:li xml:lang='x-default'>old</rdf:li>" +
            "<rdf:li xml:lang='fr'>autre</rdf:li></rdf:Alt></p:Default>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Single", "en", "en-CA", "new");
        result = result.WithLocalizedText(registry, Xmp, "Default", null, "de", "neu");
        AssertItems(result, registry, "Single", ("x-default", "new"), ("en-US", "new"));
        AssertItems(result, registry, "Default", ("x-default", "old"), ("fr", "autre"), ("de", "neu"));
    }

    [Fact]
    public void ResolvesAliasesAndRetainsOtherQualifiers()
    {
        var packet = Parse("<p:Labels><rdf:Alt><rdf:li rdf:parseType='Resource'>" +
            "<rdf:value xml:lang='fr'>old</rdf:value><p:Source>note</p:Source>" +
            "</rdf:li></rdf:Alt></p:Labels>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithLocalizedText(registry, Xmp, "Labels", null, "fr", "new");
        var item = result.GetArrayItem(registry, Xmp, "Labels", 2)!;
        Assert.Equal("note", Assert.Single(item.Qualifiers).Value);
        result = result.WithLocalizedText(registry, Xmp, "Title", null, "en", "alias");
        Assert.Equal("alias", result.GetLocalizedText(registry, "http://purl.org/dc/elements/1.1/",
            "title", null, "en")!.Value);
        Assert.Equal("old", packet.GetArrayItem(registry, Xmp, "Labels", 1)!.Value);
    }

    [Fact]
    public void ValidatesEveryItemAndMissingIntermediateNodes()
    {
        var packet = Parse("<p:Bad><rdf:Alt><rdf:li xml:lang='fr'>ok</rdf:li><rdf:li>missing</rdf:li></rdf:Alt></p:Bad>" +
            "<p:Composite><rdf:Alt><rdf:li rdf:parseType='Resource'><p:Field>v</p:Field></rdf:li></rdf:Alt></p:Composite>" +
            "<p:Seq><rdf:Seq/></p:Seq>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithLocalizedText(null!, Xmp, "Bad", null, "fr", "x"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Bad", null, "", "x")).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Bad", null, "fr", null!)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Bad", null, "fr", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Composite", null, "fr", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Seq", null, "fr", "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithLocalizedText(registry, Xmp, "Missing/xmp:Title", null, "fr", "x")).Error);
    }

    private static void AssertItems(XmpPacket packet, XmpSchemaRegistry registry, string path,
        params (string Language, string Value)[] expected)
    {
        var items = packet.GetPropertyByPath(registry, Xmp, path)!.Children;
        Assert.Equal(expected.Select(item => ((string?)item.Language, (string?)item.Value)).ToArray(),
            items.Select(item => (item.Language, item.Value)).ToArray());
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description rdf:about='subject'>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
