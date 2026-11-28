namespace VeraPdf.Parser.Tests;

using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Io;
using VeraPdf.Parser.Parser;
using System.Text;
using VeraPdf.Parser.Filters;
using VeraPdf.Parser.Pd;
using VeraPdf.Parser.Fonts;

public class ParserLibraryTests
{
    [Fact]
    public void LibraryHasStableName()
    {
        Assert.Equal("VeraPdf.Parser", ParserLibrary.Name);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(10239)]
    [InlineData(15000)]
    public void SeekableStreamPreservesContents(int size)
    {
        var original = Enumerable.Range(0, size).Select(value => (byte)value).ToArray();
        using var source = new MemoryStream(original);
        using var seekable = PdfSeekableStream.Create(source);

        Assert.Equal(original.Length, seekable.StreamLength);
        Assert.Equal(0, seekable.ReadByte());
        Assert.Equal(1, seekable.ReadByte());

        seekable.Seek(0);
        var copy = new byte[original.Length];
        Assert.Equal(original.Length, seekable.Read(copy, 0, copy.Length));
        Assert.Equal(original, copy);
    }

    [Fact]
    public void SeekableSubstreamHasIndependentPosition()
    {
        using var source = new MemoryStream("0123456789"u8.ToArray());
        using var original = PdfSeekableStream.Create(source);
        original.Seek(4);
        using var substream = original.GetSubstream(2, 5);

        Assert.Equal(0, substream.Offset);
        Assert.Equal(5, substream.StreamLength);
        Assert.Equal('2', substream.ReadByte());
        Assert.Equal('3', substream.ReadByte());
        Assert.Equal(2, substream.Offset);
        Assert.Equal('4', original.Peek());
    }

    [Fact]
    public void LexerReadsPdfValues()
    {
        var bytes = Encoding.ASCII.GetBytes("% comment\r\n[12 -3.5 /A#20 null true] << /Length 5 >>");
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var lexer = new PdfLexer(source);

        Assert.Equal(PdfTokenType.OpenArray, lexer.NextToken().Type);
        var integer = lexer.NextToken();
        Assert.Equal(PdfTokenType.Integer, integer.Type);
        Assert.Equal(12, integer.IntegerValue);
        Assert.Equal(PdfTokenType.Real, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Name, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Keyword, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Keyword, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.CloseArray, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.OpenDictionary, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Name, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Integer, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.CloseDictionary, lexer.NextToken().Type);
    }

    [Fact]
    public void LexerReadsLiteralHexAndNameEscapes()
    {
        var bytes = Encoding.Latin1.GetBytes("(A\\(B\\)\\\\C\\101) <48656c6c6f> /Type#20Name");
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var lexer = new PdfLexer(source);

        var literal = lexer.NextToken();
        Assert.Equal(PdfTokenType.LiteralString, literal.Type);
        Assert.Equal("A(B)\\CA", Encoding.Latin1.GetString(literal.Bytes!));

        var hex = lexer.NextToken();
        Assert.Equal(PdfTokenType.HexString, hex.Type);
        Assert.Equal("Hello", Encoding.ASCII.GetString(hex.Bytes!));

        var name = lexer.NextToken();
        Assert.Equal(PdfTokenType.Name, name.Type);
        Assert.Equal("Type Name", Encoding.Latin1.GetString(name.Bytes!));
    }

    [Fact]
    public void CosParserPreservesStringSyntaxMetadata()
    {
        var bytes = Encoding.ASCII.GetBytes("(literal) <41 4G 2>");
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var parser = new PdfCosParser(source);

        var literal = Assert.IsType<CosString>(parser.NextValue());
        Assert.False(literal.IsHex);
        Assert.True(literal.ContainsOnlyHex);
        Assert.Equal(0, literal.HexCount);

        var hex = Assert.IsType<CosString>(parser.NextValue());
        Assert.True(hex.IsHex);
        Assert.False(hex.ContainsOnlyHex);
        Assert.Equal(5, hex.HexCount);
        Assert.Equal(new byte[] { 0x41, 0x42 }, hex.Value);
    }

    [Fact]
    public void CosStringTextUsesBomAndPdfDocEncoding()
    {
        Assert.Equal("\u2022", new CosString([0x80]).Text);
        Assert.Equal("é", new CosString([0xFE, 0xFF, 0x00, 0xE9]).Text);
        Assert.Equal("é", new CosString([0xEF, 0xBB, 0xBF, 0xC3, 0xA9]).Text);
    }

