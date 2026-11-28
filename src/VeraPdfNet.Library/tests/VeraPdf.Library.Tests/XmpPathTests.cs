using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpPathTests
{
    [Fact]
    public void ExpansionMatchesJavaReference()
    {
        var registry = new XmpSchemaRegistry();
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-path-reference.tsv")))
        {
            var fields = line.Split('\t');
            if (fields[2] == "ERR")
            {
                var exception = Assert.Throws<XmpException>(() => XmpPath.Parse(registry, fields[0], fields[1]));
                Assert.Equal(int.Parse(fields[3]), (int)exception.Error);
                continue;
            }
            var path = XmpPath.Parse(registry, fields[0], fields[1]);
            Assert.Equal(fields[3], path.ToString());
            var segments = string.Join("|", path.Segments.Select(segment =>
                $"{JavaKind(segment.Kind)}:{segment.Name}:{segment.IsAlias.ToString().ToLowerInvariant()}"));
            Assert.Equal(fields[4], segments);
        }
    }

    [Fact]
    public void AliasMetadataIsPreserved()
    {
        var path = XmpPath.Parse(new XmpSchemaRegistry(), "http://ns.adobe.com/pdf/1.3/", "Title");
        Assert.Equal(XmpAliasForm.AltTextDefaultItem, path.Segments[1].AliasForm);
        Assert.Equal(XmpAliasForm.AltTextDefaultItem, path.Segments[2].AliasForm);
        Assert.All(path.Segments.Skip(1), segment => Assert.True(segment.IsAlias));
    }

    [Fact]
    public void PathConstructionMatchesJavaReference()
    {
        var registry = new XmpSchemaRegistry();
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-path-factory-reference.tsv")))
        {
            var fields = line.Split('\t');
            var shortCase = fields[0] is "index" or "language";
            var outcome = fields[shortCase ? 2 : 3];
            string Execute() => fields[0] switch
            {
                "field" => XmpPathFactory.ComposeStructFieldPath(registry, fields[1], fields[2]),
                "qualifier" => XmpPathFactory.ComposeQualifierPath(registry, fields[1], fields[2]),
                "selector" => XmpPathFactory.ComposeFieldSelector(registry, "items", fields[1], fields[2], "value"),
                "language" => XmpPathFactory.ComposeLanguageSelector("items", fields[1]),
                "index" => XmpPathFactory.ComposeArrayItemPath("items", int.Parse(fields[1])),
                _ => throw new InvalidDataException("Unknown reference operation")
            };
            if (outcome == "ERR")
                Assert.Equal(int.Parse(fields[^1]), (int)Assert.Throws<XmpException>(Execute).Error);
            else
                Assert.Equal(fields[^1], Execute());
        }
    }

    [Theory]
    [InlineData("title[")]
    [InlineData("title[field=")]
    [InlineData("title[]")]
    [InlineData("title[3")]
    public void TruncatedPathsFailExplicitly(string expression)
    {
        var error = Assert.Throws<XmpException>(() =>
            XmpPath.Parse(new XmpSchemaRegistry(), "http://purl.org/dc/elements/1.1/", expression));
        Assert.Equal(XmpError.BadPath, error.Error);
    }

    [Fact]
    public void ComposesPathsAndNormalizesLanguages()
    {
        var registry = new XmpSchemaRegistry();
        const string dc = "http://purl.org/dc/elements/1.1/";
        const string pdf = "http://ns.adobe.com/pdf/1.3/";
        Assert.Equal("items[3]", XmpPathFactory.ComposeArrayItemPath("items", 3));
        Assert.Equal("items[last()]", XmpPathFactory.ComposeArrayItemPath("items", -1));
        Assert.Equal(XmpError.BadIndex, Assert.Throws<XmpException>(() => XmpPathFactory.ComposeArrayItemPath("items", -2)).Error);
        Assert.Equal("/dc:title", XmpPathFactory.ComposeStructFieldPath(registry, dc, "title"));
        Assert.Equal("/?dc:title", XmpPathFactory.ComposeQualifierPath(registry, dc, "title"));
        Assert.Equal("items[dc:title=\"value\"]", XmpPathFactory.ComposeFieldSelector(registry, "items", dc, "title", "value"));
        Assert.Equal("items[?xml:lang=\"en-US-extra\"]", XmpPathFactory.ComposeLanguageSelector("items", " En_uS_Extra "));
        Assert.Equal("items[?xml:lang=\"x-default\"]", XmpPathFactory.ComposeLanguageSelector("items", "x-default"));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => XmpPathFactory.ComposeStructFieldPath(registry, pdf, "Title")).Error);
        Assert.Equal("/xmp:CreateDate", XmpPathFactory.ComposeStructFieldPath(registry, pdf, "CreationDate"));
    }

    private static int JavaKind(XmpPathSegmentKind kind) => kind switch
    {
        XmpPathSegmentKind.Schema => int.MinValue,
        XmpPathSegmentKind.Field => 1,
        XmpPathSegmentKind.Qualifier => 2,
        XmpPathSegmentKind.ArrayIndex => 3,
        XmpPathSegmentKind.ArrayLast => 4,
        XmpPathSegmentKind.QualifierSelector => 5,
        XmpPathSegmentKind.FieldSelector => 6,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
