using System.Text;
using System.Xml.Linq;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpPacketWriterTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";

    [Fact]
    public void RoundTripsSupportedRawTreeAndLeavesStreamsOpen()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='urn:custom'>"
            + "<rdf:Description rdf:about='subject'><p:Text>before&#13;after &amp; &lt;</p:Text>"
            + "<p:Empty/><p:Uri rdf:resource='urn:test?a=1&amp;b=2'/>"
            + "<p:Struct rdf:parseType='Resource'><c:Field>field</c:Field></p:Struct>"
            + "<p:Bag><rdf:Bag><rdf:li>one</rdf:li></rdf:Bag></p:Bag>"
            + "<p:Seq><rdf:Seq><rdf:li xml:lang='en'>one</rdf:li><rdf:li rdf:parseType='Resource'><c:Field>two</c:Field></rdf:li></rdf:Seq></p:Seq>"
            + "<p:Alt><rdf:Alt><rdf:li xml:lang='x-default'>default</rdf:li></rdf:Alt></p:Alt>"
            + "<p:Qualified rdf:parseType='Resource'><rdf:value><rdf:Seq><rdf:li>v</rdf:li></rdf:Seq></rdf:value>"
            + "<c:Source rdf:parseType='Resource'><c:Detail>detail</c:Detail></c:Source></p:Qualified>"
            + "<p:UriQualified rdf:resource='urn:uri' c:Source='note' xml:lang='fr'/>"
            + "</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var source = XmpPacket.Parse(input);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(source, output);
        Assert.True(output.CanWrite);
        output.Position = 0;
        var roundTrip = XmpPacket.Parse(output);
        Assert.Equal(source.About, roundTrip.About);
        Assert.Equal(source.Properties.Count, roundTrip.Properties.Count);
        for (var i = 0; i < source.Properties.Count; i++)
            AssertNodeEqual(source.Properties[i], roundTrip.Properties[i]);
    }

    [Fact]
    public void SortOrdersPropertiesFieldsAndQualifiersWithoutChangingArraysOrPacket()
    {
        const string custom = "urn:custom";
        const string dc = "http://purl.org/dc/elements/1.1/";
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='{custom}' xmlns:dc='{dc}'>"
            + "<rdf:Description><c:Z>z</c:Z><p:Z>z</p:Z><dc:title>title</dc:title>"
            + "<p:A rdf:parseType='Resource'><c:Z>z</c:Z><p:B>b</p:B><p:A>a</p:A></p:A>"
            + "<p:Array><rdf:Seq><rdf:li>z</rdf:li><rdf:li>a</rdf:li></rdf:Seq></p:Array>"
            + "<p:Qualified rdf:parseType='Resource'><rdf:value xml:lang='en'>v</rdf:value>"
            + "<c:Z>z</c:Z><p:B>b</p:B><p:A>a</p:A></p:Qualified>"
            + "</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var packet = XmpPacket.Parse(input);
        var originalOrder = packet.Properties.Select(node => node.Name).ToArray();
        using var output = new MemoryStream();
        XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions { Sort = true });
        var document = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        XNamespace rdf = Rdf;
        XNamespace xmp = Xmp;
        XNamespace c = custom;
        XNamespace d = dc;
        var description = document.Descendants(rdf + "Description").Single();
        Assert.Equal(new[] { xmp + "A", xmp + "Array", xmp + "Qualified", xmp + "Z", d + "title", c + "Z" },
            description.Elements().Select(element => element.Name));
        Assert.Equal(new[] { xmp + "A", xmp + "B", c + "Z" },
            description.Element(xmp + "A")!.Elements().Select(element => element.Name));
        Assert.Equal(new[] { "z", "a" }, description.Element(xmp + "Array")!.Element(rdf + "Seq")!
            .Elements(rdf + "li").Select(element => element.Value));
        var qualified = description.Element(xmp + "Qualified")!.Elements().ToArray();
        Assert.Equal(new[] { rdf + "value", xmp + "A", xmp + "B", c + "Z" }, qualified.Select(element => element.Name));
        Assert.Equal("en", (string?)qualified[0].Attribute(XNamespace.Xml + "lang"));
        Assert.Equal(originalOrder, packet.Properties.Select(node => node.Name));
        output.Position = 0;
        var parsed = XmpPacket.Parse(output);
        Assert.Equal(new[] { "z", "a" }, parsed.GetProperty(Xmp, "Array")!.Children.Select(node => node.Value));
        Assert.Equal("v", parsed.GetProperty(Xmp, "Qualified")!.Value);
    }

    [Fact]
    public void SortedTypedResourcesRetainTypeAndSortNestedValues()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='urn:custom'>"
            + "<rdf:Description><p:Typed><c:Type>"
            + "<rdf:value rdf:parseType='Resource'><c:Z>z</c:Z><c:A>a</c:A></rdf:value>"
            + "<c:Z>last</c:Z><c:A>first</c:A></c:Type></p:Typed></rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(XmpPacket.Parse(input), output, new XmpPacketWriterOptions { Sort = true });
        XNamespace rdf = Rdf;
        XNamespace c = "urn:custom";
        var type = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray())).Descendants(c + "Type").Single();
        Assert.Equal(new[] { rdf + "value", c + "A", c + "Z" }, type.Elements().Select(element => element.Name));
        Assert.Equal(new[] { c + "A", c + "Z" }, type.Element(rdf + "value")!.Elements().Select(element => element.Name));
        output.Position = 0;
        var typed = XmpPacket.Parse(output).GetProperty(Xmp, "Typed")!;
        Assert.Equal("urn:custom:Type", typed.Qualifiers.Single(q => q.NamespaceUri == Rdf).Value);
    }

    [Fact]
    public void SortingUsesStableNamespacePrefixesRegardlessOfInputOrder()
    {
        static XmpPacket Packet(string first, string second)
        {
            var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:a='urn:a' xmlns:z='urn:z'>"
                + $"<rdf:Description>{first}{second}</rdf:Description></rdf:RDF>";
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return XmpPacket.Parse(input);
        }
        using var first = new MemoryStream();
        using var second = new MemoryStream();
        var options = new XmpPacketWriterOptions { Sort = true, IncludePacketWrapper = true, ExactPacketLength = 1000 };
        XmpPacketWriter.Write(Packet("<z:Z>z</z:Z>", "<a:A>a</a:A>"), first, options);
        XmpPacketWriter.Write(Packet("<a:A>a</a:A>", "<z:Z>z</z:Z>"), second, options);
        Assert.Equal(first.ToArray(), second.ToArray());
        Assert.Equal(1000, first.Length);
    }

    [Fact]
    public void RoundTripsTypedResourcesAndTheirQualifiers()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='urn:custom'>"
            + "<rdf:Description>"
            + "<p:Typed><c:Type><c:Field>value</c:Field></c:Type></p:Typed>"
            + "<p:Qualified xml:lang='fr'><c:Type><rdf:value>text</rdf:value><c:Source>note</c:Source></c:Type></p:Qualified>"
            + "<p:Array><rdf:Seq><rdf:li><c:Type><rdf:value rdf:resource='urn:test'/><c:Source>note</c:Source></c:Type></rdf:li></rdf:Seq></p:Array>"
            + "<p:Nested><c:Type><rdf:value rdf:parseType='Resource'><c:Inner><c:Other><c:Field>nested</c:Field></c:Other></c:Inner></rdf:value><c:Source><c:Other><c:Field>note</c:Field></c:Other></c:Source></c:Type></p:Nested>"
            + "</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var packet = XmpPacket.Parse(input);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(packet, output);
        var text = Encoding.UTF8.GetString(output.ToArray());
        Assert.Contains(":Type", text);
        Assert.DoesNotContain("<rdf:type", text);
        var document = XDocument.Parse(text);
        XNamespace rdf = Rdf;
        XNamespace xmp = Xmp;
        XNamespace custom = "urn:custom";
        var description = document.Descendants(rdf + "Description").First();
        var typed = description.Element(xmp + "Typed")!.Element(custom + "Type")!;
        Assert.Equal("value", typed.Element(rdf + "value")!.Element(custom + "Field")!.Value);
        var qualified = description.Element(xmp + "Qualified")!.Element(custom + "Type")!;
        Assert.Equal("text", qualified.Element(rdf + "value")!.Value);
        Assert.Equal("note", qualified.Element(custom + "Source")!.Value);
        output.Position = 0;
        var copy = XmpPacket.Parse(output);
        Assert.Equal(packet.Properties.Count, copy.Properties.Count);
        for (var i = 0; i < packet.Properties.Count; i++)
            AssertNodeEqual(packet.Properties[i], copy.Properties[i]);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void WritesEmptyMetadataWithoutChangingSubject()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes($"<rdf:RDF xmlns:rdf='{Rdf}'><rdf:Description/></rdf:RDF>"));
        var packet = XmpPacket.Parse(input);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(packet, output);
        output.Position = 0;
        var copy = XmpPacket.Parse(output);
        Assert.Null(copy.About);
        Assert.Empty(copy.Properties);
        Assert.Throws<ArgumentNullException>(() => XmpPacketWriter.Write(null!, output));
        Assert.Throws<ArgumentNullException>(() => XmpPacketWriter.Write(packet, null!));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void WritesPacketWrappersAndPreservesContent(bool readOnly, bool omitXmpMeta)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(
            $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description><p:Text>caf\u00e9</p:Text></rdf:Description></rdf:RDF>"));
        var packet = XmpPacket.Parse(input);
        using var output = new MemoryStream();
        var padding = readOnly ? 0 : 513;
        XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions
        {
            IncludePacketWrapper = true, ReadOnlyPacket = readOnly, OmitXmpMetaElement = omitXmpMeta, PaddingBytes = padding
        });
        var text = Encoding.UTF8.GetString(output.ToArray());
        Assert.StartsWith("<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>\n", text);
        Assert.EndsWith(new string(' ', padding) + (readOnly ? "<?xpacket end=\"r\"?>" : "<?xpacket end=\"w\"?>"), text);
        Assert.Equal(!omitXmpMeta, text.Contains("<x:xmpmeta", StringComparison.Ordinal));
        output.Position = 0;
        var parsed = XmpPacket.ParseDiscovered(output);
        Assert.Equal("caf\u00e9", parsed.GetProperty(Xmp, "Text")!.Value);
        Assert.Contains("W5M0MpCehiHzreSzNTczkc9d", parsed.PacketHeader);
        Assert.True(output.CanWrite);
    }

    [Fact]
    public void ExactLengthCountsUtf8BytesAndDoesNotDependOnOutputPosition()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(
            $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='caf\u00e9'/></rdf:RDF>"));
        var packet = XmpPacket.Parse(input);
        using var output = new MemoryStream();
        output.WriteByte(42);
        XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions { IncludePacketWrapper = true, ExactPacketLength = 1000 });
        Assert.Equal(1001, output.Length);
        output.Position = 1;
        Assert.Equal("caf\u00e9", XmpPacket.ParseDiscovered(output).GetProperty(Xmp, "A")!.Value);
        using var tooSmall = new MemoryStream();
        Assert.Equal(XmpError.BadSerialize, Assert.Throws<XmpException>(() =>
            XmpPacketWriter.Write(packet, tooSmall, new XmpPacketWriterOptions { IncludePacketWrapper = true, ExactPacketLength = 1 })).Error);
        Assert.Empty(tooSmall.ToArray());
    }

    [Fact]
    public void ExactLengthAcceptsMinimumSizeAndSupportsNonSeekableStreams()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes($"<rdf:RDF xmlns:rdf='{Rdf}'/>"));
        var packet = XmpPacket.Parse(input);
        using var baseline = new MemoryStream();
        XmpPacketWriter.Write(packet, baseline, new XmpPacketWriterOptions { IncludePacketWrapper = true });
        var minimum = checked((int)baseline.Length);
        using var output = new NonSeekableWriteStream();
        XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions { IncludePacketWrapper = true, ExactPacketLength = minimum });
        Assert.Equal(baseline.ToArray(), output.ToArray());
        Assert.True(output.CanWrite);
        using var tooSmall = new NonSeekableWriteStream();
        Assert.Equal(XmpError.BadSerialize, Assert.Throws<XmpException>(() =>
            XmpPacketWriter.Write(packet, tooSmall, new XmpPacketWriterOptions { IncludePacketWrapper = true, ExactPacketLength = minimum - 1 })).Error);
        Assert.Empty(tooSmall.ToArray());
    }

    [Theory]
    [InlineData(XmpPacketEncoding.Utf16BigEndian, false, false)]
    [InlineData(XmpPacketEncoding.Utf16BigEndian, true, false)]
    [InlineData(XmpPacketEncoding.Utf16BigEndian, true, true)]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian, false, false)]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian, true, false)]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian, true, true)]
    public void WritesUtf16WithoutBomAndRoundTrips(XmpPacketEncoding packetEncoding, bool wrapped, bool readOnly)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(
            $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='caf\u00e9 &#x1F600;'/></rdf:RDF>"));
        using var output = new NonSeekableWriteStream();
        XmpPacketWriter.Write(XmpPacket.Parse(input), output, new XmpPacketWriterOptions
        {
            Encoding = packetEncoding, IncludePacketWrapper = wrapped, ReadOnlyPacket = readOnly,
            PaddingBytes = wrapped && !readOnly ? 4098 : 0
        });
        var encoding = new UnicodeEncoding(packetEncoding == XmpPacketEncoding.Utf16BigEndian, false, true);
        var bytes = output.ToArray();
        var text = encoding.GetString(bytes);
        Assert.False(text.StartsWith('\uFEFF'));
        Assert.StartsWith(wrapped ? "<?xpacket begin=\"\uFEFF\"" : "<x:xmpmeta", text);
        if (wrapped)
            Assert.EndsWith(new string(' ', readOnly ? 0 : 2049) +
                (readOnly ? "<?xpacket end=\"r\"?>" : "<?xpacket end=\"w\"?>"), text);
        using var roundTrip = new MemoryStream(bytes);
        var parsed = wrapped ? XmpPacket.ParseDiscovered(roundTrip) : XmpPacket.Parse(roundTrip);
        Assert.Equal("caf\u00e9 \U0001F600", parsed.GetProperty(Xmp, "A")!.Value);
        Assert.True(output.CanWrite);
    }

    [Theory]
    [InlineData(XmpPacketEncoding.Utf16BigEndian)]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian)]
    public void Utf16ExactLengthIsMeasuredInBytes(XmpPacketEncoding encoding)
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(
            $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='&#x1F600;'/></rdf:RDF>"));
        var packet = XmpPacket.Parse(input);
        using var baseline = new MemoryStream();
        XmpPacketWriter.Write(packet, baseline, new XmpPacketWriterOptions { Encoding = encoding, IncludePacketWrapper = true });
        var minimum = checked((int)baseline.Length);
        using var output = new NonSeekableWriteStream();
        XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions
            { Encoding = encoding, IncludePacketWrapper = true, ExactPacketLength = minimum + 6 });
        Assert.Equal(minimum + 6, output.ToArray().Length);
        using var parsedStream = new MemoryStream(output.ToArray());
        Assert.Equal("\U0001F600", XmpPacket.ParseDiscovered(parsedStream).GetProperty(Xmp, "A")!.Value);
        using var tooSmall = new MemoryStream();
        Assert.Equal(XmpError.BadSerialize, Assert.Throws<XmpException>(() => XmpPacketWriter.Write(packet, tooSmall,
            new XmpPacketWriterOptions { Encoding = encoding, IncludePacketWrapper = true, ExactPacketLength = minimum - 2 })).Error);
        Assert.Equal(0, tooSmall.Length);
    }

    [Theory]
    [InlineData(XmpPacketEncoding.Utf8, 10000)]
    [InlineData(XmpPacketEncoding.Utf16BigEndian, 20000)]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian, 20000)]
    public void AddsThumbnailPaddingOnlyWhenThumbnailPropertyIsAbsent(XmpPacketEncoding encoding, int extraBytes)
    {
        var prefix = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>";
        foreach (var hasThumbnail in new[] { false, true })
        {
            var xml = prefix + (hasThumbnail ? "<p:Thumbnails><rdf:Alt/></p:Thumbnails>" : "")
                + "</rdf:Description></rdf:RDF>";
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            var packet = XmpPacket.Parse(input);
            using var baseline = new MemoryStream();
            using var output = new NonSeekableWriteStream();
            XmpPacketWriter.Write(packet, baseline, new XmpPacketWriterOptions
                { Encoding = encoding, IncludePacketWrapper = true, PaddingBytes = 10 });
            XmpPacketWriter.Write(packet, output, new XmpPacketWriterOptions
                { Encoding = encoding, IncludePacketWrapper = true, PaddingBytes = 10, IncludeThumbnailPadding = true });
            var bytes = output.ToArray();
            Assert.Equal(baseline.Length + (hasThumbnail ? 0 : extraBytes), bytes.Length);
            using var roundTrip = new MemoryStream(bytes);
            Assert.Equal(hasThumbnail, XmpPacket.ParseDiscovered(roundTrip).GetProperty(Xmp, "Thumbnails") is not null);
            Assert.True(output.CanWrite);
        }
    }

    [Theory]
    [InlineData(XmpPacketEncoding.Utf8, "\r\n", "\t")]
    [InlineData(XmpPacketEncoding.Utf16BigEndian, "\n", "   ")]
    [InlineData(XmpPacketEncoding.Utf16LittleEndian, "", " ")]
    public void IndentedOutputPreservesValuesAndHonorsByteLength(XmpPacketEncoding encoding, string newline, string indent)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><p:Text> a&#13;b&#10;c </p:Text>" +
            "<p:Struct rdf:parseType='Resource'><p:Field>v</p:Field></p:Struct>" +
            "<p:Array><rdf:Seq><rdf:li xml:lang='fr'>texte</rdf:li></rdf:Seq></p:Array>" +
            "</rdf:Description></rdf:RDF>";
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var packet = XmpPacket.Parse(input);
        var options = new XmpPacketWriterOptions
        {
            Encoding = encoding, IncludePacketWrapper = true, Indent = true, NewLineChars = newline, IndentChars = indent
        };
        using var baseline = new MemoryStream();
        XmpPacketWriter.Write(packet, baseline, options);
        var codec = encoding switch
        {
            XmpPacketEncoding.Utf8 => Encoding.UTF8,
            XmpPacketEncoding.Utf16BigEndian => Encoding.BigEndianUnicode,
            _ => Encoding.Unicode
        };
        var text = codec.GetString(baseline.ToArray());
        Assert.StartsWith("<?xpacket begin=\"\uFEFF\" id=\"W5M0MpCehiHzreSzNTczkc9d\"?>" + newline, text);
        Assert.Contains(newline + indent + "<rdf:RDF", text);
        Assert.EndsWith(newline + "<?xpacket end=\"w\"?>", text);
        baseline.Position = 0;
        var copy = XmpPacket.ParseDiscovered(baseline);
        for (var i = 0; i < packet.Properties.Count; i++)
            AssertNodeEqual(packet.Properties[i], copy.Properties[i]);

        using var exact = new NonSeekableWriteStream();
        var extra = encoding == XmpPacketEncoding.Utf8 ? 7 : 8;
        var length = checked((int)baseline.Length + extra);
        XmpPacketWriter.Write(packet, exact, new XmpPacketWriterOptions
        {
            Encoding = encoding, IncludePacketWrapper = true, Indent = true, NewLineChars = newline,
            IndentChars = indent, ExactPacketLength = length
        });
        Assert.Equal(length, exact.ToArray().Length);
    }

    [Fact]
    public void RejectsInconsistentOptionsBeforeWriting()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes($"<rdf:RDF xmlns:rdf='{Rdf}'/>"));
        var packet = XmpPacket.Parse(input);
        XmpPacketWriterOptions[] invalid =
        [
            new() { PaddingBytes = -1 },
            new() { ExactPacketLength = -1 },
            new() { PaddingBytes = 1 },
            new() { ReadOnlyPacket = true },
            new() { ExactPacketLength = 500 },
            new() { IncludePacketWrapper = true, ReadOnlyPacket = true, PaddingBytes = 1 },
            new() { IncludePacketWrapper = true, ReadOnlyPacket = true, ExactPacketLength = 500 },
            new() { IncludePacketWrapper = true, PaddingBytes = 1, ExactPacketLength = 500 },
            new() { IncludeThumbnailPadding = true },
            new() { IncludePacketWrapper = true, ReadOnlyPacket = true, IncludeThumbnailPadding = true },
            new() { IncludePacketWrapper = true, ExactPacketLength = 500, IncludeThumbnailPadding = true },
            new() { IndentChars = null! },
            new() { NewLineChars = null! },
            new() { Indent = true, IndentChars = "<" },
            new() { NewLineChars = "x" },
            new() { Encoding = (XmpPacketEncoding)42 },
            new() { Encoding = XmpPacketEncoding.Utf16BigEndian, IncludePacketWrapper = true, PaddingBytes = 1 },
            new() { Encoding = XmpPacketEncoding.Utf16LittleEndian, IncludePacketWrapper = true, ExactPacketLength = 501 }
        ];
        using var output = new MemoryStream();
        foreach (var options in invalid)
        {
            Assert.Equal(XmpError.BadOptions, Assert.Throws<XmpException>(() => XmpPacketWriter.Write(packet, output, options)).Error);
            Assert.Empty(output.ToArray());
        }
        Assert.Throws<ArgumentNullException>(() => XmpPacketWriter.Write(packet, output, null!));
    }

    [Fact]
    public void WritesBareRdfWithoutPacketInstructions()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes($"<rdf:RDF xmlns:rdf='{Rdf}'/>"));
        using var output = new MemoryStream();
        XmpPacketWriter.Write(XmpPacket.Parse(input), output, new XmpPacketWriterOptions { OmitXmpMetaElement = true });
        var text = Encoding.UTF8.GetString(output.ToArray());
        Assert.StartsWith("<rdf:RDF", text);
        Assert.DoesNotContain("xpacket", text);
        output.Position = 0;
        Assert.Empty(XmpPacket.Parse(output).Properties);
    }

    private sealed class NonSeekableWriteStream : Stream
    {
        private readonly MemoryStream _buffer = new();
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public byte[] ToArray() => _buffer.ToArray();
        public override void Flush() => _buffer.Flush();
        public override void Write(byte[] buffer, int offset, int count) => _buffer.Write(buffer, offset, count);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) _buffer.Dispose();
            base.Dispose(disposing);
        }
    }

    private static void AssertNodeEqual(XmpNode expected, XmpNode actual)
    {
        Assert.Equal(expected.NamespaceUri, actual.NamespaceUri);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Kind, actual.Kind);
        Assert.Equal(expected.Value, actual.Value);
        Assert.Equal(expected.Language, actual.Language);
        Assert.Equal(expected.Children.Count, actual.Children.Count);
        Assert.Equal(expected.Qualifiers.Count, actual.Qualifiers.Count);
        for (var i = 0; i < expected.Children.Count; i++) AssertNodeEqual(expected.Children[i], actual.Children[i]);
        for (var i = 0; i < expected.Qualifiers.Count; i++) AssertNodeEqual(expected.Qualifiers[i], actual.Qualifiers[i]);
    }
}
