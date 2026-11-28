using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosBoolModelAdapterTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AdaptsBooleanValuesToCanonicalCosBoolInstances(bool value)
    {
        var adapted = CosBoolModelAdapter.FromParserValue(new CosBoolean(value));
        var expected = value ? CosBoolModelAdapter.True : CosBoolModelAdapter.False;

        Assert.IsAssignableFrom<ICosBool>(adapted);
        Assert.Same(expected, adapted);
        Assert.Equal("CosBool", ((IModelObject)adapted!).GetObjectType());
        Assert.Equal(value, adapted!.Getvalue());
    }

    [Fact]
    public void ReturnsNullForNonBooleanParserValues()
    {
        Assert.Null(CosBoolModelAdapter.FromParserValue(new CosInteger(1)));
        Assert.Throws<ArgumentNullException>(() => CosBoolModelAdapter.FromParserValue(null!));
        Assert.Throws<ArgumentNullException>(() => CosBoolModelAdapter.ValueOf(null!));
    }
}
