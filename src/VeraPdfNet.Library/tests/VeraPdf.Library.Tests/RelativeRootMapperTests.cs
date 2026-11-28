using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class RelativeRootMapperTests
{
    [Fact]
    public void MapsFromInputDirectoryToConfiguredSourceRootRelativeToDestination()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sourceDirectory = new DirectoryInfo(Path.Combine(root, "source"));
        var destinationRoot = new DirectoryInfo(Path.Combine(root, "destination"));
        var original = new FileInfo(Path.Combine(sourceDirectory.FullName, "document.pdf"));
        var mapper = RelativeRootMapper.FromValues(sourceDirectory, destinationRoot, "checked-", "_result");

        try
        {
            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(destinationRoot.FullName, "checked-document_result.pdf"), mapped.FullName);
            Assert.True(destinationRoot.Exists);
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
    public void ResolvesConfiguredSourceRootRelativeToInputDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var sourceRoot = new DirectoryInfo(Path.Combine(root, "source"));
        var inputDirectory = Path.Combine(sourceRoot.FullName, "nested");
        var destinationRoot = new DirectoryInfo(Path.Combine(root, "destination"));
        var original = new FileInfo(Path.Combine(inputDirectory, "document.pdf"));
        var mapper = RelativeRootMapper.FromValues(sourceRoot, destinationRoot, "veraPDF_", "");

        try
        {
            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(root, "veraPDF_document.pdf"), mapped.FullName);
            Assert.True(Directory.Exists(root));
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
}
