using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class FileOutputMappersTests
{
    [Fact]
    public void SiblingFactoriesUseDefaultPrefixAndVersionWhenRequested()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var original = new FileInfo(Path.Combine(root, "document.pdf"));

        try
        {
            var sibling = FileOutputMappers.SibFiles();
            var versioned = FileOutputMappers.VerSibFiles("checked-", "_result");
            var existing = new FileInfo(Path.Combine(root, "checked-document_result.pdf"));
            File.WriteAllText(existing.FullName, "");

            Assert.Equal(Path.Combine(root, "veraPDF_document.pdf"), sibling.MapFile(original).FullName);
            Assert.Equal(Path.Combine(root, "checked-document_result_(0).pdf"), versioned.MapFile(original).FullName);
            Assert.Equal("checked-", versioned.Prefix);
            Assert.Equal("_result", versioned.Suffix);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void SubfolderFactoriesCreateSourceRelativeDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var original = new FileInfo(Path.Combine(root, "document.pdf"));

        try
        {
            var mapper = FileOutputMappers.SubFold(Path.Combine("reports", "nested"), "checked-", ".xml");

            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(root, "reports", "nested", "checked-document.pdf.xml"), mapped.FullName);
            Assert.True(Directory.Exists(Path.GetDirectoryName(mapped.FullName)));
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
    public void RelativeDestinationFactoryUsesConfiguredRootsAndDefaults()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var source = new DirectoryInfo(Path.Combine(root, "source"));
        var destination = new DirectoryInfo(Path.Combine(root, "destination"));
        var original = new FileInfo(Path.Combine(source.FullName, "document.pdf"));
        var mapper = FileOutputMappers.ToRelativeDest(source, destination);

        try
        {
            var mapped = mapper.MapFile(original);

            Assert.Equal(Path.Combine(destination.FullName, "veraPDF_document.pdf"), mapped.FullName);
            Assert.True(destination.Exists);
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
    public void FoldFactoriesCreateAndValidateOutputDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outputPath = Path.Combine(root, "output");

        try
        {
            var mapper = FileOutputMappers.Fold(outputPath, "checked-", "_result");
            var original = new FileInfo(Path.Combine(root, "document.pdf"));

            Assert.Equal(Path.Combine(outputPath, "checked-document_result.pdf"), mapper.MapFile(original).FullName);
            Assert.True(Directory.Exists(outputPath));
            Assert.Throws<ArgumentException>(() => FileOutputMappers.Fold("", "p-", ""));
            Assert.Throws<ArgumentNullException>(() => FileOutputMappers.Fold(Path.Combine(root, "invalid"), null!, ""));
            Assert.False(Directory.Exists(Path.Combine(root, "invalid")));

            var filePath = Path.Combine(root, "not-a-directory");
            File.WriteAllText(filePath, "");
            Assert.Throws<ArgumentException>(() => FileOutputMappers.Fold(filePath, "p-", ""));
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