    [Fact]
    public void CosParserReadsArrayDictionaryAndIndirectReference()
    {
        var bytes = Encoding.ASCII.GetBytes("[null 4 0 R << /Name /Example >>]");
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var parser = new PdfCosParser(source);

        var array = Assert.IsType<CosArray>(parser.NextValue());
        Assert.IsType<CosNull>(array.Values[0]);
        Assert.IsType<CosIndirectReference>(array.Values[1]);
        Assert.IsType<CosDictionary>(array.Values[2]);
    }

    [Fact]
    public void CosStreamUsesDirectLength()
    {
        var bytes = Encoding.Latin1.GetBytes("<< /Length 5 >>\nstream\nhello\nendstream");
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var lexer = new PdfLexer(source);
        Assert.Equal(PdfTokenType.OpenDictionary, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Name, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.Integer, lexer.NextToken().Type);
        Assert.Equal(PdfTokenType.CloseDictionary, lexer.NextToken().Type);
        source.Seek(0);
        var parser = new PdfCosParser(source);

        var dictionary = Assert.IsType<CosDictionary>(parser.NextValue());
        var streamKeyword = parser.Lexer.NextToken();
        Assert.Equal(PdfTokenType.Keyword, streamKeyword.Type);

        var stream = parser.ReadStream(dictionary);
        Assert.Equal("hello", Encoding.ASCII.GetString(stream.RawData));
        Assert.True(stream.StreamKeywordCRLFCompliant);
        Assert.True(stream.EndstreamKeywordEOLCompliant);
    }

    [Theory]
    [InlineData("stream\r\nx\r\nendstream", true, true)]
    [InlineData("stream\nx\nendstream", true, true)]
    [InlineData("stream\rx\nendstream", false, true)]
    [InlineData("stream\nxendstream", true, false)]
    public void CosStreamRetainsKeywordSpacing(
        string sourceText,
        bool expectedStreamSpacing,
        bool expectedEndstreamSpacing)
    {
        var bytes = Encoding.Latin1.GetBytes("<< /Length 1 >>" + sourceText);
        using var source = PdfSeekableStream.Create(new MemoryStream(bytes));
        var parser = new PdfCosParser(source);
        var dictionary = Assert.IsType<CosDictionary>(parser.NextValue());
        Assert.Equal(PdfTokenType.Keyword, parser.Lexer.NextToken().Type);

        var stream = parser.ReadStream(dictionary);

        Assert.Equal(expectedStreamSpacing, stream.StreamKeywordCRLFCompliant);
        Assert.Equal(expectedEndstreamSpacing, stream.EndstreamKeywordEOLCompliant);
    }

    [Fact]
    public void SeekableStreamRejectsConfiguredMaximum()
    {
        PdfSeekableStream.MaximumStreamSize = 4;
        try
        {
            Assert.Throws<PdfResourceLimitExceededException>(() =>
                PdfSeekableStream.Create(new MemoryStream(new byte[5])));
        }
        finally
        {
            PdfSeekableStream.MaximumStreamSize = null;
        }
    }

    [Fact]
    public void SeekableStreamSupportsUnread()
    {
        using var source = PdfSeekableStream.Create(new MemoryStream(new byte[] { 10, 20 }));

        Assert.Equal(10, source.ReadByte());
        source.Unread();
        Assert.Equal(10, source.ReadByte());
    }

    [Fact]
    public void TraditionalXrefOpensAndResolvesIndirectObject()
    {
        var prefix = "%PDF-1.7\n";
        var objectOne = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n";
        var objectTwo = "2 0 obj\n<< /Type /Pages /Count 0 /Kids [] >>\nendobj\n";
        var objectOneOffset = Encoding.ASCII.GetByteCount(prefix);
        var objectTwoOffset = objectOneOffset + Encoding.ASCII.GetByteCount(objectOne);
        var xrefOffset = objectTwoOffset + Encoding.ASCII.GetByteCount(objectTwo);
        var pdf =
            prefix +
            objectOne +
            objectTwo +
            "xref\n0 3\n" +
            "0000000000 65535 f \n" +
            $"{objectOneOffset:0000000000} 00000 n \n" +
            $"{objectTwoOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 3 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        using var reader = PdfDocumentReader.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)), leaveOpen: false);

        Assert.Equal(1.7f, reader.PdfVersion);
        Assert.Equal(3, reader.Keys.Count);
        var objectValue = reader.Resolve(new CosKey(1, 0));
        var indirect = Assert.IsType<CosIndirectObject>(objectValue);
        var dictionary = Assert.IsType<CosDictionary>(indirect.Value);
        Assert.True(dictionary.TryGetValue("Type", out var type));
        Assert.Equal("Catalog", Assert.IsType<CosName>(type).Text);
    }

