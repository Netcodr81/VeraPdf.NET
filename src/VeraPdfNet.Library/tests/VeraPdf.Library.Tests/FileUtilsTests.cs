using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class FileUtilsTests
{
    [Theory]
    [InlineData("document", "")]
    [InlineData("document.pdf", "pdf")]
    [InlineData("document.tar.gz", "gz")]
    [InlineData(".hidden", "hidden")]
    [InlineData("document.", "document.")]
    public void ExtractsExtensionUsingLastDot(string fileName, string expected)
    {
        Assert.Equal(expected, FileUtils.ExtFromFileName(fileName));
    }

    [Fact]
    public void ExtensionMatchingIsExactOrOrdinalIgnoreCase()
    {
        Assert.True(FileUtils.HasExt("report.pdf", "pdf"));
        Assert.True(FileUtils.HasExt("report.pdf", ".pdf"));
        Assert.False(FileUtils.HasExt("report.PDF", "pdf"));
        Assert.True(FileUtils.HasExtNoCase("report.PDF", "pdf"));
        Assert.False(FileUtils.HasExt("report.pdfx", "pdf"));
    }

    [Theory]
    [InlineData("report", "sch", "report.sch")]
    [InlineData("report.xml", "sch", "report.sch")]
    [InlineData("report.schema", "sch", "report.sch")]
    [InlineData("./report.xml", "sch", "./report.sch")]
    [InlineData("../report.xml", "sch", "../report.sch")]
    [InlineData("C:\\folder.with.dots\\report.xml", "sch", "C:\\folder.with.dots\\report.sch")]
    [InlineData("/folder.with.dots/report.xml", "sch", "/folder.with.dots/report.sch")]
    public void AddsOrReplacesExtensionWithoutChangingDirectories(string path, string extension, string expected)
    {
        Assert.Equal(expected, FileUtils.AddExt(path, extension));
    }
}
