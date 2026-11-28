using VeraPdf.Library.Components;

namespace VeraPdf.Library.Tests;

public class ComponentDetailsTests
{
    [Fact]
    public void ExposesTheReferenceDefaultDetails()
    {
        Assert.Equal(new Uri("http://component.verapdf.org#default"), ComponentDetails.Default.Id);
        Assert.Equal("name", ComponentDetails.Default.Name);
        Assert.Equal("version", ComponentDetails.Default.Version);
        Assert.Equal("provider", ComponentDetails.Default.Provider);
        Assert.Equal("description", ComponentDetails.Default.Description);
        Assert.Same(ComponentDetails.Default, ComponentDetails.Default);
    }

    [Fact]
    public void ValuesCompareByAllFields()
    {
        var id = new Uri("urn:verapdf:component:example");
        var first = ComponentDetails.FromValues(id, "parser", "1.2.3", "veraPDF", "PDF parser");
        var same = ComponentDetails.FromValues(id, "parser", "1.2.3", "veraPDF", "PDF parser");
        var different = ComponentDetails.FromValues(id, "parser", "1.2.4", "veraPDF", "PDF parser");

        Assert.Equal(first, same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(first, different);
    }
}
