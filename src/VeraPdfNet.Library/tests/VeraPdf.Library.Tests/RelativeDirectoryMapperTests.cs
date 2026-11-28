using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class RelativeDirectoryMapperTests
{
    [Fact]
    public void DefaultMapperPreservesSourceDirectoryAndUsesSingletonDefaults()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var original = new FileInfo(Path.Combine(root, "input.pdf"));

        var mapped = RelativeDirectoryMapper.DefaultInstance.MapFile(original);

        Assert.Same(RelativeDirectoryMapper.DefaultInstance, RelativeDirectoryMapper.DefaultInstance);
        Assert.Equal(Path.Combine(root, "veraPDF_input.pdf"), mapped.FullName);
        Assert.False(mapped.Exists);
    }

    [Fact]
    public void ConfiguredMapperCreatesRelativeOutputDirectoryAndAppliesSuffixes()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var original = new FileInfo(Path.Combine(root, "source", "document.pdf"));
        var mapper = RelativeDirectoryMapper.FromValues("reports/nested", "checked-", "_validation");

        try
        {
            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(root, "source", "reports", "nested", "checked-document_validation.pdf"),
                mapped.FullName);
            Assert.True(Directory.Exists(Path.GetDirectoryName(mapped.FullName)));
            Assert.False(mapped.Exists);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void FactoryWithPrefixAndSuffixUsesSourceDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var original = new FileInfo(Path.Combine(root, "document.pdf"));

        var mapped = RelativeDirectoryMapper.WithPrefixAndSuffix("p-", ".bak").MapFile(original);

        Assert.Equal(Path.Combine(root, "p-document.pdf.bak"), mapped.FullName);
    }
}
