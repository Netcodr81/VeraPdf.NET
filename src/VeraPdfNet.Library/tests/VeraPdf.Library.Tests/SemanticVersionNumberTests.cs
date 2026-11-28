using VeraPdf.Library.Versioning;

namespace VeraPdf.Library.Tests;

public class SemanticVersionNumberTests
{
    [Fact]
    public void ParsesCoreVersionAndIgnoresPrereleaseAndBuildMetadata()
    {
        var version = Versions.FromString("1.2.3-rc.1+build.5");

        Assert.Equal("1.2.3", version.VersionString);
        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal(0, version.CompareTo(Versions.FromString("1.2.3")));
    }

    [Fact]
    public void ReleaseParserAcceptsVPrefixAndSnapshotSuffix()
    {
        Assert.Equal("1.2.3", Versions.FromString("v1.2.3-SNAPSHOT").VersionString);
        Assert.Equal("1.2.3", Versions.FromString("1.2.3").VersionString);
    }

    [Fact]
    public void StringComponentsDefaultMissingPartsAndIgnoreExtraParts()
    {
        Assert.Equal("1.2.0", Versions.FromString("1.2").VersionString);
        Assert.Equal("1.2.3", Versions.FromString("1.2.3.4").VersionString);
        Assert.Equal("1.0.0", Versions.FromStrings(["1"]).VersionString);
    }

    [Fact]
    public void ComparisonIsLexicographicAndDoesNotOverflow()
    {
        Assert.True(Versions.FromInts(int.MaxValue, 0, 0).CompareTo(Versions.FromInts(0, 0, 0)) > 0);
        Assert.True(Versions.FromInts(1, 0, 0).CompareTo(Versions.FromInts(0, int.MaxValue, 0)) > 0);
        Assert.True(Versions.FromInts(1, 2, 4).CompareTo(Versions.FromInts(1, 2, 3)) > 0);
    }

    [Fact]
    public void RejectsNullEmptyMalformedAndNegativeInputs()
    {
        Assert.Throws<ArgumentNullException>(() => Versions.FromString(null!));
        Assert.Throws<ArgumentException>(() => Versions.FromString(""));
        Assert.Throws<ArgumentException>(() => Versions.FromString("1.bad.3"));
        Assert.Throws<ArgumentException>(() => Versions.FromString("1..3"));
        Assert.Throws<ArgumentException>(() => Versions.FromInts([1, 2]));
        Assert.Throws<ArgumentException>(() => Versions.FromInts([1, 2, 3, 4]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Versions.FromInts(1, -1, 0));
    }
}
