using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class FileOutputMapperContractTests
{
    [Fact]
    public void ExposesPrefixSuffixAndMappedFile()
    {
        var original = new FileInfo(Path.Combine("input", "document.pdf"));
        IFileOutputMapper mapper = new StubMapper("checked-", ".log");

        var mapped = mapper.MapFile(original);

        Assert.Equal("checked-", mapper.Prefix);
        Assert.Equal(".log", mapper.Suffix);
        Assert.Equal(Path.GetFullPath(Path.Combine("input", "checked-document.pdf.log")), mapped.FullName);
    }

    private sealed class StubMapper(string prefix, string suffix) : IFileOutputMapper
    {
        public string Prefix { get; } = prefix;
        public string Suffix { get; } = suffix;

        public FileInfo MapFile(FileInfo original)
        {
            ArgumentNullException.ThrowIfNull(original);
            return new FileInfo(Path.Combine(original.DirectoryName!, Prefix + original.Name + Suffix));
        }
    }
}
