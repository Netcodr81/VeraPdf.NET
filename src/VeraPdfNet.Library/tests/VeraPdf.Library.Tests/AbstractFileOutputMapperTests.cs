using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class AbstractFileOutputMapperTests
{
    [Theory]
    [InlineData("document.pdf", "pre-", "", "pre-document.pdf")]
    [InlineData("document.tar.gz", "pre-", "_checked", "pre-document.tar_checked.gz")]
    [InlineData("document.pdf", "pre-", ".bak", "pre-document.pdf.bak")]
    [InlineData("document", "pre-", "_checked", "pre-document_checked")]
    [InlineData(".hidden", "pre-", "_checked", "pre-.hidden_checked")]
    [InlineData(".hidden.txt", "pre-", "_checked", "pre-.hidden_checked.txt")]
    public void AddsPrefixAndPlacesSuffixAccordingToExtension(
        string fileName, string prefix, string suffix, string expected)
    {
        var mapper = new TestMapper(prefix, suffix);

        Assert.Equal(expected, mapper.MapFile(new FileInfo(fileName)).Name);
    }

    [Fact]
    public void UsesReferenceDefaultsAndDisplay()
    {
        var mapper = new TestMapper();

        Assert.Equal("veraPDF_", mapper.Prefix);
        Assert.Equal(string.Empty, mapper.Suffix);
        Assert.Equal("AbstractFileOutputMapper [prefix=veraPDF_, suffix=]", mapper.ToString());
    }

    private sealed class TestMapper : AbstractFileOutputMapper
    {
        public TestMapper() { }

        public TestMapper(string prefix, string suffix) : base(prefix, suffix) { }

        protected override FileInfo DoMapFile(FileInfo original) =>
            new(AddPrefixAndSuffix(original, this));
    }
}
