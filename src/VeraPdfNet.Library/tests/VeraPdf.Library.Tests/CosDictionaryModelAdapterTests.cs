using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Model.PDLayer;
using VeraPdf.Model.XMPLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosDictionaryModelAdapterTests
{
    [Fact]
    public void ExposesSizeKeysAndSupportedValueLinks()
    {
        var value = new CosDictionary();
        value.Set("Type", new CosName("Catalog"u8.ToArray()));
        value.Set("Length", new CosInteger(5));
        value.Set("Null", new CosNull());

        var adapted = CosDictionaryModelAdapter.FromParserValue(value);

        Assert.Equal(3, adapted.Getsize());
        Assert.Equal("Type&Length&Null", adapted.GetkeysString());
        Assert.Collection(
            adapted.GetLinkedObjects("keys"),
            key => Assert.Equal("Type", Assert.IsAssignableFrom<ICosName>(key).GetinternalRepresentation()),
            key => Assert.Equal("Length", Assert.IsAssignableFrom<ICosName>(key).GetinternalRepresentation()),
            key => Assert.Equal("Null", Assert.IsAssignableFrom<ICosName>(key).GetinternalRepresentation()));
        Assert.Collection(
            adapted.GetLinkedObjects("values"),
            item => Assert.Equal("Catalog", Assert.IsAssignableFrom<ICosName>(item).GetinternalRepresentation()),
            item => Assert.Equal(5, Assert.IsAssignableFrom<ICosInteger>(item).GetintValue()),
            item => Assert.IsAssignableFrom<ICosNull>(item));
    }

    [Fact]
    public void PreservesSizeAndKeysWhenAValueIsMissing()
    {
        var value = new CosDictionary();
        value.Set("Missing", null!);

        var adapted = CosDictionaryModelAdapter.FromParserValue(value);

        Assert.Equal(1, adapted.Getsize());
        Assert.Equal("Missing", adapted.GetkeysString());
        Assert.Single(adapted.GetLinkedObjects("keys"));
        Assert.Empty(adapted.GetLinkedObjects("values"));
    }

    [Fact]
    public void SupportsArrayValuesAndMetadataLinks()
    {
        var value = new CosDictionary();
        var nestedArray = new CosArray();
        nestedArray.Add(new CosInteger(4));
        value.Set("Nested", nestedArray);
        var adapted = CosDictionaryModelAdapter.FromParserValue(value);

        var nested = Assert.IsAssignableFrom<ICosArray>(
            Assert.Single(adapted.GetLinkedObjects("values")));
        Assert.Equal(
            4,
            Assert.IsAssignableFrom<ICosInteger>(
                Assert.Single(nested.GetLinkedObjects("elements"))).GetintValue());
        Assert.Empty(adapted.GetLinkedObjects("metadata"));
        Assert.Throws<InvalidOperationException>(() => adapted.GetLinkedObjects("unknown"));
    }

    [Fact]
    public void ExposesNonCatalogXmlMetadataStream()
    {
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        metadataDictionary.Set("Filter", new CosName("FlateDecode"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary, [1, 2, 3]);
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);

        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));

        Assert.Equal("FlateDecode", metadata.GetFilter());
        Assert.False(metadata.GetisCatalogMetadata());
        Assert.Equal("Type&Subtype&Filter", metadata.Getentries());
        var stream = Assert.IsAssignableFrom<ICosStream>(
            Assert.Single(metadata.GetLinkedObjects("stream")));
        Assert.Equal(3, stream.GetrealLength());
        Assert.False(Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage"))).GetisSerializationValid());
    }

    [Theory]
    [InlineData("<rdf:Seq><rdf:li>2</rdf:li><rdf:li>1</rdf:li></rdf:Seq>", true)]
    [InlineData("<rdf:Seq><rdf:li>2</rdf:li><rdf:li>2</rdf:li></rdf:Seq>", true)]
    [InlineData("<rdf:Seq><rdf:li>2</rdf:li><rdf:li>3</rdf:li></rdf:Seq>", null)]
    [InlineData("<rdf:Seq><rdf:li>2</rdf:li><rdf:li>x</rdf:li></rdf:Seq>", false)]
    [InlineData("<rdf:Bag><rdf:li>2</rdf:li><rdf:li>1</rdf:li></rdf:Bag>", false)]
    public void PreservesClosedChoiceUncertaintyForTiffSubsampling(
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<tiff:YCbCrSubSampling>{value}</tiff:YCbCrSubSampling>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("seq integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("YCbCrPositioning", "1", true)]
    [InlineData("YCbCrPositioning", "2", true)]
    [InlineData("YCbCrPositioning", "3", null)]
    [InlineData("YCbCrPositioning", "4", null)]
    [InlineData("YCbCrPositioning", "x", false)]
    [InlineData("ResolutionUnit", "2", true)]
    [InlineData("ResolutionUnit", "3", true)]
    [InlineData("ResolutionUnit", "1", null)]
    [InlineData("ResolutionUnit", "4", null)]
    [InlineData("ResolutionUnit", "x", false)]
    public void PreservesClosedChoiceUncertaintyForTiffIntegerProperties(
        string propertyName,
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<tiff:{propertyName}>{value}</tiff:{propertyName}>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("Compression", "1", true)]
    [InlineData("Compression", "6", true)]
    [InlineData("Compression", "2", null)]
    [InlineData("Compression", "x", false)]
    [InlineData("PhotometricInterpretation", "2", true)]
    [InlineData("PhotometricInterpretation", "6", true)]
    [InlineData("PhotometricInterpretation", "1", null)]
    [InlineData("PhotometricInterpretation", "x", false)]
    [InlineData("Orientation", "1", true)]
    [InlineData("Orientation", "4", true)]
    [InlineData("Orientation", "8", true)]
    [InlineData("Orientation", "9", null)]
    [InlineData("Orientation", "x", false)]
    [InlineData("PlanarConfiguration", "1", true)]
    [InlineData("PlanarConfiguration", "2", true)]
    [InlineData("PlanarConfiguration", "3", null)]
    [InlineData("PlanarConfiguration", "x", false)]
    public void PreservesClosedChoiceUncertaintyForTiffRestrictedIntegers(
        string propertyName,
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<tiff:{propertyName}>{value}</tiff:{propertyName}>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("ExposureProgram", "0", true)]
    [InlineData("ExposureProgram", "8", true)]
    [InlineData("ExposureProgram", "9", null)]
    [InlineData("ExposureProgram", "x", false)]
    [InlineData("MeteringMode", "0", true)]
    [InlineData("MeteringMode", "6", true)]
    [InlineData("MeteringMode", "255", true)]
    [InlineData("MeteringMode", "7", null)]
    [InlineData("MeteringMode", "254", null)]
    [InlineData("MeteringMode", "x", false)]
    [InlineData("FocalPlaneResolutionUnit", "2", true)]
    [InlineData("FocalPlaneResolutionUnit", "3", true)]
    [InlineData("FocalPlaneResolutionUnit", "1", null)]
    [InlineData("FocalPlaneResolutionUnit", "x", false)]
    [InlineData("SensingMethod", "1", true)]
    [InlineData("SensingMethod", "5", true)]
    [InlineData("SensingMethod", "8", true)]
    [InlineData("SensingMethod", "9", null)]
    [InlineData("SensingMethod", "x", false)]
    public void PreservesClosedChoiceUncertaintyForExifIntegerProperties(
        string propertyName,
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<exif:{propertyName}>{value}</exif:{propertyName}>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("FileSource", "3", true)]
    [InlineData("FileSource", "4", null)]
    [InlineData("FileSource", "x", false)]
    [InlineData("SceneType", "1", true)]
    [InlineData("SceneType", "0", null)]
    [InlineData("SceneType", "x", false)]
    [InlineData("CustomRendered", "0", true)]
    [InlineData("CustomRendered", "1", true)]
    [InlineData("CustomRendered", "2", null)]
    [InlineData("CustomRendered", "x", false)]
    [InlineData("ExposureMode", "0", true)]
    [InlineData("ExposureMode", "2", true)]
    [InlineData("ExposureMode", "3", null)]
    [InlineData("ExposureMode", "x", false)]
    public void PreservesClosedChoiceUncertaintyForExifIntegerPropertiesPartTwo(
        string propertyName,
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<exif:{propertyName}>{value}</exif:{propertyName}>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<rdf:Seq><rdf:li>4</rdf:li><rdf:li>5</rdf:li><rdf:li>6</rdf:li><rdf:li>0</rdf:li></rdf:Seq>", true)]
    [InlineData("<rdf:Seq><rdf:li>1</rdf:li><rdf:li>2</rdf:li><rdf:li>3</rdf:li><rdf:li>0</rdf:li></rdf:Seq>", true)]
    [InlineData("<rdf:Seq><rdf:li>1</rdf:li><rdf:li>2</rdf:li><rdf:li>3</rdf:li></rdf:Seq>", null)]
    [InlineData("<rdf:Seq><rdf:li>1</rdf:li><rdf:li>x</rdf:li></rdf:Seq>", false)]
    [InlineData("<rdf:Bag><rdf:li>4</rdf:li><rdf:li>5</rdf:li><rdf:li>6</rdf:li><rdf:li>0</rdf:li></rdf:Bag>", false)]
    public void PreservesClosedChoiceUncertaintyForExifComponentsConfiguration(
        string value,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(
                xmpPrefix + $"<exif:ComponentsConfiguration>{value}</exif:ComponentsConfiguration>" +
                xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var property = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal("seq integer", property.GetpredefinedType());
        Assert.Equal(expected, property.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("text/plain; charset=utf-8", false)]
    [InlineData("application/\u00E9", false)]
    public void ValidatesPredefinedMimeTypeSyntax(string value, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:dc=\"http://purl.org/dc/elements/1.1/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix + $"<dc:format>{value}</dc:format>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var format = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, format.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stDim:w>wide</stDim:w>", false)]
    [InlineData("<other:w>8.5</other:w>", false)]
    public void RejectsInvalidPredefinedDimensionsFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpTPg=\"http://ns.adobe.com/xap/1.0/t/pg/\" " +
            "xmlns:stDim=\"http://ns.adobe.com/xap/1.0/sType/Dimensions#\" " +
            "xmlns:other=\"https://example.test/other#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               $"<xmpTPg:MaxPageSize rdf:parseType=\"Resource\">{fields}</xmpTPg:MaxPageSize>" +
                                               xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var maxPageSize = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, maxPageSize.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stJob:unknown>value</stJob:unknown>", false)]
    [InlineData("<stJob:name><rdf:Description/></stJob:name>", false)]
    public void RejectsInvalidPredefinedJobFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpBJ=\"http://ns.adobe.com/xap/1.0/bj/\" " +
            "xmlns:stJob=\"http://ns.adobe.com/xap/1.0/sType/Job#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpBJ:JobRef><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Bag></xmpBJ:JobRef>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var jobRef = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, jobRef.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stFnt:unknown>value</stFnt:unknown>", false)]
    [InlineData("<stFnt:composite>yes</stFnt:composite>", false)]
    public void RejectsInvalidPredefinedFontFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpTPg=\"http://ns.adobe.com/xap/1.0/t/pg/\" " +
            "xmlns:stFnt=\"http://ns.adobe.com/xap/1.0/sType/Font#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpTPg:Fonts><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Bag></xmpTPg:Fonts>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var fonts = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, fonts.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stEvt:unknown>value</stEvt:unknown>", false)]
    [InlineData("<stEvt:when>not-a-date</stEvt:when>", false)]
    public void RejectsInvalidPredefinedResourceEventFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" " +
            "xmlns:stEvt=\"http://ns.adobe.com/xap/1.0/sType/ResourceEvent#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpMM:History><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Seq></xmpMM:History>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var history = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, history.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stVer:unknown>value</stVer:unknown>", false)]
    [InlineData("<stVer:event rdf:parseType=\"Resource\"><stEvt:unknown>value</stEvt:unknown></stVer:event>", false)]
    public void RejectsInvalidPredefinedVersionFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" " +
            "xmlns:stVer=\"http://ns.adobe.com/xap/1.0/sType/Version#\" " +
            "xmlns:stEvt=\"http://ns.adobe.com/xap/1.0/sType/ResourceEvent#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpMM:Versions><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Seq></xmpMM:Versions>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var versions = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, versions.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stRef:unknown>value</stRef:unknown>", false)]
    [InlineData("<stRef:instanceID><rdf:Description/></stRef:instanceID>", false)]
    public void RejectsInvalidPredefinedResourceReferenceFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" " +
            "xmlns:stRef=\"http://ns.adobe.com/xap/1.0/sType/ResourceRef#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpMM:DerivedFrom rdf:parseType=\"Resource\">" + fields +
                                               "</xmpMM:DerivedFrom>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var derivedFrom = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, derivedFrom.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<stImg:format>PNG</stImg:format>", null)]
    [InlineData("<stImg:unknown>value</stImg:unknown>", false)]
    public void PreservesProfileDependentThumbnailFormatResults(string fields, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" " +
            "xmlns:stImg=\"http://ns.adobe.com/xap/1.0/g/img/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmp:Thumbnails><rdf:Alt><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Alt></xmp:Thumbnails>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var thumbnails = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, thumbnails.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:value>not-an-integer</xmpDM:value>", false)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    public void RejectsInvalidPredefinedTimeFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:duration rdf:parseType=\"Resource\">" + fields +
                                               "</xmpDM:duration>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var duration = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, duration.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    [InlineData("<xmpDM:managed>yes</xmpDM:managed>", false)]
    public void RejectsInvalidPredefinedMediaFields(string fields, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:contributedMedia><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" + fields +
                                               "</rdf:li></rdf:Bag></xmpDM:contributedMedia>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var contributedMedia = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, contributedMedia.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedBeatSpliceStretchFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<xmpDM:useFileBeatsMarker>True</xmpDM:useFileBeatsMarker>" +
            "<xmpDM:riseInDecibel>1.5</xmpDM:riseInDecibel>" +
            "<xmpDM:riseInTimeDuration rdf:parseType=\"Resource\">" +
            "<xmpDM:value>1</xmpDM:value><xmpDM:scale>1</xmpDM:scale>" +
            "</xmpDM:riseInTimeDuration>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:beatSpliceParams rdf:parseType=\"Resource\">" + fields +
                                               "</xmpDM:beatSpliceParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var beatSpliceParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(beatSpliceParams.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>")]
    [InlineData("<xmpDM:useFileBeatsMarker>yes</xmpDM:useFileBeatsMarker>")]
    [InlineData("<xmpDM:riseInDecibel>not-a-real</xmpDM:riseInDecibel>")]
    public void RejectsInvalidPredefinedBeatSpliceStretchFields(string field)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:beatSpliceParams rdf:parseType=\"Resource\">" + field +
                                               "</xmpDM:beatSpliceParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var beatSpliceParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.False(beatSpliceParams.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedTimeScaleStretchFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<xmpDM:frameSize>29.97</xmpDM:frameSize>" +
            "<xmpDM:frameOverlappingPercentage>10</xmpDM:frameOverlappingPercentage>" +
            "<xmpDM:quality>High</xmpDM:quality>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:timeScaleParams rdf:parseType=\"Resource\">" + fields +
                                               "</xmpDM:timeScaleParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var timeScaleParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(timeScaleParams.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:quality>Other</xmpDM:quality>", null)]
    [InlineData("<xmpDM:frameSize>not-a-real</xmpDM:frameSize>", false)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    public void HandlesInvalidOrProfileDependentTimeScaleStretchFields(
        string field,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:timeScaleParams rdf:parseType=\"Resource\">" + field +
                                               "</xmpDM:timeScaleParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var timeScaleParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, timeScaleParams.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedResampleStretchFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:resampleParams rdf:parseType=\"Resource\">" +
                                               "<xmpDM:quality>Medium</xmpDM:quality>" +
                                               "</xmpDM:resampleParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var resampleParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(resampleParams.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:quality>Ultra</xmpDM:quality>", null)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    [InlineData("<xmpDM:quality><rdf:Description/></xmpDM:quality>", false)]
    public void HandlesInvalidOrProfileDependentResampleStretchFields(
        string field,
        bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:resampleParams rdf:parseType=\"Resource\">" + field +
                                               "</xmpDM:resampleParams>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var resampleParams = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, resampleParams.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedMarkerFieldsInDynamicMediaArray()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string marker =
            "<rdf:li rdf:parseType=\"Resource\">" +
            "<xmpDM:startTime><rdf:Description/></xmpDM:startTime>" +
            "<xmpDM:duration>arbitrary value</xmpDM:duration>" +
            "<xmpDM:comment>Chapter note</xmpDM:comment>" +
            "<xmpDM:name>Opening</xmpDM:name>" +
            "<xmpDM:location>https://example.test/clip</xmpDM:location>" +
            "<xmpDM:target>sequence-1</xmpDM:target>" +
            "<xmpDM:type>Chapter</xmpDM:type>" +
            "</rdf:li>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:markers><rdf:Seq>" + marker +
                                               "</rdf:Seq></xmpDM:markers>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var markers = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(markers.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:type>Custom</xmpDM:type>", null)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    [InlineData("<other:comment>value</other:comment>", false)]
    public void HandlesInvalidOrProfileDependentMarkerFields(string field, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\" " +
            "xmlns:other=\"https://example.test/other#\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:markers><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" + field +
                                               "</rdf:li></rdf:Seq></xmpDM:markers>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var markers = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, markers.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedProjectLinkFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<xmpDM:path>https://example.test/project.mov</xmpDM:path>" +
            "<xmpDM:type>movie</xmpDM:type>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:projectRef rdf:parseType=\"Resource\">" + fields +
                                               "</xmpDM:projectRef>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var projectRef = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(projectRef.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:type>other</xmpDM:type>", null)]
    [InlineData("<xmpDM:path><rdf:Description/></xmpDM:path>", false)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    public void HandlesInvalidOrProfileDependentProjectLinkFields(string field, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:projectRef rdf:parseType=\"Resource\">" + field +
                                               "</xmpDM:projectRef>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var projectRef = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, projectRef.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedTimecodeFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<xmpDM:timeValue>01:02:03:04</xmpDM:timeValue>" +
            "<xmpDM:timeFormat>24Timecode</xmpDM:timeFormat>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:startTimecode rdf:parseType=\"Resource\">" + fields +
                                               "</xmpDM:startTimecode>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var startTimecode = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(startTimecode.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<xmpDM:timeValue>1:02:03:04</xmpDM:timeValue>", null)]
    [InlineData("<xmpDM:timeFormat>48Timecode</xmpDM:timeFormat>", null)]
    [InlineData("<xmpDM:unknown>value</xmpDM:unknown>", false)]
    public void HandlesInvalidOrProfileDependentTimecodeFields(string field, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpDM:startTimecode rdf:parseType=\"Resource\">" + field +
                                               "</xmpDM:startTimecode>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var startTimecode = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, startTimecode.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedColorantFieldsInPagedTextArray()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpTPg=\"http://ns.adobe.com/xap/1.0/t/pg/\" " +
            "xmlns:g=\"http://ns.adobe.com/xap/1.0/g/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string colorant =
            "<rdf:li rdf:parseType=\"Resource\">" +
            "<g:swatchName>Blue</g:swatchName><g:mode>RGB</g:mode><g:type>PROCESS</g:type>" +
            "<g:red>0</g:red><g:green>128</g:green><g:blue>255</g:blue>" +
            "</rdf:li>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpTPg:Colorants><rdf:Seq>" + colorant +
                                               "</rdf:Seq></xmpTPg:Colorants>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var colorants = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(colorants.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<g:mode>Gray</g:mode>", null)]
    [InlineData("<g:cyan>101</g:cyan>", null)]
    [InlineData("<g:cyan>not-a-real</g:cyan>", false)]
    [InlineData("<g:unknown>value</g:unknown>", false)]
    public void HandlesInvalidOrProfileDependentColorantFields(string field, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:xmpTPg=\"http://ns.adobe.com/xap/1.0/t/pg/\" " +
            "xmlns:g=\"http://ns.adobe.com/xap/1.0/g/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<xmpTPg:Colorants><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" + field +
                                               "</rdf:li></rdf:Seq></xmpTPg:Colorants>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var colorants = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, colorants.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesLocaleValuesInDcLanguageBag()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:dc=\"http://purl.org/dc/elements/1.1/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string language =
            "<dc:language><rdf:Bag><rdf:li>en-US</rdf:li><rdf:li>fr-CA</rdf:li></rdf:Bag></dc:language>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix + language + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var languageProperty = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(languageProperty.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<rdf:Seq><rdf:li>en-US</rdf:li></rdf:Seq>", false)]
    [InlineData("<rdf:Bag><rdf:li><rdf:Description/></rdf:li></rdf:Bag>", false)]
    public void RejectsInvalidLocaleArrays(string arrayValue, bool expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:dc=\"http://purl.org/dc/elements/1.1/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<dc:language>" + arrayValue + "</dc:language>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var languageProperty = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, languageProperty.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("12,34,56N", true)]
    [InlineData("1,2.5N", null)]
    [InlineData("invalid-coordinate", false)]
    public void PreservesProfileDependentGpsCoordinateResults(string coordinate, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               $"<exif:GPSLatitude>{coordinate}</exif:GPSLatitude>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var latitude = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, latitude.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedExifFlashFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<exif:Fired>True</exif:Fired><exif:Function>False</exif:Function>" +
            "<exif:RedEyeMode>False</exif:RedEyeMode>" +
            "<exif:Return>3</exif:Return><exif:Mode>1</exif:Mode>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:Flash rdf:parseType=\"Resource\">" + fields +
                                               "</exif:Flash>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var flash = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(flash.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<exif:Return>1</exif:Return>", null)]
    [InlineData("<exif:Fired>yes</exif:Fired>", false)]
    [InlineData("<exif:Unknown>value</exif:Unknown>", false)]
    public void HandlesInvalidOrProfileDependentExifFlashFields(string field, bool? expected)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:Flash rdf:parseType=\"Resource\">" + field +
                                               "</exif:Flash>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var flash = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.Equal(expected, flash.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedCfaPatternFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<exif:Columns>2</exif:Columns><exif:Rows>2</exif:Rows>" +
            "<exif:Values><rdf:Seq><rdf:li>0</rdf:li><rdf:li>1</rdf:li>" +
            "<rdf:li>1</rdf:li><rdf:li>2</rdf:li></rdf:Seq></exif:Values>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:CFAPattern rdf:parseType=\"Resource\">" + fields +
                                               "</exif:CFAPattern>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var cfaPattern = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(cfaPattern.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<exif:Rows>not-an-integer</exif:Rows>")]
    [InlineData("<exif:Values><rdf:Bag><rdf:li>0</rdf:li></rdf:Bag></exif:Values>")]
    [InlineData("<exif:Unknown>value</exif:Unknown>")]
    public void RejectsInvalidCfaPatternFields(string field)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:CFAPattern rdf:parseType=\"Resource\">" + field +
                                               "</exif:CFAPattern>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var cfaPattern = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.False(cfaPattern.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedOecfSfrFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<exif:Columns>2</exif:Columns><exif:Rows>2</exif:Rows>" +
            "<exif:Names><rdf:Seq><rdf:li>Red</rdf:li><rdf:li>Green</rdf:li></rdf:Seq></exif:Names>" +
            "<exif:Values><rdf:Seq><rdf:li>1/2</rdf:li><rdf:li>3/4</rdf:li></rdf:Seq></exif:Values>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:SpatialFrequencyResponse rdf:parseType=\"Resource\">" + fields +
                                               "</exif:SpatialFrequencyResponse>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var sfr = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(sfr.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<exif:Columns>not-an-integer</exif:Columns>")]
    [InlineData("<exif:Names><rdf:Bag><rdf:li>Red</rdf:li></rdf:Bag></exif:Names>")]
    [InlineData("<exif:Unknown>value</exif:Unknown>")]
    public void RejectsInvalidOecfSfrFields(string field)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:SpatialFrequencyResponse rdf:parseType=\"Resource\">" + field +
                                               "</exif:SpatialFrequencyResponse>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var sfr = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.False(sfr.GetisValueTypeCorrect());
    }

    [Fact]
    public void ValidatesPredefinedDeviceSettingsFields()
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string fields =
            "<exif:Columns>2</exif:Columns><exif:Rows>1</exif:Rows>" +
            "<exif:Settings><rdf:Seq><rdf:li>setting-a</rdf:li>" +
            "<rdf:li>setting-b</rdf:li></rdf:Seq></exif:Settings>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:DeviceSettingDescription rdf:parseType=\"Resource\">" + fields +
                                               "</exif:DeviceSettingDescription>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var deviceSettings = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.True(deviceSettings.GetisValueTypeCorrect());
    }

    [Theory]
    [InlineData("<exif:Rows>not-an-integer</exif:Rows>")]
    [InlineData("<exif:Settings><rdf:Bag><rdf:li>setting</rdf:li></rdf:Bag></exif:Settings>")]
    [InlineData("<exif:Unknown>value</exif:Unknown>")]
    public void RejectsInvalidDeviceSettingsFields(string field)
    {
        const string xmpPrefix =
            "<?xpacket begin=\"\uFEFF\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:exif=\"http://ns.adobe.com/exif/1.0/\">";
        const string xmpSuffix = "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmpPrefix +
                                               "<exif:DeviceSettingDescription rdf:parseType=\"Resource\">" + field +
                                               "</exif:DeviceSettingDescription>" + xmpSuffix));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));
        var deviceSettings = Assert.IsAssignableFrom<IXMPProperty>(
            Assert.Single(package.GetLinkedObjects("Properties")));

        Assert.False(deviceSettings.GetisValueTypeCorrect());
    }

    [Fact]
    public void ExposesXmpPackageHeaderAndRawProperties()
    {
        const string xmp = "<?xpacket begin=\"\uFEFF\" bytes=\"128\" encoding=\"UTF-8\"?>" +
                           "<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
                           "<rdf:Description xmlns:dc=\"http://purl.org/dc/elements/1.1/\" " +
                           "xmlns:xmp=\"http://ns.adobe.com/xap/1.0/\" " +
                           "xmlns:xmpRights=\"http://ns.adobe.com/xap/1.0/rights/\" " +
                           "xmlns:xmpMM=\"http://ns.adobe.com/xap/1.0/mm/\" " +
                           "xmlns:stEvt=\"http://ns.adobe.com/xap/1.0/sType/ResourceEvent#\" " +
                           "xmlns:stRef=\"http://ns.adobe.com/xap/1.0/sType/ResourceRef#\" " +
                           "xmlns:stVer=\"http://ns.adobe.com/xap/1.0/sType/Version#\" " +
                           "xmlns:xmpBJ=\"http://ns.adobe.com/xap/1.0/bj/\" " +
                           "xmlns:stJob=\"http://ns.adobe.com/xap/1.0/sType/Job#\" " +
                           "xmlns:stFnt=\"http://ns.adobe.com/xap/1.0/sType/Font#\" " +
                           "xmlns:stImg=\"http://ns.adobe.com/xap/1.0/g/img/\" " +
                           "xmlns:xmpTPg=\"http://ns.adobe.com/xap/1.0/t/pg/\" " +
                           "xmlns:stDim=\"http://ns.adobe.com/xap/1.0/sType/Dimensions#\" " +
                           "xmlns:pdfaid=\"http://www.aiim.org/pdfa/ns/id/\" " +
                           "xmlns:photoshop=\"http://ns.adobe.com/photoshop/1.0/\" " +
                           "xmlns:tiff=\"http://ns.adobe.com/tiff/1.0/\" " +
                           "xmlns:exif=\"http://ns.adobe.com/exif/1.0/\" " +
                           "xmlns:aux=\"http://ns.adobe.com/exif/1.0/aux/\" " +
                           "xmlns:crs=\"http://ns.adobe.com/camera-raw-settings/1.0/\" " +
                           "xmlns:xmpDM=\"http://ns.adobe.com/xmp/1.0/DynamicMedia/\" " +
                           "xmlns:pdf=\"http://ns.adobe.com/pdf/1.3/\">" +
                           "<dc:title><rdf:Alt><rdf:li xml:lang=\"x-default\">Example</rdf:li>" +
                           "<rdf:li xml:lang=\"fr\">Exemple</rdf:li></rdf:Alt></dc:title>" +
                           "<dc:description><rdf:Alt/></dc:description>" +
                           "<dc:rights><rdf:Alt><rdf:li>missing language</rdf:li></rdf:Alt></dc:rights>" +
                           "<dc:coverage><rdf:Seq><rdf:li>not scalar text</rdf:li></rdf:Seq></dc:coverage>" +
                           "<dc:creator><rdf:Seq><rdf:li>First author</rdf:li><rdf:li>Second author</rdf:li></rdf:Seq></dc:creator>" +
                           "<dc:subject><rdf:Seq><rdf:li>wrong array kind</rdf:li></rdf:Seq></dc:subject>" +
                           "<dc:relation><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
                           "<dc:title>Nested title</dc:title></rdf:li></rdf:Bag></dc:relation>" +
                           "<dc:publisher><rdf:Bag/></dc:publisher>" +
                           "<xmpRights:Marked>True</xmpRights:Marked>" +
                           "<xmpMM:DocumentID>urn:example:document</xmpMM:DocumentID>" +
                           "<xmpMM:DerivedFrom rdf:parseType=\"Resource\">" +
                           "<stRef:instanceID>urn:example:source-instance</stRef:instanceID>" +
                           "<stRef:documentID>urn:example:source-document</stRef:documentID>" +
                           "<stRef:versionID>1.0</stRef:versionID>" +
                           "<stRef:renditionClass>default</stRef:renditionClass>" +
                           "<stRef:renditionParams>quality=high</stRef:renditionParams>" +
                           "<stRef:manager>Example manager</stRef:manager>" +
                           "<stRef:managerVariant>Editor</stRef:managerVariant>" +
                           "<stRef:manageTo>urn:example:manage-to</stRef:manageTo>" +
                           "<stRef:manageUI>urn:example:manage-ui</stRef:manageUI>" +
                           "</xmpMM:DerivedFrom>" +
                           "<xmpMM:InstanceID><rdf:Seq><rdf:li>urn:example:instance</rdf:li></rdf:Seq></xmpMM:InstanceID>" +
                           "<xmpMM:History><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
                           "<stEvt:action>created</stEvt:action><stEvt:instanceID>urn:example:event</stEvt:instanceID>" +
                           "<stEvt:parameters>Initial import</stEvt:parameters><stEvt:softwareAgent>Example tool</stEvt:softwareAgent>" +
                           "<stEvt:when>2024-01-02T03:04:05Z</stEvt:when>" +
                           "</rdf:li></rdf:Seq></xmpMM:History>" +
                           "<xmpMM:Versions><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
                           "<stVer:comments>First version</stVer:comments>" +
                           "<stVer:event rdf:parseType=\"Resource\"><stEvt:action>saved</stEvt:action>" +
                           "<stEvt:when>2024-01-03T04:05:06Z</stEvt:when></stVer:event>" +
                           "<stVer:modifyDate>2024-01-03T04:05:06Z</stVer:modifyDate>" +
                           "<stVer:modifier>Example editor</stVer:modifier>" +
                           "<stVer:version>1.0</stVer:version>" +
                           "</rdf:li></rdf:Seq></xmpMM:Versions>" +
                           "<xmpMM:Manager>Example Manager</xmpMM:Manager>" +
                           "<xmpMM:RenditionClass>default</xmpMM:RenditionClass>" +
                           "<xmpBJ:JobRef><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
                           "<stJob:name>Example job</stJob:name><stJob:id>job-1</stJob:id>" +
                           "<stJob:url>https://example.test/job</stJob:url>" +
                           "</rdf:li></rdf:Bag></xmpBJ:JobRef>" +
                           "<xmpTPg:MaxPageSize rdf:parseType=\"Resource\"><stDim:w>8.5</stDim:w>" +
                           "<stDim:h>11</stDim:h><stDim:unit>inches</stDim:unit></xmpTPg:MaxPageSize>" +
                           "<xmpTPg:NPages>12.5</xmpTPg:NPages>" +
                           "<xmpTPg:Fonts><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
                           "<stFnt:fontName>Example Font</stFnt:fontName>" +
                           "<stFnt:fontFamily>Example</stFnt:fontFamily>" +
                           "<stFnt:fontFace>Regular</stFnt:fontFace>" +
                           "<stFnt:fontType>TrueType</stFnt:fontType>" +
                           "<stFnt:versionString>1.0</stFnt:versionString>" +
                           "<stFnt:composite>False</stFnt:composite>" +
                           "<stFnt:fontFileName>font.ttf</stFnt:fontFileName>" +
                           "<stFnt:childFontFiles><rdf:Seq><rdf:li>child.otf</rdf:li></rdf:Seq></stFnt:childFontFiles>" +
                           "</rdf:li></rdf:Bag></xmpTPg:Fonts>" +
                           "<xmpTPg:Colorants>Example colorant</xmpTPg:Colorants>" +
                           "<xmpTPg:PlateNames>Example plate</xmpTPg:PlateNames>" +
                           "<pdfaid:part>2</pdfaid:part><pdfaid:amd>Example amendment</pdfaid:amd>" +
                           "<pdfaid:conformance>A</pdfaid:conformance><pdfaid:corr>Example correction</pdfaid:corr>" +
                           "<xmp:BaseURL>not a URL</xmp:BaseURL>" +
                           "<xmp:CreateDate>2024-02</xmp:CreateDate>" +
                           "<xmp:ModifyDate>2024-01-02T03:04:05Z</xmp:ModifyDate>" +
                           "<xmp:MetadataDate>not a date</xmp:MetadataDate>" +
                           "<xmp:Label>Example label</xmp:Label><xmp:Rating>4</xmp:Rating>" +
                           "<xmp:Thumbnails><rdf:Alt><rdf:li rdf:parseType=\"Resource\">" +
                           "<stImg:height>100</stImg:height><stImg:width>200</stImg:width>" +
                           "<stImg:image>base64data</stImg:image><stImg:format>JPEG</stImg:format>" +
                           "</rdf:li></rdf:Alt></xmp:Thumbnails>" +
                           "<photoshop:Headline>Example headline</photoshop:Headline>" +
                           "<photoshop:SupplementalCategories>Example category</photoshop:SupplementalCategories>" +
                           "<tiff:ImageWidth>640</tiff:ImageWidth><tiff:Artist>Example Artist</tiff:Artist>" +
                           "<exif:ExposureTime>1/5</exif:ExposureTime>" +
                           "<exif:ColorSpace>1</exif:ColorSpace><exif:LightSource>3</exif:LightSource>" +
                           "<exif:MakerNote>Example note</exif:MakerNote>" +
                           "<exif:GPSMeasureMode>3</exif:GPSMeasureMode>" +
                           "<aux:Lens>Example lens</aux:Lens><aux:SerialNumber>12345</aux:SerialNumber>" +
                           "<crs:AutoBrightness>true</crs:AutoBrightness>" +
                           "<crs:AutoExposure>True</crs:AutoExposure><crs:Brightness>25</crs:Brightness>" +
                           "<crs:CameraProfile>Example Camera</crs:CameraProfile>" +
                           "<crs:ChromaticAberrationB>5</crs:ChromaticAberrationB>" +
                           "<crs:ChromaticAberrationR>-5</crs:ChromaticAberrationR>" +
                           "<crs:ColorNoiseReduction>10</crs:ColorNoiseReduction>" +
                           "<crs:Contrast>15</crs:Contrast>" +
                           "<crs:CropTop>0.1</crs:CropTop><crs:CropLeft>0.1</crs:CropLeft>" +
                           "<crs:CropBottom>0.9</crs:CropBottom><crs:CropRight>0.9</crs:CropRight>" +
                           "<crs:CropAngle>1.5</crs:CropAngle>" +
                           "<crs:CropWidth>0.8</crs:CropWidth><crs:CropHeight>0.8</crs:CropHeight>" +
                           "<crs:CropUnits>1</crs:CropUnits><crs:Exposure>0.5</crs:Exposure>" +
                           "<crs:GreenHue>10</crs:GreenHue><crs:GreenSaturation>20</crs:GreenSaturation>" +
                           "<crs:HasCrop>True</crs:HasCrop><crs:HasSettings>True</crs:HasSettings>" +
                           "<crs:LuminanceSmoothing>10</crs:LuminanceSmoothing>" +
                           "<crs:RawFileName>example.raw</crs:RawFileName>" +
                           "<crs:RedHue>10</crs:RedHue><crs:RedSaturation>20</crs:RedSaturation>" +
                           "<crs:Saturation>15</crs:Saturation><crs:Shadows>5</crs:Shadows>" +
                           "<crs:ShadowTint>3</crs:ShadowTint><crs:Sharpness>25</crs:Sharpness>" +
                           "<crs:Temperature>5500</crs:Temperature><crs:Tint>2</crs:Tint>" +
                           "<crs:ToneCurveName>Linear</crs:ToneCurveName><crs:Version>1.0</crs:Version>" +
                           "<crs:VignetteAmount>-10</crs:VignetteAmount>" +
                           "<crs:VignetteMidpoint>50</crs:VignetteMidpoint>" +
                           "<crs:WhiteBalance>Daylight</crs:WhiteBalance>" +
                           "<crs:ToneCurve>(0, 0) (255, 255)</crs:ToneCurve>" +
                           "<xmpDM:artist>Example artist</xmpDM:artist>" +
                           "<xmpDM:album>Example album</xmpDM:album>" +
                           "<xmpDM:trackNumber>3</xmpDM:trackNumber>" +
                           "<xmpDM:genre>Example genre</xmpDM:genre>" +
                           "<xmpDM:copyright>Example copyright</xmpDM:copyright>" +
                           "<xmpDM:releaseDate>2026-01-02</xmpDM:releaseDate>" +
                           "<xmpDM:composer>Example composer</xmpDM:composer>" +
                           "<xmpDM:engineer>Example engineer</xmpDM:engineer>" +
                           "<xmpDM:videoFrameRate>25</xmpDM:videoFrameRate>" +
                           "<xmpDM:videoFrameSize>1920x1080</xmpDM:videoFrameSize>" +
                           "<xmpDM:videoPixelAspectRatio>1/1</xmpDM:videoPixelAspectRatio>" +
                           "<xmpDM:videoAlphaUnityIsTransparent>False</xmpDM:videoAlphaUnityIsTransparent>" +
                           "<xmpDM:videoAlphaPremultipleColor>Example</xmpDM:videoAlphaPremultipleColor>" +
                           "<xmpDM:videoCompressor>Example codec</xmpDM:videoCompressor>" +
                           "<xmpDM:audioSampleRate>48000</xmpDM:audioSampleRate>" +
                           "<xmpDM:audioCompressor>Example audio codec</xmpDM:audioCompressor>" +
                           "<xmpDM:speakerPlacement>stereo</xmpDM:speakerPlacement>" +
                           "<xmpDM:fileDataRate>1/2</xmpDM:fileDataRate>" +
                           "<xmpDM:tapeName>Example tape</xmpDM:tapeName>" +
                           "<xmpDM:altTapeName>Alternate tape</xmpDM:altTapeName>" +
                           "<xmpDM:startTimecode>00:00:00:00</xmpDM:startTimecode>" +
                           "<xmpDM:altTimecode>00:00:01:00</xmpDM:altTimecode>" +
                           "<xmpDM:duration rdf:parseType=\"Resource\"><xmpDM:value>300</xmpDM:value>" +
                           "<xmpDM:scale>1/30</xmpDM:scale></xmpDM:duration>" +
                           "<xmpDM:scene>Opening scene</xmpDM:scene>" +
                           "<xmpDM:shotName>Opening shot</xmpDM:shotName>" +
                           "<xmpDM:shotDate>2026-01-02</xmpDM:shotDate>" +
                           "<xmpDM:shotLocation>Stage A</xmpDM:shotLocation>" +
                           "<xmpDM:logComment>Scene setup</xmpDM:logComment>" +
                           "<xmpDM:markers>Example markers</xmpDM:markers>" +
                           "<xmpDM:contributedMedia><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
                           "<xmpDM:path>urn:example:media</xmpDM:path><xmpDM:track>Audio 1</xmpDM:track>" +
                           "<xmpDM:startTime rdf:parseType=\"Resource\"><xmpDM:value>0</xmpDM:value>" +
                           "<xmpDM:scale>1/30</xmpDM:scale></xmpDM:startTime>" +
                           "<xmpDM:duration rdf:parseType=\"Resource\"><xmpDM:value>300</xmpDM:value>" +
                           "<xmpDM:scale>1/30</xmpDM:scale></xmpDM:duration>" +
                           "<xmpDM:managed>True</xmpDM:managed><xmpDM:webStatement>urn:example:web</xmpDM:webStatement>" +
                           "</rdf:li></rdf:Bag></xmpDM:contributedMedia>" +
                           "<xmpDM:absPeakAudioFilePath>file:///tmp/peak.wav</xmpDM:absPeakAudioFilePath>" +
                           "<xmpDM:relativePeakAudioFilePath>audio/peak.wav</xmpDM:relativePeakAudioFilePath>" +
                           "<xmpDM:videoModDate>2026-01-03</xmpDM:videoModDate>" +
                           "<xmpDM:audioModDate>2026-01-04</xmpDM:audioModDate>" +
                           "<xmpDM:metadataModDate>2026-01-05</xmpDM:metadataModDate>" +
                           "<xmpDM:tempo>120.5x</xmpDM:tempo><xmpDM:instrument>Example instrument</xmpDM:instrument>" +
                           "<xmpDM:introTime>00:00:02.000</xmpDM:introTime>" +
                           "<xmpDM:outCue>00:00:08.000</xmpDM:outCue>" +
                           "<xmpDM:relativeTimestamp>00:00:03.000</xmpDM:relativeTimestamp>" +
                           "<xmpDM:loop>False</xmpDM:loop><xmpDM:numberOfBeats>32.5</xmpDM:numberOfBeats>" +
                           "<xmpDM:timeScaleParams>Example time scale</xmpDM:timeScaleParams>" +
                           "<xmpDM:resampleParams>Example resample</xmpDM:resampleParams>" +
                           "<xmpDM:beatSpliceParams>Example splice</xmpDM:beatSpliceParams>" +
                           "<xmpDM:projectRef>Example project</xmpDM:projectRef>" +
                           "<xmpDM:videoPixelDepth>8Int</xmpDM:videoPixelDepth>" +
                           "<xmpDM:videoColorSpace>sRGB</xmpDM:videoColorSpace>" +
                           "<xmpDM:videoAlphaMode>straight</xmpDM:videoAlphaMode>" +
                           "<xmpDM:videoFieldOrder>Progressive</xmpDM:videoFieldOrder>" +
                           "<xmpDM:pullDown>WSSW</xmpDM:pullDown>" +
                           "<xmpDM:audioSampleType>16Int</xmpDM:audioSampleType>" +
                           "<xmpDM:audioChannelType>Stereo</xmpDM:audioChannelType>" +
                           "<xmpDM:key>C</xmpDM:key><xmpDM:stretchMode>Fixed length</xmpDM:stretchMode>" +
                           "<xmpDM:timeSignature>4/4</xmpDM:timeSignature>" +
                           "<xmpDM:scaleType>Major</xmpDM:scaleType>" +
                           "<xmpDM:unportedProperty>example</xmpDM:unportedProperty>" +
                           "<pdf:Producer>Example Producer</pdf:Producer>" +
                           "</rdf:Description></rdf:RDF>" +
                           "<?xpacket end=\"w\"?>";
        var streamDictionary = new CosDictionary();
        streamDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        streamDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(streamDictionary, System.Text.Encoding.UTF8.GetBytes(xmp));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));

        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));

        Assert.True(package.GetisSerializationValid());
        Assert.Equal("UTF-8", package.GetactualEncoding());
        Assert.Equal("128", package.Getbytes());
        Assert.Equal("UTF-8", package.Getencoding());
        var properties = package.GetLinkedObjects("Properties")
            .Select(Assert.IsAssignableFrom<IXMPProperty>)
            .ToArray();
        Assert.Equal(140, properties.Length);
        var dcTitle = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:title");
        Assert.True(dcTitle.GetisPredefinedInXMP2004());
        Assert.True(dcTitle.GetisPredefinedInXMP2005());
        Assert.Equal("lang alt", dcTitle.GetpredefinedType());
        Assert.True(dcTitle.GetisValueTypeCorrect());
        var dcDescription = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:description");
        Assert.True(dcDescription.GetisValueTypeCorrect());
        var dcRights = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:rights");
        Assert.False(dcRights.GetisValueTypeCorrect());
        var dcCoverage = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:coverage");
        Assert.Equal("text", dcCoverage.GetpredefinedType());
        Assert.False(dcCoverage.GetisValueTypeCorrect());
        var dcCreator = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:creator");
        Assert.Equal("seq propername", dcCreator.GetpredefinedType());
        Assert.True(dcCreator.GetisValueTypeCorrect());
        var dcSubject = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:subject");
        Assert.False(dcSubject.GetisValueTypeCorrect());
        var dcRelation = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:relation");
        Assert.False(dcRelation.GetisValueTypeCorrect());
        var dcPublisher = Assert.Single(properties,
            property => property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:publisher");
        Assert.True(dcPublisher.GetisValueTypeCorrect());
        var rightsMarked = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/rights/ - xmpRights:Marked");
        Assert.True(rightsMarked.GetisPredefinedInXMP2004());
        Assert.True(rightsMarked.GetisPredefinedInXMP2005());
        Assert.Equal("boolean", rightsMarked.GetpredefinedType());
        Assert.True(rightsMarked.GetisValueTypeCorrect());
        var documentId = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:DocumentID");
        Assert.True(documentId.GetisPredefinedInXMP2004());
        Assert.True(documentId.GetisPredefinedInXMP2005());
        Assert.Equal("uri", documentId.GetpredefinedType());
        Assert.True(documentId.GetisValueTypeCorrect());
        var derivedFrom = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:DerivedFrom");
        Assert.True(derivedFrom.GetisValueTypeCorrect());
        var history = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:History");
        Assert.True(history.GetisValueTypeCorrect());
        var versions = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:Versions");
        Assert.True(versions.GetisValueTypeCorrect());
        var renditionClass = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:RenditionClass");
        Assert.True(renditionClass.GetisValueTypeCorrect());
        var instanceId = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:InstanceID");
        Assert.False(instanceId.GetisValueTypeCorrect());
        var baseUrl = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:BaseURL");
        Assert.Equal("url", baseUrl.GetpredefinedType());
        Assert.True(baseUrl.GetisValueTypeCorrect());
        var manager = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/mm/ - xmpMM:Manager");
        Assert.True(manager.GetisPredefinedInXMP2004());
        Assert.True(manager.GetisPredefinedInXMP2005());
        Assert.Equal("agentname", manager.GetpredefinedType());
        var jobRef = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/bj/ - xmpBJ:JobRef");
        Assert.True(jobRef.GetisPredefinedInXMP2004());
        Assert.True(jobRef.GetisPredefinedInXMP2005());
        Assert.Equal("bag job", jobRef.GetpredefinedType());
        Assert.True(jobRef.GetisValueTypeCorrect());
        var xmpLabel = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:Label");
        Assert.False(xmpLabel.GetisPredefinedInXMP2004());
        Assert.True(xmpLabel.GetisPredefinedInXMP2005());
        Assert.Equal("text", xmpLabel.GetpredefinedType());
        var xmpRating = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:Rating");
        Assert.False(xmpRating.GetisPredefinedInXMP2004());
        Assert.True(xmpRating.GetisPredefinedInXMP2005());
        Assert.Equal("real", xmpRating.GetpredefinedType());
        Assert.True(xmpRating.GetisValueTypeCorrect());
        var thumbnails = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:Thumbnails");
        Assert.True(thumbnails.GetisValueTypeCorrect());
        var createDate = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:CreateDate");
        Assert.True(createDate.GetisValueTypeCorrect());
        var modifyDate = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:ModifyDate");
        Assert.True(modifyDate.GetisValueTypeCorrect());
        var metadataDate = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/ - xmp:MetadataDate");
        Assert.False(metadataDate.GetisValueTypeCorrect());
        var maxPageSize = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/t/pg/ - xmpTPg:MaxPageSize");
        Assert.True(maxPageSize.GetisPredefinedInXMP2004());
        Assert.True(maxPageSize.GetisPredefinedInXMP2005());
        Assert.Equal("dimensions", maxPageSize.GetpredefinedType());
        Assert.True(maxPageSize.GetisValueTypeCorrect());
        var pageCount = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/t/pg/ - xmpTPg:NPages");
        Assert.True(pageCount.GetisPredefinedInXMP2004());
        Assert.True(pageCount.GetisPredefinedInXMP2005());
        Assert.Equal("integer", pageCount.GetpredefinedType());
        Assert.False(pageCount.GetisValueTypeCorrect());
        var fonts = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/t/pg/ - xmpTPg:Fonts");
        Assert.False(fonts.GetisPredefinedInXMP2004());
        Assert.True(fonts.GetisPredefinedInXMP2005());
        Assert.Equal("bag font", fonts.GetpredefinedType());
        Assert.True(fonts.GetisValueTypeCorrect());
        var colorants = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/t/pg/ - xmpTPg:Colorants");
        Assert.False(colorants.GetisPredefinedInXMP2004());
        Assert.True(colorants.GetisPredefinedInXMP2005());
        Assert.Equal("seq colorant", colorants.GetpredefinedType());
        var plateNames = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/xap/1.0/t/pg/ - xmpTPg:PlateNames");
        Assert.False(plateNames.GetisPredefinedInXMP2004());
        Assert.True(plateNames.GetisPredefinedInXMP2005());
        Assert.Equal("seq text", plateNames.GetpredefinedType());
        var pdfaPart = Assert.Single(properties,
            property => property.GetID() == "http://www.aiim.org/pdfa/ns/id/ - pdfaid:part");
        Assert.True(pdfaPart.GetisPredefinedInXMP2004());
        Assert.True(pdfaPart.GetisPredefinedInXMP2005());
        Assert.Equal("integer", pdfaPart.GetpredefinedType());
        Assert.True(pdfaPart.GetisValueTypeCorrect());
        var pdfaAmd = Assert.Single(properties,
            property => property.GetID() == "http://www.aiim.org/pdfa/ns/id/ - pdfaid:amd");
        Assert.True(pdfaAmd.GetisPredefinedInXMP2004());
        Assert.True(pdfaAmd.GetisPredefinedInXMP2005());
        Assert.Equal("text", pdfaAmd.GetpredefinedType());
        var pdfaConformance = Assert.Single(properties,
            property => property.GetID() == "http://www.aiim.org/pdfa/ns/id/ - pdfaid:conformance");
        Assert.True(pdfaConformance.GetisPredefinedInXMP2004());
        Assert.True(pdfaConformance.GetisPredefinedInXMP2005());
        Assert.Equal("text", pdfaConformance.GetpredefinedType());
        var pdfaCorrection = Assert.Single(properties,
            property => property.GetID() == "http://www.aiim.org/pdfa/ns/id/ - pdfaid:corr");
        Assert.False(pdfaCorrection.GetisPredefinedInXMP2004());
        Assert.True(pdfaCorrection.GetisPredefinedInXMP2005());
        Assert.Equal("text", pdfaCorrection.GetpredefinedType());
        var headline = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/photoshop/1.0/ - photoshop:Headline");
        Assert.True(headline.GetisPredefinedInXMP2004());
        Assert.True(headline.GetisPredefinedInXMP2005());
        Assert.Equal("text", headline.GetpredefinedType());
        Assert.True(headline.GetisValueTypeCorrect());
        var supplementalCategories = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/photoshop/1.0/ - photoshop:SupplementalCategories");
        Assert.True(supplementalCategories.GetisPredefinedInXMP2004());
        Assert.True(supplementalCategories.GetisPredefinedInXMP2005());
        Assert.Null(supplementalCategories.GetpredefinedType());
        var imageWidth = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/tiff/1.0/ - tiff:ImageWidth");
        Assert.True(imageWidth.GetisPredefinedInXMP2004());
        Assert.True(imageWidth.GetisPredefinedInXMP2005());
        Assert.Equal("integer", imageWidth.GetpredefinedType());
        var artist = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/tiff/1.0/ - tiff:Artist");
        Assert.True(artist.GetisPredefinedInXMP2004());
        Assert.True(artist.GetisPredefinedInXMP2005());
        Assert.Equal("propername", artist.GetpredefinedType());
        Assert.True(artist.GetisValueTypeCorrect());
        var exposureTime = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/ - exif:ExposureTime");
        Assert.True(exposureTime.GetisPredefinedInXMP2004());
        Assert.True(exposureTime.GetisPredefinedInXMP2005());
        Assert.Equal("rational", exposureTime.GetpredefinedType());
        Assert.True(exposureTime.GetisValueTypeCorrect());
        var colorSpace = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/ - exif:ColorSpace");
        Assert.True(colorSpace.GetisPredefinedInXMP2004());
        Assert.True(colorSpace.GetisPredefinedInXMP2005());
        Assert.Equal("integer", colorSpace.GetpredefinedType());
        var lightSource = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/ - exif:LightSource");
        Assert.True(lightSource.GetisPredefinedInXMP2004());
        Assert.True(lightSource.GetisPredefinedInXMP2005());
        Assert.Equal("integer", lightSource.GetpredefinedType());
        var makerNote = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/ - exif:MakerNote");
        Assert.True(makerNote.GetisPredefinedInXMP2004());
        Assert.False(makerNote.GetisPredefinedInXMP2005());
        Assert.Equal("text", makerNote.GetpredefinedType());
        var gpsMeasureMode = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/ - exif:GPSMeasureMode");
        Assert.True(gpsMeasureMode.GetisPredefinedInXMP2004());
        Assert.True(gpsMeasureMode.GetisPredefinedInXMP2005());
        Assert.Null(gpsMeasureMode.GetpredefinedType());
        var lens = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/aux/ - aux:Lens");
        Assert.False(lens.GetisPredefinedInXMP2004());
        Assert.True(lens.GetisPredefinedInXMP2005());
        Assert.Equal("text", lens.GetpredefinedType());
        var serialNumber = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/exif/1.0/aux/ - aux:SerialNumber");
        Assert.False(serialNumber.GetisPredefinedInXMP2004());
        Assert.True(serialNumber.GetisPredefinedInXMP2005());
        Assert.Equal("text", serialNumber.GetpredefinedType());
        var autoExposure = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:AutoExposure");
        Assert.False(autoExposure.GetisPredefinedInXMP2004());
        Assert.True(autoExposure.GetisPredefinedInXMP2005());
        Assert.Equal("boolean", autoExposure.GetpredefinedType());
        Assert.True(autoExposure.GetisValueTypeCorrect());
        var autoBrightness = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:AutoBrightness");
        Assert.False(autoBrightness.GetisValueTypeCorrect());
        var brightness = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:Brightness");
        Assert.False(brightness.GetisPredefinedInXMP2004());
        Assert.True(brightness.GetisPredefinedInXMP2005());
        Assert.Equal("integer", brightness.GetpredefinedType());
        var cameraProfile = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:CameraProfile");
        Assert.False(cameraProfile.GetisPredefinedInXMP2004());
        Assert.True(cameraProfile.GetisPredefinedInXMP2005());
        Assert.Equal("text", cameraProfile.GetpredefinedType());
        var aberrationRed = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:ChromaticAberrationR");
        Assert.False(aberrationRed.GetisPredefinedInXMP2004());
        Assert.True(aberrationRed.GetisPredefinedInXMP2005());
        Assert.Equal("integer", aberrationRed.GetpredefinedType());
        var cropAngle = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:CropAngle");
        Assert.False(cropAngle.GetisPredefinedInXMP2004());
        Assert.True(cropAngle.GetisPredefinedInXMP2005());
        Assert.Equal("real", cropAngle.GetpredefinedType());
        var exposure = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:Exposure");
        Assert.False(exposure.GetisPredefinedInXMP2004());
        Assert.True(exposure.GetisPredefinedInXMP2005());
        Assert.Equal("real", exposure.GetpredefinedType());
        var hasCrop = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:HasCrop");
        Assert.False(hasCrop.GetisPredefinedInXMP2004());
        Assert.True(hasCrop.GetisPredefinedInXMP2005());
        Assert.Equal("boolean", hasCrop.GetpredefinedType());
        var rawFileName = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:RawFileName");
        Assert.False(rawFileName.GetisPredefinedInXMP2004());
        Assert.True(rawFileName.GetisPredefinedInXMP2005());
        Assert.Equal("text", rawFileName.GetpredefinedType());
        var temperature = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:Temperature");
        Assert.False(temperature.GetisPredefinedInXMP2004());
        Assert.True(temperature.GetisPredefinedInXMP2005());
        Assert.Equal("integer", temperature.GetpredefinedType());
        var whiteBalance = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:WhiteBalance");
        Assert.False(whiteBalance.GetisPredefinedInXMP2004());
        Assert.True(whiteBalance.GetisPredefinedInXMP2005());
        Assert.Equal("text", whiteBalance.GetpredefinedType());
        var toneCurve = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/camera-raw-settings/1.0/ - crs:ToneCurve");
        Assert.False(toneCurve.GetisPredefinedInXMP2004());
        Assert.True(toneCurve.GetisPredefinedInXMP2005());
        Assert.Equal("text", toneCurve.GetpredefinedType());
        var album = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:album");
        Assert.False(album.GetisPredefinedInXMP2004());
        Assert.True(album.GetisPredefinedInXMP2005());
        Assert.Equal("text", album.GetpredefinedType());
        var trackNumber = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:trackNumber");
        Assert.False(trackNumber.GetisPredefinedInXMP2004());
        Assert.True(trackNumber.GetisPredefinedInXMP2005());
        Assert.Equal("integer", trackNumber.GetpredefinedType());
        var videoFrameSize = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:videoFrameSize");
        Assert.False(videoFrameSize.GetisPredefinedInXMP2004());
        Assert.True(videoFrameSize.GetisPredefinedInXMP2005());
        Assert.Equal("dimensions", videoFrameSize.GetpredefinedType());
        var audioSampleRate = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:audioSampleRate");
        Assert.False(audioSampleRate.GetisPredefinedInXMP2004());
        Assert.True(audioSampleRate.GetisPredefinedInXMP2005());
        Assert.Equal("integer", audioSampleRate.GetpredefinedType());
        var fileDataRate = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:fileDataRate");
        Assert.False(fileDataRate.GetisPredefinedInXMP2004());
        Assert.True(fileDataRate.GetisPredefinedInXMP2005());
        Assert.Equal("rational", fileDataRate.GetpredefinedType());
        var duration = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:duration");
        Assert.False(duration.GetisPredefinedInXMP2004());
        Assert.True(duration.GetisPredefinedInXMP2005());
        Assert.Equal("time", duration.GetpredefinedType());
        Assert.True(duration.GetisValueTypeCorrect());
        var shotDate = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:shotDate");
        Assert.False(shotDate.GetisPredefinedInXMP2004());
        Assert.True(shotDate.GetisPredefinedInXMP2005());
        Assert.Equal("date", shotDate.GetpredefinedType());
        var markers = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:markers");
        Assert.False(markers.GetisPredefinedInXMP2004());
        Assert.True(markers.GetisPredefinedInXMP2005());
        Assert.Equal("seq marker", markers.GetpredefinedType());
        var contributedMedia = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:contributedMedia");
        Assert.Equal("bag media", contributedMedia.GetpredefinedType());
        Assert.True(contributedMedia.GetisValueTypeCorrect());
        var videoModDate = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:videoModDate");
        Assert.False(videoModDate.GetisPredefinedInXMP2004());
        Assert.True(videoModDate.GetisPredefinedInXMP2005());
        Assert.Equal("date", videoModDate.GetpredefinedType());
        var tempo = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:tempo");
        Assert.False(tempo.GetisPredefinedInXMP2004());
        Assert.True(tempo.GetisPredefinedInXMP2005());
        Assert.Equal("real", tempo.GetpredefinedType());
        Assert.False(tempo.GetisValueTypeCorrect());
        var unportedDynamicMediaProperty = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:unportedProperty");
        Assert.False(unportedDynamicMediaProperty.GetisPredefinedInXMP2004());
        Assert.False(unportedDynamicMediaProperty.GetisPredefinedInXMP2005());
        Assert.Null(unportedDynamicMediaProperty.GetpredefinedType());
        var loop = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:loop");
        Assert.False(loop.GetisPredefinedInXMP2004());
        Assert.True(loop.GetisPredefinedInXMP2005());
        Assert.Equal("boolean", loop.GetpredefinedType());
        var projectRef = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:projectRef");
        Assert.False(projectRef.GetisPredefinedInXMP2004());
        Assert.True(projectRef.GetisPredefinedInXMP2005());
        Assert.Equal("projectlink", projectRef.GetpredefinedType());
        var videoAlphaMode = Assert.Single(properties,
            property => property.GetID() ==
                        "http://ns.adobe.com/xmp/1.0/DynamicMedia/ - xmpDM:videoAlphaMode");
        Assert.False(videoAlphaMode.GetisPredefinedInXMP2004());
        Assert.True(videoAlphaMode.GetisPredefinedInXMP2005());
        Assert.Equal("text", videoAlphaMode.GetpredefinedType());
        var pdfProducer = Assert.Single(properties,
            property => property.GetID() == "http://ns.adobe.com/pdf/1.3/ - pdf:Producer");
        Assert.True(pdfProducer.GetisPredefinedInXMP2004());
        Assert.True(pdfProducer.GetisPredefinedInXMP2005());
        Assert.Equal("agentname", pdfProducer.GetpredefinedType());
        Assert.True(pdfProducer.GetisValueTypeCorrect());
        Assert.Empty(package.GetLinkedObjects("ExtensionSchemasContainers"));
        Assert.Throws<InvalidOperationException>(() => package.GetLinkedObjects("unknown"));
    }

    [Fact]
    public void ExposesExtensionSchemaContainerAndBagValidity()
    {
        const string xmp = "<?xpacket begin=\"\uFEFF\"?>" +
                           "<rdf:RDF xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
                           "<rdf:Description xmlns:pdfaExtension=\"http://www.aiim.org/pdfa/ns/extension/\" " +
                           "xmlns:pdfaSchema=\"http://www.aiim.org/pdfa/ns/schema#\" " +
                           "xmlns:pdfaProperty=\"http://www.aiim.org/pdfa/ns/property#\" " +
                           "xmlns:pdfaType=\"http://www.aiim.org/pdfa/ns/type#\" " +
                           "xmlns:pdfaField=\"http://www.aiim.org/pdfa/ns/field#\" " +
                           "xmlns:custom=\"https://example.test/ns/\" " +
                           "xmlns:dc=\"http://purl.org/dc/elements/1.1/\">" +
                           "<pdfaExtension:schemas><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaSchema:schema>Custom schema</pdfaSchema:schema>" +
                           "<pdfaSchema:namespaceURI>https://example.test/ns/</pdfaSchema:namespaceURI>" +
                           "<pdfaSchema:prefix>custom</pdfaSchema:prefix>" +
                           "<pdfaSchema:property><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaProperty:name>customProperty</pdfaProperty:name>" +
                           "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
                           "<pdfaProperty:category>external</pdfaProperty:category>" +
                           "<pdfaProperty:description>Example property</pdfaProperty:description>" +
                           "</rdf:li><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaProperty:name>customXPath</pdfaProperty:name>" +
                           "<pdfaProperty:valueType>XPath</pdfaProperty:valueType>" +
                           "<pdfaProperty:category>external</pdfaProperty:category>" +
                           "<pdfaProperty:description>XPath property</pdfaProperty:description>" +
                           "</rdf:li><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaProperty:name>customRational</pdfaProperty:name>" +
                           "<pdfaProperty:valueType>Rational</pdfaProperty:valueType>" +
                           "<pdfaProperty:category>external</pdfaProperty:category>" +
                           "<pdfaProperty:description>Rational property</pdfaProperty:description>" +
                           "</rdf:li><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaProperty:name>customInvalidXPath</pdfaProperty:name>" +
                           "<pdfaProperty:valueType>XPath</pdfaProperty:valueType>" +
                           "<pdfaProperty:category>external</pdfaProperty:category>" +
                           "<pdfaProperty:description>Invalid XPath property</pdfaProperty:description>" +
                           "</rdf:li><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaProperty:name>customUnsupported</pdfaProperty:name>" +
                           "<pdfaProperty:valueType>UnsupportedType</pdfaProperty:valueType>" +
                           "<pdfaProperty:category>external</pdfaProperty:category>" +
                           "<pdfaProperty:description>Unsupported type property</pdfaProperty:description>" +
                           "</rdf:li></rdf:Seq>" +
                           "</pdfaSchema:property>" +
                           "<pdfaSchema:valueType><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaType:namespaceURI>https://example.test/types/</pdfaType:namespaceURI>" +
                           "<pdfaType:prefix>customType</pdfaType:prefix>" +
                           "<pdfaType:type>CustomType</pdfaType:type>" +
                           "<pdfaType:description>Example type</pdfaType:description>" +
                           "<pdfaType:field><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
                           "<pdfaField:name>customField</pdfaField:name>" +
                           "<pdfaField:valueType>Text</pdfaField:valueType>" +
                           "<pdfaField:description>Example field</pdfaField:description>" +
                           "</rdf:li></rdf:Seq></pdfaType:field>" +
                           "</rdf:li></rdf:Seq></pdfaSchema:valueType>" +
                           "</rdf:li></rdf:Bag>" +
                           "</pdfaExtension:schemas><dc:title>Example</dc:title>" +
                           "<custom:customProperty>Example value</custom:customProperty>" +
                           "<custom:customXPath>//*</custom:customXPath>" +
                           "<custom:customRational>unparsed rational text</custom:customRational>" +
                           "<custom:customInvalidXPath>[</custom:customInvalidXPath>" +
                           "<custom:customUnsupported>value</custom:customUnsupported>" +
                           "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        var streamDictionary = new CosDictionary();
        streamDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        streamDictionary.Set("Subtype", new CosName("XML"u8.ToArray()));
        var metadataStream = new CosStream(
            streamDictionary,
            System.Text.Encoding.UTF8.GetBytes(xmp));
        var dictionary = new CosDictionary();
        dictionary.Set("Type", new CosName("Page"u8.ToArray()));
        dictionary.Set("Metadata", metadataStream);
        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(dictionary)
                .GetLinkedObjects("metadata")));
        var package = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(metadata.GetLinkedObjects("XMPPackage")));

        var properties = package.GetLinkedObjects("Properties");
        Assert.Equal(6, properties.Count);
        var customProperty = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "https://example.test/ns/ - custom:customProperty"));
        Assert.True(customProperty.GetisDefinedInCurrentPackage());
        Assert.True(customProperty.GetisValueTypeCorrect());
        Assert.Equal("Text", customProperty.GetpredefinedType());
        var customXPath = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "https://example.test/ns/ - custom:customXPath"));
        Assert.Equal("XPath", customXPath.GetpredefinedType());
        Assert.True(customXPath.GetisValueTypeCorrect());
        var customRational = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "https://example.test/ns/ - custom:customRational"));
        Assert.Equal("Rational", customRational.GetpredefinedType());
        Assert.True(customRational.GetisValueTypeCorrect());
        var customInvalidXPath = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "https://example.test/ns/ - custom:customInvalidXPath"));
        Assert.False(customInvalidXPath.GetisValueTypeCorrect());
        var customUnsupported = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "https://example.test/ns/ - custom:customUnsupported"));
        Assert.True(customUnsupported.GetisDefinedInCurrentPackage());
        Assert.Equal("UnsupportedType", customUnsupported.GetpredefinedType());
        Assert.Null(customUnsupported.GetisValueTypeCorrect());
        var standardProperty = Assert.IsAssignableFrom<IXMPProperty>(
            properties.Single(property =>
                property.GetID() == "http://purl.org/dc/elements/1.1/ - dc:title"));
        Assert.True(standardProperty.GetisPredefinedInXMP2004());
        Assert.True(standardProperty.GetisPredefinedInXMP2005());
        Assert.False(standardProperty.GetisDefinedInCurrentPackage());
        Assert.Equal("lang alt", standardProperty.GetpredefinedType());
        Assert.False(standardProperty.GetisValueTypeCorrect());
        var container = Assert.IsAssignableFrom<IExtensionSchemasContainer>(
            Assert.Single(package.GetLinkedObjects("ExtensionSchemasContainers")));
        Assert.Equal("pdfaExtension", container.Getprefix());
        Assert.True(container.GetisValidBag());
        var definition = Assert.IsAssignableFrom<IExtensionSchemaDefinition>(
            Assert.Single(container.GetLinkedObjects("ExtensionSchemaDefinitions")));
        Assert.False(definition.GetcontainsUndefinedFields());
        Assert.Equal("", definition.GetundefinedFields());
        Assert.True(definition.GetisNamespaceURIValidURI());
        Assert.True(definition.GetisPrefixValidText());
        Assert.True(definition.GetisPropertyValidSeq());
        Assert.True(definition.GetisSchemaValidText());
        Assert.True(definition.GetisValueTypeValidSeq());
        Assert.Equal("pdfaSchema", definition.GetpropertyPrefix());
        var property = Assert.IsAssignableFrom<IExtensionSchemaProperty>(
            definition.GetLinkedObjects("ExtensionSchemaProperties").First());
        Assert.False(property.GetcontainsUndefinedFields());
        Assert.Equal("", property.GetundefinedFields());
        Assert.Equal("external", property.Getcategory());
        Assert.True(property.GetisCategoryValidText());
        Assert.True(property.GetisDescriptionValidText());
        Assert.True(property.GetisNameValidText());
        Assert.True(property.GetisValueTypeValidText());
        Assert.Null(property.GetisValueTypeDefined());
        Assert.Equal("pdfaProperty", property.GetcategoryPrefix());
        Assert.Equal("pdfaProperty", property.GetdescriptionPrefix());
        Assert.Equal("pdfaProperty", property.GetnamePrefix());
        Assert.Equal("pdfaProperty", property.GetvalueTypePrefix());

        Assert.Equal("pdfaSchema", definition.GetvalueTypePrefix());
        var valueType = Assert.IsAssignableFrom<IExtensionSchemaValueType>(
            Assert.Single(definition.GetLinkedObjects("ExtensionSchemaValueTypes")));
        Assert.False(valueType.GetcontainsUndefinedFields());
        Assert.Equal("", valueType.GetundefinedFields());
        Assert.True(valueType.GetisDescriptionValidText());
        Assert.True(valueType.GetisFieldValidSeq());
        Assert.True(valueType.GetisNamespaceURIValidURI());
        Assert.True(valueType.GetisPrefixValidText());
        Assert.True(valueType.GetisTypeValidText());
        Assert.Equal("pdfaType", valueType.GetdescriptionPrefix());
        Assert.Equal("pdfaType", valueType.GetfieldPrefix());
        Assert.Equal("pdfaType", valueType.GetnamespaceURIPrefix());
        Assert.Equal("pdfaType", valueType.GetprefixPrefix());
        Assert.Equal("pdfaType", valueType.GettypePrefix());

        var field = Assert.IsAssignableFrom<IExtensionSchemaField>(
            Assert.Single(valueType.GetLinkedObjects("ExtensionSchemaFields")));
        Assert.False(field.GetcontainsUndefinedFields());
        Assert.Equal("", field.GetundefinedFields());
        Assert.True(field.GetisDescriptionValidText());
        Assert.True(field.GetisNameValidText());
        Assert.True(field.GetisValueTypeValidText());
        Assert.Null(field.GetisValueTypeDefined());
        Assert.Equal("pdfaField", field.GetdescriptionPrefix());
        Assert.Equal("pdfaField", field.GetnamePrefix());
        Assert.Equal("pdfaField", field.GetvalueTypePrefix());
    }

    [Fact]
    public void OmitsCatalogAndNonXmlMetadata()
    {
        var metadataDictionary = new CosDictionary();
        metadataDictionary.Set("Type", new CosName("Metadata"u8.ToArray()));
        metadataDictionary.Set("Subtype", new CosName("Binary"u8.ToArray()));
        var metadataStream = new CosStream(metadataDictionary, []);
        var catalog = new CosDictionary();
        catalog.Set("Type", new CosName("Catalog"u8.ToArray()));
        catalog.Set("Metadata", metadataStream);
        var nonXml = new CosDictionary();
        nonXml.Set("Type", new CosName("Page"u8.ToArray()));
        nonXml.Set("Metadata", metadataStream);

        Assert.Empty(CosDictionaryModelAdapter.FromParserValue(catalog).GetLinkedObjects("metadata"));
        Assert.Empty(CosDictionaryModelAdapter.FromParserValue(nonXml).GetLinkedObjects("metadata"));
    }

    [Fact]
    public void ChecksPropertyDefinednessAgainstTheCatalogMetadataPackage()
    {
        var pdf = CreateMetadataContextPdf();
        using var reader = PdfDocumentReader.Open(
            new MemoryStream(System.Text.Encoding.ASCII.GetBytes(pdf)));
        var catalog = Assert.IsType<CosDictionary>(
            reader.ResolveDictionary(reader.Trailer.Entries["Root"]));
        var catalogMetadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(catalog, reader)
                .GetLinkedObjects("metadata")));
        Assert.True(catalogMetadata.GetisCatalogMetadata());

        var mainPackage = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(catalogMetadata.GetLinkedObjects("XMPPackage")));
        var mainProperty = FindProperty(mainPackage, "customProperty");
        Assert.True(mainProperty.GetisDefinedInMainPackage());

        var page = Assert.IsType<CosIndirectObject>(reader.Resolve(new CosKey(3, 0))).Value;
        var pageDictionary = Assert.IsType<CosDictionary>(page);
        var pageMetadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(pageDictionary, reader)
                .GetLinkedObjects("metadata")));
        Assert.False(pageMetadata.GetisCatalogMetadata());
        var pagePackage = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(pageMetadata.GetLinkedObjects("XMPPackage")));

        Assert.True(FindProperty(pagePackage, "customProperty").GetisDefinedInMainPackage());
        Assert.False(FindProperty(pagePackage, "undeclaredProperty").GetisDefinedInMainPackage());

        var unscopedDictionary = new CosDictionary();
        unscopedDictionary.Set("Type", new CosName("Page"u8.ToArray()));
        unscopedDictionary.Set(
            "Metadata",
            Assert.IsType<CosStream>(
                reader.ResolveReference(pageDictionary.Entries["Metadata"])));
        var withoutReader = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(CosDictionaryModelAdapter.FromParserValue(unscopedDictionary)
                .GetLinkedObjects("metadata")));
        var unscopedPackage = Assert.IsAssignableFrom<IXMPPackage>(
            Assert.Single(withoutReader.GetLinkedObjects("XMPPackage")));
        Assert.Null(FindProperty(unscopedPackage, "customProperty").GetisDefinedInMainPackage());
    }

    private static IXMPProperty FindProperty(IXMPPackage package, string name) =>
        Assert.IsAssignableFrom<IXMPProperty>(
            package.GetLinkedObjects("Properties").Single(property =>
                property.GetID() == $"urn:example:custom - custom:{name}"));

    private static string CreateMetadataContextPdf()
    {
        const string mainXmp =
            "<?xpacket begin=\"\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:pdfaExtension=\"http://www.aiim.org/pdfa/ns/extension/\" " +
            "xmlns:pdfaSchema=\"http://www.aiim.org/pdfa/ns/schema#\" " +
            "xmlns:pdfaProperty=\"http://www.aiim.org/pdfa/ns/property#\" " +
            "xmlns:custom=\"urn:example:custom\">" +
            "<pdfaExtension:schemas><rdf:Bag><rdf:li rdf:parseType=\"Resource\">" +
            "<pdfaSchema:namespaceURI>urn:example:custom</pdfaSchema:namespaceURI>" +
            "<pdfaSchema:property><rdf:Seq><rdf:li rdf:parseType=\"Resource\">" +
            "<pdfaProperty:name>customProperty</pdfaProperty:name>" +
            "<pdfaProperty:valueType>Text</pdfaProperty:valueType>" +
            "</rdf:li></rdf:Seq></pdfaSchema:property>" +
            "</rdf:li></rdf:Bag></pdfaExtension:schemas>" +
            "<custom:customProperty>main</custom:customProperty>" +
            "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";
        const string pageXmp =
            "<?xpacket begin=\"\"?><rdf:RDF " +
            "xmlns:rdf=\"http://www.w3.org/1999/02/22-rdf-syntax-ns#\">" +
            "<rdf:Description xmlns:custom=\"urn:example:custom\">" +
            "<custom:customProperty>page</custom:customProperty>" +
            "<custom:undeclaredProperty>page</custom:undeclaredProperty>" +
            "</rdf:Description></rdf:RDF><?xpacket end=\"w\"?>";

        var mainBytes = System.Text.Encoding.ASCII.GetBytes(mainXmp);
        var pageBytes = System.Text.Encoding.ASCII.GetBytes(pageXmp);
        var prefix = "%PDF-1.7\n";
        var objects = new[]
        {
            "1 0 obj\n<< /Type /Catalog /Metadata 2 0 R >>\nendobj\n",
            $"2 0 obj\n<< /Type /Metadata /Subtype /XML /Length {mainBytes.Length} >>\nstream\n" +
            mainXmp + "\nendstream\nendobj\n",
            "3 0 obj\n<< /Type /Page /Metadata 4 0 R >>\nendobj\n",
            $"4 0 obj\n<< /Type /Metadata /Subtype /XML /Length {pageBytes.Length} >>\nstream\n" +
            pageXmp + "\nendstream\nendobj\n"
        };
        var offsets = new long[objects.Length + 1];
        var pdf = new System.Text.StringBuilder(prefix);
        for (var index = 0; index < objects.Length; index++)
        {
            offsets[index + 1] = System.Text.Encoding.ASCII.GetByteCount(pdf.ToString());
            pdf.Append(objects[index]);
        }

        var xrefOffset = System.Text.Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 5\n0000000000 65535 f \n");
        for (var index = 1; index < offsets.Length; index++)
        {
            pdf.Append($"{offsets[index]:0000000000} 00000 n \n");
        }

        pdf.Append("trailer\n<< /Size 5 /Root 1 0 R >>\n");
        pdf.Append($"startxref\n{xrefOffset}\n%%EOF");
        return pdf.ToString();
    }
}