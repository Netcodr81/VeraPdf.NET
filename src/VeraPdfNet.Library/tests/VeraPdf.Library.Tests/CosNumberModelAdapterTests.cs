using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosNumberModelAdapterTests
{
    [Fact]
    public void AdaptsIntegerValuesToTheCosIntegerContract()
    {
        var adapted = CosNumberModelAdapter.FromParserValue(new CosInteger(42));

        Assert.IsAssignableFrom<ICosInteger>(adapted);
        Assert.Equal("CosInteger", ((VeraPdf.Model.BaseLayer.IModelObject)adapted!).GetObjectType());
        Assert.Equal("42.0", adapted!.GetstringValue());
        Assert.Equal(42, adapted.GetintValue());
        Assert.Equal(42.0, adapted.GetrealValue());
    }

    [Fact]
    public void AdaptsRealValuesWithTruncatedIntegerProjection()
    {
        var adapted = CosNumberModelAdapter.FromParserValue(new CosReal(-2.75));

        Assert.IsAssignableFrom<ICosReal>(adapted);
        Assert.Equal("CosReal", ((VeraPdf.Model.BaseLayer.IModelObject)adapted!).GetObjectType());
        Assert.Equal("-2.75", adapted!.GetstringValue());
        Assert.Equal(-2, adapted.GetintValue());
        Assert.Equal(-2.75, adapted.GetrealValue());
    }

    [Fact]
    public void ReturnsNullForNonNumberParserValues()
    {
        Assert.Null(CosNumberModelAdapter.FromParserValue(new CosBoolean(true)));
        Assert.Throws<ArgumentNullException>(() => CosNumberModelAdapter.FromParserValue(null!));
    }

    [Theory]
    [InlineData(double.PositiveInfinity, long.MaxValue)]
    [InlineData(double.NegativeInfinity, long.MinValue)]
    [InlineData(double.NaN, 0)]
    public void FollowsJavaLongCastBehaviorOutsideFiniteIntegerRanges(double value, long expected)
    {
        var adapted = CosNumberModelAdapter.FromParserValue(new CosReal(value));

        Assert.Equal(expected, adapted!.GetintValue());
    }
}
