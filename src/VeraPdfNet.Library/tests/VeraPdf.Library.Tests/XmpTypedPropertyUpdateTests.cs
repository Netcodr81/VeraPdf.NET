using System.Globalization;
using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpTypedPropertyUpdateTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Dc = "http://purl.org/dc/elements/1.1/";

    [Fact]
    public void WritesTypedValuesAndRoundTripsThroughSerialization()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'><p:Keep>untouched</p:Keep></p:Root>");
        var registry = new XmpSchemaRegistry();
        var date = XmpDateTime.Parse("2024-02-29T12:34:56.123456789+05:30");
        byte[] bytes = [0, 1, 127, 128, 255];
        var result = packet.WithPropertyBoolean(registry, Xmp, "Boolean", true)
            .WithPropertyInteger(registry, Xmp, "Integer", int.MinValue)
            .WithPropertyLong(registry, Xmp, "Long", long.MaxValue)
            .WithPropertyDouble(registry, Xmp, "Double", 0.1)
            .WithPropertyDate(registry, Xmp, "Date", date)
            .WithPropertyBase64(registry, Xmp, "Root/xmp:Bytes", bytes);
        Assert.Equal("True", result.GetPropertyString(registry, Xmp, "Boolean"));
        Assert.True(result.GetPropertyBoolean(registry, Xmp, "Boolean"));
        Assert.Equal(int.MinValue, result.GetPropertyInteger(registry, Xmp, "Integer"));
        Assert.Equal(long.MaxValue, result.GetPropertyLong(registry, Xmp, "Long"));
        Assert.Equal(0.1, result.GetPropertyDouble(registry, Xmp, "Double"));
        Assert.Equal(date.ToString(), result.GetPropertyDate(registry, Xmp, "Date")!.ToString());
        Assert.Equal(bytes, result.GetPropertyBase64(registry, Xmp, "Root/xmp:Bytes"));
        Assert.Same(packet.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"),
            result.GetPropertyByPath(registry, Xmp, "Root/xmp:Keep"));
        Assert.Null(packet.GetProperty(Xmp, "Boolean"));
        Assert.Equal(packet.About, result.About);
        Assert.Equal(packet.PacketHeader, result.PacketHeader);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(result, output);
        output.Position = 0;
        var parsed = XmpPacket.Parse(output);
        Assert.Equal(int.MinValue, parsed.GetPropertyInteger(registry, Xmp, "Integer"));
        Assert.Equal(long.MaxValue, parsed.GetPropertyLong(registry, Xmp, "Long"));
        Assert.Equal(0.1, parsed.GetPropertyDouble(registry, Xmp, "Double"));
        Assert.Equal(date.ToString(), parsed.GetPropertyDate(registry, Xmp, "Date")!.ToString());
        Assert.Equal(bytes, parsed.GetPropertyBase64(registry, Xmp, "Root/xmp:Bytes"));
    }

    [Fact]
    public void FormatsNumbersIndependentlyOfCurrentCulture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var registry = new XmpSchemaRegistry();
            var result = Parse("").WithPropertyInteger(registry, Xmp, "Integer", -123)
                .WithPropertyLong(registry, Xmp, "Long", long.MinValue)
                .WithPropertyDouble(registry, Xmp, "Double", 1.25)
                .WithPropertyBoolean(registry, Xmp, "Boolean", false);
            Assert.Equal("-123", result.GetPropertyString(registry, Xmp, "Integer"));
            Assert.Equal("-9223372036854775808", result.GetPropertyString(registry, Xmp, "Long"));
            Assert.Equal("1.25", result.GetPropertyString(registry, Xmp, "Double"));
            Assert.Equal("False", result.GetPropertyString(registry, Xmp, "Boolean"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void PreservesExistingQualifiersAndResolvesAliases()
    {
        var packet = Parse("<p:Qualified rdf:parseType='Resource'><rdf:value>old</rdf:value>" +
            "<p:Source>note</p:Source></p:Qualified>" +
            $"<dc:creator xmlns:dc='{Dc}'><rdf:Seq><rdf:li>old</rdf:li></rdf:Seq></dc:creator>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyLong(registry, Xmp, "Qualified", 42)
            .WithPropertyInteger(registry, Xmp, "Author", 7);
        Assert.Equal("42", result.GetProperty(Xmp, "Qualified")!.Value);
        Assert.Equal("note", Assert.Single(result.GetProperty(Xmp, "Qualified")!.Qualifiers).Value);
        Assert.Equal("7", result.GetArrayItem(registry, Dc, "creator", 1)!.Value);
        Assert.Equal("old", packet.GetProperty(Xmp, "Qualified")!.Value);
        Assert.Same(result, result.WithPropertyLong(registry, Xmp, "Qualified", 42));
    }

    [Fact]
    public void UpdatesSelectedItemAndQualifierUsingTypedValues()
    {
        var packet = Parse("<p:Items><rdf:Bag><rdf:li rdf:parseType='Resource'>" +
            "<p:Key>one</p:Key><p:Value>before</p:Value></rdf:li>" +
            "<rdf:li rdf:parseType='Resource'><p:Key>two</p:Key><p:Value>before</p:Value></rdf:li>" +
            "</rdf:Bag></p:Items>");
        var registry = new XmpSchemaRegistry();
        var result = packet.WithPropertyBoolean(registry, Xmp, "Items[xmp:Key='two']/xmp:Value", false)
            .WithPropertyInteger(registry, Xmp, "Items[2]/xmp:Value/?xmp:Version", 5);
        Assert.False(result.GetPropertyBoolean(registry, Xmp, "Items[2]/xmp:Value"));
        Assert.Equal(5, result.GetPropertyInteger(registry, Xmp, "Items[2]/xmp:Value/?xmp:Version"));
        Assert.Equal("before", result.GetPropertyString(registry, Xmp, "Items[1]/xmp:Value"));
        Assert.Equal("before", packet.GetPropertyString(registry, Xmp, "Items[2]/xmp:Value"));
    }

    [Fact]
    public void ReportsUnsupportedPathsAndNullInputs()
    {
        var packet = Parse("<p:Root rdf:parseType='Resource'/>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.WithPropertyBoolean(null!, Xmp, "B", true));
        Assert.Throws<ArgumentNullException>(() => packet.WithPropertyDate(registry, Xmp, "Date", null!));
        Assert.Throws<ArgumentNullException>(() => packet.WithPropertyBase64(registry, Xmp, "Bytes", null!));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyInteger(registry, Xmp, "Root", 1)).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.WithPropertyBoolean(registry, Xmp, "Missing/xmp:Field", true)).Error);
        var result = packet.WithPropertyBase64(registry, Xmp, "Bytes", []);
        Assert.Equal("", result.GetPropertyString(registry, Xmp, "Bytes"));
        Assert.Empty(result.GetPropertyBase64(registry, Xmp, "Bytes")!);
        result = result.WithPropertyDate(registry, Xmp, "EmptyDate", XmpDateTime.Create());
        Assert.Equal("", result.GetPropertyString(registry, Xmp, "EmptyDate"));
        Assert.Equal(XmpError.BadValue, Assert.Throws<XmpException>(() => result.GetPropertyDate(registry, Xmp, "EmptyDate")).Error);
    }

    private static XmpPacket Parse(string properties)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description rdf:about='subject'>{properties}</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return XmpPacket.Parse(input);
    }
}
