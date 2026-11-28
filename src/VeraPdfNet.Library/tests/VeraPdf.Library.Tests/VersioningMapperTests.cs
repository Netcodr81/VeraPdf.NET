using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class VersioningMapperTests
{
    [Fact]
    public void ReturnsMappedPathWhenItDoesNotExist()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var original = new FileInfo(Path.Combine(root, "document.pdf"));
        var mapper = VersioningMapper.DefaultInstance;

        var mapped = mapper.MapFile(original);

        Assert.Equal(Path.Combine(root, "veraPDF_document.pdf"), mapped.FullName);
        Assert.False(mapped.Exists);
    }

    [Fact]
    public void ChoosesNextVersionAfterExistingMappedFileAndVersionedSiblings()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var original = new FileInfo(Path.Combine(root, "document.pdf"));
        var mapper = VersioningMapper.FromMapper(
            RelativeDirectoryMapper.WithPrefixAndSuffix("checked-", "_result"));

        try
        {
            File.WriteAllText(Path.Combine(root, "checked-document_result.pdf"), "");
            File.WriteAllText(Path.Combine(root, "checked-document_result_(0).pdf"), "");
            File.WriteAllText(Path.Combine(root, "checked-document_result_(4).pdf"), "");
            File.WriteAllText(Path.Combine(root, "checked-document_result_(invalid).pdf"), "");
            File.WriteAllText(Path.Combine(root, "checked-document_result_(5).txt"), "");

            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(root, "checked-document_result_(5).pdf"), mapped.FullName);
            Assert.Equal("checked-", mapper.Prefix);
            Assert.Equal("_result", mapper.Suffix);
            Assert.False(mapped.Exists);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void InsertsVersionBeforeExtensionAndTreatsDotfilesAsExtensionless()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var mapper = VersioningMapper.FromMapper(RelativeDirectoryMapper.WithPrefixAndSuffix("", ""));

        try
        {
            var pdf = new FileInfo(Path.Combine(root, "document.pdf"));
            File.WriteAllText(pdf.FullName, "");
            Assert.Equal(Path.Combine(root, "document_(0).pdf"), mapper.MapFile(pdf).FullName);

            var dotFile = new FileInfo(Path.Combine(root, ".profile"));
            File.WriteAllText(dotFile.FullName, "");
            Assert.Equal(Path.Combine(root, ".profile_(0)"), mapper.MapFile(dotFile).FullName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