    [Fact]
    public void XrefStreamReadsEntriesAndTrailer()
    {
        var prefix = "%PDF-1.7\n";
        var objectOne = "1 0 obj\n<< /Name /Example >>\nendobj\n";
        var objectOneOffset = Encoding.ASCII.GetByteCount(prefix);
        var xrefObjectOffset = objectOneOffset + Encoding.ASCII.GetByteCount(objectOne);
        var xrefHeader =
            "5 0 obj\n<< /Type /XRef /Size 2 /Root 1 0 R /W [1 4 2] /Index [0 2] /Length 14 >>\n";

        var raw = new byte[14];
        raw[0] = 0;
        for (var i = 1; i <= 4; i++) raw[i] = 0;
        raw[5] = 255; raw[6] = 255;

        raw[7] = 1;
        raw[8] = (byte)(objectOneOffset >> 24);
        raw[9] = (byte)(objectOneOffset >> 16);
        raw[10] = (byte)(objectOneOffset >> 8);
        raw[11] = (byte)objectOneOffset;
        raw[12] = 0;
        raw[13] = 0;

        var pdf =
            prefix +
            objectOne +
            xrefHeader +
            "stream\n" +
            Encoding.Latin1.GetString(raw) +
            "\nendstream\nendobj\n" +
            $"startxref\n{xrefObjectOffset}\n%%EOF";

        using var reader = PdfDocumentReader.Open(
            new MemoryStream(Encoding.Latin1.GetBytes(pdf)), leaveOpen: false);

        Assert.Equal(2, reader.Keys.Count);
        Assert.True(reader.Trailer.TryGetValue("Root", out var root));
        Assert.Equal(1, Assert.IsType<CosIndirectReference>(root).ObjectNumber);

        var objectValue = reader.Resolve(new CosKey(1, 0));
        var indirect = Assert.IsType<CosIndirectObject>(objectValue);
        var dictionary = Assert.IsType<CosDictionary>(indirect.Value);
        Assert.True(dictionary.TryGetValue("Name", out var name));
        Assert.Equal("Example", Assert.IsType<CosName>(name).Text);
    }

