using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpArrayUtilitiesTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";

    [Fact]
    public void CatenatesSimpleArrayValuesAndEscapesQuotes()
    {
        var packet = Parse("<p:Items><rdf:Seq><rdf:li/><rdf:li>alpha beta</rdf:li>" +
            "<rdf:li>a,b</rdf:li><rdf:li>a;b</rdf:li><rdf:li>a  b</rdf:li>" +
            "<rdf:li>&quot;quoted&quot;</rdf:li><rdf:li rdf:resource='urn:test'/></rdf:Seq></p:Items>");
        var registry = new XmpSchemaRegistry();
        Assert.Equal("; alpha beta; \"a,b\"; \"a;b\"; \"a  b\"; \"\"\"quoted\"\"\"; urn:test",
            XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Items"));
        Assert.Equal("; alpha beta; a,b; \"a;b\"; \"a  b\"; \"\"\"quoted\"\"\"; urn:test",
            XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Items", allowCommas: true));
        Assert.Equal("", packet.GetArrayItem(registry, Xmp, "Items", 1)!.Value);
    }

    [Fact]
    public void HandlesReferenceQuotePairsAndUnicodeSeparators()
    {
        var packet = Parse("<p:Items><rdf:Bag><rdf:li>alpha&#xFF0C;beta</rdf:li>" +
            "<rdf:li>first&#x2000;&#x2000;last</rdf:li><rdf:li>&#xAB;test&#xBB;</rdf:li>" +
            "<rdf:li>first&#10;last</rdf:li></rdf:Bag></p:Items>");
        var result = XmpArrayUtilities.CatenateArrayItems(packet, new XmpSchemaRegistry(), Xmp, "Items",
            "\u3000\uff1b ", "\u00ab\u00bb");
        Assert.Equal("\u00abalpha\uff0cbeta\u00bb\u3000\uff1b " +
            "\u00abfirst\u2000\u2000last\u00bb\u3000\uff1b " +
            "\u00ab\u00ab\u00abtest\u00bb\u00bb\u00bb\u3000\uff1b \u00abfirst\nlast\u00bb", result);
    }

    [Fact]
    public void RetainsReferenceQuirksForQuotesBeforeASeparator()
    {
        var packet = Parse("<p:Items><rdf:Bag><rdf:li>a&quot;b;c&quot;d</rdf:li>" +
            "<rdf:li>a&#x301F;;b</rdf:li><rdf:li>a;b&#x301F;</rdf:li></rdf:Bag></p:Items>");
        Assert.Equal("\"a\"b;c\"\"d\"; \"a\u301f;b\"; \"a;b\u301f\u301f\"",
            XmpArrayUtilities.CatenateArrayItems(packet, new XmpSchemaRegistry(), Xmp, "Items"));
        Assert.Equal("\u00aba\"b;c\"d\u00bb; \u00aba\u301f;b\u00bb; \u00aba;b\u301f\u301f\u00bb",
            XmpArrayUtilities.CatenateArrayItems(packet, new XmpSchemaRegistry(), Xmp, "Items", quotes: "\u00ab\u00bbignored"));
    }

    [Fact]
    public void ResolvesNestedArrayPathsWithoutModifyingTree()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Items><rdf:Bag>" +
            "<rdf:li>one</rdf:li><rdf:li>two</rdf:li></rdf:Bag></p:Items></p:Root>");
        var registry = new XmpSchemaRegistry();
        var array = packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Items")!;
        Assert.Equal("one ; two", XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp,
            "Root/xmp:Items", " ; ", ""));
        Assert.Same(array, packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Items"));
        Assert.Equal(new[] { "one", "two" }, array.Children.Select(child => child.Value));
    }

    [Theory]
    [InlineData("", false, new string[0])]
    [InlineData("  ;,\t  ", false, new string[0])]
    [InlineData("a; b,c; a; A", false, new[] { "a", "b", "c", "A" })]
    [InlineData("a; b,c; a; A", true, new[] { "a", "b,c", "A" })]
    [InlineData(",a, b,,c", true, new[] { "a, b,,c" })]
    [InlineData("one space  two; three ", false, new[] { "one space", "two", "three" })]
    [InlineData("\"\"; \"one; two\"; \"a\"\"b\"; \"unfinished", false, new[] { "", "one; two", "a\"b", "unfinished" })]
    [InlineData("\u00abfirst \u00abinside\u00bb;next\u00bb; last", false, new[] { "first \u00abinside", "next\u00bb", "last" })]
    [InlineData("\u301dleft\u301eright", false, new[] { "left", "right" })]
    [InlineData("\u2019text", false, new[] { "text" })]
    [InlineData("first\u2000\u2000second\uff1bthird\u2028fourth", false, new[] { "first", "second", "third", "fourth" })]
    public void SeparatesValuesUsingReferenceRules(string input, bool preserveCommas, string[] expected)
    {
        Assert.Equal(expected, XmpArrayUtilities.SeparateArrayItems(input, preserveCommas));
    }

    [Fact]
    public void SeparatesCatenatedValuesAndKeepsFirstDistinctOccurrence()
    {
        var packet = Parse("<p:Items><rdf:Bag><rdf:li>one</rdf:li><rdf:li>a,b</rdf:li>" +
            "<rdf:li>a;b</rdf:li><rdf:li>&quot;quoted&quot;</rdf:li><rdf:li>one</rdf:li></rdf:Bag></p:Items>");
        var result = XmpArrayUtilities.CatenateArrayItems(packet, new XmpSchemaRegistry(), Xmp, "Items");
        Assert.Equal(new[] { "one", "a,b", "a;b", "\"quoted\"" }, XmpArrayUtilities.SeparateArrayItems(result));
        Assert.Equal(XmpError.BadParameter,
            Assert.Throws<XmpException>(() => XmpArrayUtilities.SeparateArrayItems(null!)).Error);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(XmpArrayUtilities.SeparateArrayItems("one"));
    }

    [Fact]
    public void AppendsDistinctValuesToExistingArrayWithoutChangingOriginal()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Items><rdf:Seq>" +
            "<rdf:li xml:lang='en'>existing</rdf:li><rdf:li>existing</rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Field>v</p:Field></rdf:li>" +
            "</rdf:Seq></p:Items><p:Other>untouched</p:Other></p:Root>");
        var registry = new XmpSchemaRegistry();
        const string path = "Root/xmp:Items";
        var oldArray = packet.GetPropertyByPath(registry, Xmp, path)!;
        var updated = XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, path,
            "existing; new; new; \"a,b\"", arrayKind: XmpNodeKind.OrderedArray);
        var array = updated.GetPropertyByPath(registry, Xmp, path)!;
        Assert.Equal(XmpNodeKind.OrderedArray, array.Kind);
        Assert.Equal(3, oldArray.Children.Count);
        Assert.Equal(5, array.Children.Count);
        Assert.Same(oldArray.Children[0], array.Children[0]);
        Assert.Equal("en", array.Children[0].Language);
        Assert.Equal(new[] { "new", "a,b" }, array.Children.Skip(3).Select(child => child.Value));
        Assert.Equal("untouched", updated.GetPropertyByPath(registry, Xmp, "Root/xmp:Other")!.Value);
        Assert.Same(updated, XmpArrayUtilities.AppendSeparatedArrayItems(updated, registry, Xmp, path, "new"));

        using var output = new MemoryStream();
        XmpPacketWriter.Write(updated, output);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output).GetPropertyByPath(registry, Xmp, path)!;
        Assert.Equal(array.Children.Select(child => child.Value), roundTrip.Children.Select(child => child.Value));
        Assert.Equal("en", roundTrip.Children[0].Language);
    }

    [Fact]
    public void CreatesTopLevelAndNestedArraysWithoutCreatingIntermediateStructures()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Field>unchanged</p:Field></p:Root>");
        var registry = new XmpSchemaRegistry();
        var top = XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "New", "a,b; a", true,
            XmpNodeKind.OrderedArray);
        Assert.Null(packet.GetProperty(Xmp, "New"));
        Assert.Equal(XmpNodeKind.OrderedArray, top.GetProperty(Xmp, "New")!.Kind);
        Assert.Equal(new[] { "a,b", "a" }, top.GetProperty(Xmp, "New")!.Children.Select(item => item.Value));
        var nested = XmpArrayUtilities.AppendSeparatedArrayItems(top, registry, Xmp, "Root/xmp:New", "");
        Assert.Equal(XmpNodeKind.UnorderedArray, nested.GetPropertyByPath(registry, Xmp, "Root/xmp:New")!.Kind);
        Assert.Empty(nested.GetPropertyByPath(registry, Xmp, "Root/xmp:New")!.Children);
        Assert.Null(top.GetPropertyByPath(registry, Xmp, "Root/xmp:New"));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Root/xmp:Missing/xmp:New", "a")).Error);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Missing"));
    }

    [Fact]
    public void AppendsInsideArrayItemsAndQualifiedValues()
    {
        var packet = Parse("<p:Outer><rdf:Seq><rdf:li rdf:parseType='Resource'>" +
            "<p:Tags><rdf:Bag><rdf:li>one</rdf:li></rdf:Bag></p:Tags></rdf:li></rdf:Seq></p:Outer>" +
            "<p:Qualified rdf:parseType='Resource'><rdf:value>value</rdf:value>" +
            "<p:Tags><rdf:Bag><rdf:li>one</rdf:li></rdf:Bag></p:Tags></p:Qualified>");
        var registry = new XmpSchemaRegistry();
        var updated = XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Outer[1]/xmp:Tags", "two");
        updated = XmpArrayUtilities.AppendSeparatedArrayItems(updated, registry, Xmp, "Qualified/?xmp:Tags", "two");
        Assert.Equal(new[] { "one", "two" }, updated.GetPropertyByPath(registry, Xmp, "Outer[1]/xmp:Tags")!.Children.Select(c => c.Value));
        Assert.Equal(new[] { "one", "two" }, updated.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Tags")!.Children.Select(c => c.Value));
        Assert.Single(packet.GetPropertyByPath(registry, Xmp, "Qualified/?xmp:Tags")!.Children);
    }

    [Fact]
    public void ArrayUpdateRejectsInvalidInputsAndIncompatiblePaths()
    {
        var packet = Parse("<p:Simple>value</p:Simple><p:Alt><rdf:Alt/></p:Alt><p:Bag><rdf:Bag/></p:Bag>");
        var registry = new XmpSchemaRegistry();
        foreach (var path in new[] { "Simple", "Alt" })
            Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() =>
                XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, path, "x")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Bag", "x", arrayKind: XmpNodeKind.OrderedArray)).Error);
        Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Bag", "x", arrayKind: XmpNodeKind.AlternativeArray)).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Bag", null!)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Bag[1]", "x")).Error);
    }

    [Fact]
    public void ArrayUpdateResolvesDirectAliasesAndPreservesPacketInformation()
    {
        const string dc = "http://purl.org/dc/elements/1.1/";
        var xml = $"<?xpacket begin='packet'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='subject'><p:Other>value</p:Other></rdf:Description></rdf:RDF>";
        var packet = XmpPacket.ParseDiscovered(xml);
        var registry = new XmpSchemaRegistry();
        var updated = XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Authors", "first");
        Assert.Equal("subject", updated.About);
        Assert.Equal(packet.PacketHeader, updated.PacketHeader);
        Assert.Equal("first", updated.GetProperty(dc, "creator")!.Children[0].Value);
        Assert.Null(packet.GetProperty(dc, "creator"));
        updated = XmpArrayUtilities.AppendSeparatedArrayItems(updated, registry, Xmp, "Authors", "second");
        Assert.Equal(new[] { "first", "second" }, updated.GetProperty(dc, "creator")!.Children.Select(item => item.Value));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.AppendSeparatedArrayItems(packet, registry, Xmp, "Author", "value")).Error);
    }

    [Fact]
    public void RejectsInvalidFormsAndOptionsButMissingArrayReturnsEmpty()
    {
        var packet = Parse("<p:Simple>value</p:Simple><p:Alt><rdf:Alt><rdf:li>v</rdf:li></rdf:Alt></p:Alt>" +
            "<p:Composite><rdf:Bag><rdf:li rdf:parseType='Resource'><p:Field>v</p:Field></rdf:li></rdf:Bag></p:Composite>" +
            "<p:Empty><rdf:Seq/></p:Empty>");
        var registry = new XmpSchemaRegistry();
        Assert.Equal("", XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Missing", "invalid", "invalid"));
        Assert.Equal("", XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Empty"));
        foreach (var path in new[] { "Simple", "Alt", "Composite" })
            Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
                XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, path)).Error);
        foreach (var separator in new[] { ",", " ", ";;", ";x" })
            Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
                XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Empty", separator)).Error);
        foreach (var quotes in new[] { "'", "\u00ab", "\u00ab\"", "\u2019" })
            Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
                XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "Empty", quotes: quotes)).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.CatenateArrayItems(packet, registry, "", "Empty")).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() =>
            XmpArrayUtilities.CatenateArrayItems(packet, registry, Xmp, "")).Error);
        Assert.Throws<ArgumentNullException>(() => XmpArrayUtilities.CatenateArrayItems(null!, registry, Xmp, "Empty"));
        Assert.Throws<ArgumentNullException>(() => XmpArrayUtilities.CatenateArrayItems(packet, null!, Xmp, "Empty"));
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
