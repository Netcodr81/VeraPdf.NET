using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpSchemaRegistryTests
{
    [Fact]
    public void StandardMappingsMatchJavaReference()
    {
        var registry = new XmpSchemaRegistry();
        var expected = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-registry-reference.tsv"));
        var actual = registry.GetNamespaces().Select(pair => $"N\t{pair.Key}\t{pair.Value}")
            .Concat(registry.GetAliases().Select(pair =>
                $"A\t{pair.Key}\t{pair.Value.NamespaceUri}\t{pair.Value.Prefix}\t{pair.Value.PropertyName}\t{pair.Value.Form}"));
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RegistersUniquePrefixesAndReturnsSnapshotCopies()
    {
        var registry = new XmpSchemaRegistry();
        var before = registry.GetNamespaces();
        Assert.Equal("pdf_1_:", registry.RegisterNamespace("urn:first", "pdf"));
        Assert.Equal("pdf_2_:", registry.RegisterNamespace("urn:second", "pdf:"));
        Assert.Equal("pdf_1_:", registry.RegisterNamespace("urn:first", "ignored"));
        Assert.Equal("urn:first", registry.GetNamespaceUri("pdf_1_"));
        Assert.DoesNotContain("urn:first", before.Keys);
        registry.DeleteNamespace("urn:first");
        Assert.Null(registry.GetNamespacePrefix("urn:first"));
        Assert.Null(registry.GetNamespaceUri("pdf_1_"));
        Assert.Equal("pdf_1_:", registry.RegisterNamespace("urn:third", "pdf"));
    }

    [Theory]
    [InlineData("1abc")]
    [InlineData("a:b")]
    [InlineData("a b")]
    public void InvalidPrefixesAreRejected(string prefix)
    {
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentException>(() => registry.RegisterNamespace("urn:test", prefix));
    }

    [Fact]
    public void SurrogateInPrefixIsRejected()
    {
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentException>(() => registry.RegisterNamespace("urn:test", "a" + '\ud800'));
    }

    [Fact]
    public void AliasesResolveThroughRegisteredPrefixes()
    {
        var registry = new XmpSchemaRegistry();
        const string pdf = "http://ns.adobe.com/pdf/1.3/";
        var alias = registry.ResolveAlias(pdf, "Title");
        Assert.Equal(new XmpAlias("http://purl.org/dc/elements/1.1/", "dc:", "title",
            XmpAliasForm.AltTextDefaultItem), alias);
        Assert.Equal(alias, registry.FindAlias("pdf:Title"));
        Assert.Equal(7, registry.FindAliases(pdf).Count);
        registry.DeleteNamespace(pdf);
        Assert.Null(registry.ResolveAlias(pdf, "Title"));
        Assert.Equal(alias, registry.FindAlias("pdf:Title"));
    }

    [Fact]
    public void CustomMappingsAreThreadLocalAndSharedBetweenInstances()
    {
        var first = new XmpSchemaRegistry();
        var second = new XmpSchemaRegistry();
        first.RegisterNamespace("urn:thread", "test");
        Assert.Equal("test:", second.GetNamespacePrefix("urn:thread"));

        string? found = "not run";
        var thread = new Thread(() =>
        {
            found = first.GetNamespacePrefix("urn:thread");
            first.RegisterNamespace("urn:thread", "other");
        });
        thread.Start();
        thread.Join();
        Assert.Null(found);
        Assert.Equal("test:", first.GetNamespacePrefix("urn:thread"));
        _ = new XmpSchemaRegistry();
        Assert.Null(first.GetNamespacePrefix("urn:thread"));
    }

    [Fact]
    public void StandardMappingsAreSharedAcrossThreadsOfOneInstanceOnly()
    {
        var first = new XmpSchemaRegistry();
        var second = new XmpSchemaRegistry();
        first.RegisterNamespace("urn:standard", "custom", true);
        string? found = null;
        var thread = new Thread(() => found = first.GetNamespacePrefix("urn:standard"));
        thread.Start();
        thread.Join();
        Assert.Equal("custom:", found);
        Assert.Null(second.GetNamespacePrefix("urn:standard"));
    }
}
