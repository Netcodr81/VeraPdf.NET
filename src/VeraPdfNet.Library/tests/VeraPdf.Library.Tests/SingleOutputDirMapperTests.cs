using VeraPdf.Library.Core.Utils;

namespace VeraPdf.Library.Tests;

public class SingleOutputDirMapperTests
{
    [Fact]
    public void MapsDifferentInputsToOneOutputDirectoryWithoutCreatingIt()
    {
        var outputDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var mapper = SingleOutputDirMapper.FromValues(outputDirectory, "checked-", "_result");
        var firstInput = new FileInfo(Path.Combine("first", "one.pdf"));
        var secondInput = new FileInfo(Path.Combine("second", "two.xml"));

        var firstOutput = mapper.MapFile(firstInput);
        var secondOutput = mapper.MapFile(secondInput);

        Assert.Equal(Path.Combine(outputDirectory.FullName, "checked-one_result.pdf"), firstOutput.FullName);
        Assert.Equal(Path.Combine(outputDirectory.FullName, "checked-two_result.xml"), secondOutput.FullName);
        Assert.False(outputDirectory.Exists);
        Assert.False(firstOutput.Exists);
        Assert.False(secondOutput.Exists);
    }
}