    [Fact]
    public void IncrementalRevisionsMergeOldAndNewObjects()
    {
        var prefix = "%PDF-1.7\n";
        var objectOne = "1 0 obj\n<< /Name /Old >>\nendobj\n";
        var firstObjectOffset = Encoding.ASCII.GetByteCount(prefix);
        var firstXrefOffset = firstObjectOffset + Encoding.ASCII.GetByteCount(objectOne);
        var first =
            prefix +
            objectOne +
            "xref\n0 2\n" +
            "0000000000 65535 f \n" +
            $"{firstObjectOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 2 /Root 1 0 R >>\n" +
            $"startxref\n{firstXrefOffset}\n%%EOF\n";

        var secondObject = "2 0 obj\n<< /Name /New >>\nendobj\n";
        var secondObjectOffset = Encoding.ASCII.GetByteCount(first) + Encoding.ASCII.GetByteCount(secondObject) -
            Encoding.ASCII.GetByteCount(secondObject);
        secondObjectOffset = Encoding.ASCII.GetByteCount(first);
        var secondXrefOffset = secondObjectOffset + Encoding.ASCII.GetByteCount(secondObject);
        var pdf =
            first +
            secondObject +
            "xref\n0 3\n" +
            "0000000000 65535 f \n" +
            $"{firstObjectOffset:0000000000} 00000 n \n" +
            $"{secondObjectOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 3 /Root 1 0 R /Prev " +
            $"{firstXrefOffset} >>\n" +
            $"startxref\n{secondXrefOffset}\n%%EOF";

        using var reader = PdfDocumentReader.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)), leaveOpen: false);

        Assert.Equal(3, reader.Keys.Count);
        var oldObject = Assert.IsType<CosIndirectObject>(reader.Resolve(new CosKey(1, 0)));
        var newObject = Assert.IsType<CosIndirectObject>(reader.Resolve(new CosKey(2, 0)));
        Assert.True(((CosDictionary)oldObject.Value).TryGetValue("Name", out var oldName));
        Assert.True(((CosDictionary)newObject.Value).TryGetValue("Name", out var newName));
        Assert.Equal("Old", Assert.IsType<CosName>(oldName).Text);
        Assert.Equal("New", Assert.IsType<CosName>(newName).Text);
    }

    [Fact]
    public void CrossReferenceLoopIsRejected()
    {
        var prefix = "%PDF-1.7\n";
        var xrefOffset = Encoding.ASCII.GetByteCount(prefix);
        var pdf = "%PDF-1.7\n" +
            "xref\n0 1\n0000000000 65535 f \n" +
            $"trailer\n<< /Size 1 /Prev {xrefOffset} >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        var exception = Assert.Throws<IOException>(() =>
            PdfDocumentReader.Open(new MemoryStream(Encoding.ASCII.GetBytes(pdf))));
        Assert.Contains("loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsciiHexFilterDecodesWhitespaceAndOddNibble()
    {
        var data = Encoding.ASCII.GetBytes("41 0A");
        var decoded = PdfStreamFilterService.DecodeAsciiHex(data);
        Assert.Equal(new byte[] { 0x41, 0x0a }, decoded);
    }

    [Fact]
    public void Ascii85FilterDecodesKnownGroup()
    {
        var decoded = PdfStreamFilterService.DecodeAscii85(Encoding.ASCII.GetBytes("z"));
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, decoded);
    }

    [Fact]
    public void RunLengthFilterHandlesLiteralAndRepeatRuns()
    {
        var encoded = new byte[] { 2, (byte)'A', (byte)'B', (byte)'C', 254, (byte)'Z', 128 };
        var decoded = PdfStreamFilterService.DecodeRunLength(encoded);
        Assert.Equal(Encoding.ASCII.GetBytes("ABCZZZ"), decoded);
    }

    [Fact]
    public void FlateFilterRoundTripsContent()
    {
        var original = Encoding.ASCII.GetBytes("verapdf flate test");
        using var input = new MemoryStream();
        using (var zlib = new System.IO.Compression.ZLibStream(input, System.IO.Compression.CompressionLevel.SmallestSize, true))
        {
            zlib.Write(original);
        }
        var encoded = input.ToArray();

        Assert.Equal(original, new PdfStreamFilterService().DecodeFlate(encoded));
    }

    [Fact]
    public void PngPredictorDecodesSubRows()
    {
        var parameters = new CosDictionary();
        parameters.Set("Predictor", new CosInteger(11));
        parameters.Set("Colors", new CosInteger(1));
        parameters.Set("BitsPerComponent", new CosInteger(8));
        parameters.Set("Columns", new CosInteger(3));

        var streamParameters = new CosDictionary();
        streamParameters.Set("DecodeParms", parameters);
        var stream = new CosStream(streamParameters, new byte[] { 1, 10, 20, 30 });
        var decoded = new PdfStreamFilterService().Decode(stream);

        Assert.Equal(new byte[] { 10, 30, 60 }, decoded);
    }

    [Fact]
    public void DecodedStreamLimitIsRejected()
    {
        var streamParameters = new CosDictionary();
        var stream = new CosStream(streamParameters, new byte[] { 1, 2, 3, 4, 5 });
        var service = new PdfStreamFilterService(new PdfStreamDecodeOptions
        {
            MaximumDecodedBytes = 4
        });

        Assert.Throws<PdfResourceLimitExceededException>(() => service.Decode(stream));
    }

    [Fact]
    public void PdfDocumentExposesCatalogPagesResourcesAndContents()
    {
        var prefix = "%PDF-1.7\n";
        var catalog = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R /Metadata 6 0 R >>\nendobj\n";
        var pageTree = "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 /MediaBox [0 0 612 792] /Resources 4 0 R >>\nendobj\n";
        var page = "3 0 obj\n<< /Type /Page /Parent 2 0 R /Contents 5 0 R /Annots [7 0 R] >>\nendobj\n";
        var resources = "4 0 obj\n<< /Font << /F1 8 0 R >> /XObject << /Im1 9 0 R >> /ColorSpace << /CS1 10 0 R >> >>\nendobj\n";
        var contents = "5 0 obj\n<< /Length 12 >>\nstream\nHello World!\nendstream\nendobj\n";
        var metadata = "6 0 obj\n<< /Type /Metadata /Subtype /XML /Length 6 >>\nstream\n<xml/>\nendstream\nendobj\n";
        var annotation = "7 0 obj\n<< /Type /Annot /Subtype /Text /Contents (Note) >>\nendobj\n";
        var font = "8 0 obj\n<< /Type /Font /Subtype /Type1 >>\nendobj\n";
        var image = "9 0 obj\n<< /Type /XObject /Subtype /Image /Length 0 >>\nstream\n\nendstream\nendobj\n";
        var color = "10 0 obj\n[/ICCBased 11 0 R]\nendobj\n";
        var profile = "11 0 obj\n<< /N 3 >>\nendobj\n";

        var sections = new[] { catalog, pageTree, page, resources, contents, metadata, annotation, font, image, color, profile };
        var offsets = new long[12];
        var position = Encoding.ASCII.GetByteCount(prefix);
        for (var i = 0; i < sections.Length; i++)
        {
            offsets[i + 1] = position;
            position += Encoding.ASCII.GetByteCount(sections[i]);
        }

        var pdf = prefix + string.Concat(sections);
        var xrefOffset = position;
        pdf += "xref\n0 12\n" +
            "0000000000 65535 f \n";
        for (var i = 1; i <= 11; i++)
        {
            pdf += $"{offsets[i]:0000000000} 00000 n \n";
        }
        pdf += "trailer\n<< /Size 12 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        using var document = PdfDocument.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)), leaveOpen: false);

        Assert.Equal(1, document.PageCount);
        var pageObject = document.GetPage(0);
        Assert.Equal(new double[] { 0, 0, 612, 792 }, pageObject.GetMediaBox(document));
        Assert.Equal(new double[] { 0, 0, 612, 792 }, pageObject.GetCropBox(document));

        var resourcesObject = pageObject.GetResources(document);
        Assert.NotNull(resourcesObject);
        Assert.Contains("F1", resourcesObject.GetFontNames());
        Assert.Contains("Im1", resourcesObject.GetXObjectNames());
        Assert.Contains("CS1", resourcesObject.GetColorSpaceNames());

        var content = Assert.Single(pageObject.GetContents(document));
        Assert.Equal("Hello World!", Encoding.ASCII.GetString(content.GetDecodedData()));
        Assert.Single(pageObject.GetAnnotations(document));

        var metadataObject = document.Catalog.Metadata(document);
        Assert.NotNull(metadataObject);
        Assert.Equal("<xml/>", Encoding.ASCII.GetString(metadataObject.GetDecodedData()));
    }

    [Fact]
    public void PageTreeCycleIsRejected()
    {
        var prefix = "%PDF-1.7\n";
        var catalog = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n";
        var pageTree = "2 0 obj\n<< /Type /Pages /Kids [2 0 R] /Count 1 >>\nendobj\n";
        var pdf = prefix + catalog + pageTree;
        var xrefOffset = Encoding.ASCII.GetByteCount(pdf);
        pdf += "xref\n0 3\n" +
            "0000000000 65535 f \n" +
            $"{Encoding.ASCII.GetByteCount(prefix):0000000000} 00000 n \n" +
            $"{Encoding.ASCII.GetByteCount(prefix + catalog):0000000000} 00000 n \n" +
            "trailer\n<< /Size 3 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        var document = PdfDocument.Open(new MemoryStream(Encoding.ASCII.GetBytes(pdf)));
        Assert.Throws<IOException>(() => document.Pages);
        document.Dispose();
    }

    [Fact]
    public void PdfDocumentFullRewriteReopens()
    {
        var prefix = "%PDF-1.7\n";
        var catalog = "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n";
        var pages = "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 /MediaBox [0 0 612 792] >>\nendobj\n";
        var page = "3 0 obj\n<< /Type /Page /Parent 2 0 R >>\nendobj\n";
        var catalogOffset = Encoding.ASCII.GetByteCount(prefix);
        var pagesOffset = catalogOffset + Encoding.ASCII.GetByteCount(catalog);
        var pageOffset = pagesOffset + Encoding.ASCII.GetByteCount(pages);
        var xrefOffset = pageOffset + Encoding.ASCII.GetByteCount(page);
        var pdf = prefix + catalog + pages + page +
            "xref\n0 4\n" +
            "0000000000 65535 f \n" +
            $"{catalogOffset:0000000000} 00000 n \n" +
            $"{pagesOffset:0000000000} 00000 n \n" +
            $"{pageOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 4 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        using var document = PdfDocument.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)), leaveOpen: false);
        using var saved = new MemoryStream();
        document.SaveAs(saved);
        saved.Position = 0;

        using var reopened = PdfDocument.Open(saved, leaveOpen: true);
        Assert.Equal(1, reopened.PageCount);
        Assert.Equal(new double[] { 0, 0, 612, 792 },
            reopened.GetPage(0).GetMediaBox(reopened));
    }

    [Fact]
    public void CMapParserReadsCodeSpacesCidAndUnicodeMappings()
    {
        var text = Encoding.ASCII.GetBytes(
            "/WMode 1\n" +
            "2 begincodespacerange\n<00><FF>\n<20><20>\nendcodespacerange\n" +
            "2 beginbfchar\n<41><0041>\n<42><0042>\nendbfchar\n" +
            "1 beginbfrange\n<43><45><0043>\nendbfrange");
        var cmap = new CMapParser().Parse(text);

        Assert.Equal(1, cmap.WritingMode);
        Assert.Equal(2, cmap.CodeRanges.Count);
        Assert.Equal("A", cmap.ToUnicode(0x41));
        Assert.Equal("C", cmap.ToUnicode(0x43));
    }

    [Fact]
    public void StandardFontMetricsLoadsEmbeddedHelveticaAfm()
    {
        var metrics = StandardFontMetrics.Load("Helvetica");
        Assert.NotNull(metrics);
        Assert.Equal(278, metrics.Widths.GetValueOrDefault("space"));
        Assert.NotNull(StandardFontMetrics.Load("Helvetica-Bold"));
    }

    [Fact]
    public void TrueTypeParserReadsFormatFourCmap()
    {
        var font = new byte[160];
        font[4] = 0;
        font[5] = 1;
        Encoding.ASCII.GetBytes("cmap").CopyTo(font, 12);
        font[12 + 8] = 0;
        font[12 + 9] = 0;
        font[12 + 10] = 0;
        font[12 + 11] = 100;

        var offset = 100;
        font[offset] = 0;
        font[offset + 1] = 0;
        font[offset + 2] = 0;
        font[offset + 3] = 1;
        font[offset + 4] = 0;
        font[offset + 5] = 3;
        font[offset + 6] = 0;
        font[offset + 7] = 1;
        font[offset + 8] = 0;
        font[offset + 9] = 0;
        font[offset + 10] = 0;
        font[offset + 11] = 0;
        WriteUInt16(font, offset + 4, 3);
        WriteUInt16(font, offset + 6, 1);
        WriteUInt32(font, offset + 8, 12);

        var sub = offset + 12;
        WriteUInt16(font, sub, 4);
        WriteUInt16(font, sub + 2, 32);
        WriteUInt16(font, sub + 4, 0);
        WriteUInt16(font, sub + 6, 4);
        var end = sub + 14;
        WriteUInt16(font, end, 0x41);
        WriteUInt16(font, end + 2, 0x42);
        var start = end + 6;
        WriteUInt16(font, start, 0x41);
        WriteUInt16(font, start + 2, 0x42);
        var delta = start + 4;
        WriteUInt16(font, delta, 0);
        WriteUInt16(font, delta + 2, 1);

        var parsed = new TrueTypeFontInfo(font);
        Assert.Equal(0x41, parsed.CharacterToGlyph[0x41]);
    }

    private static void WriteUInt16(byte[] data, int offset, int value)
    {
        data[offset] = (byte)(value >> 8);
        data[offset + 1] = (byte)value;
    }

    private static void WriteUInt32(byte[] data, int offset, int value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }
}
