using VeraPdf.Library.Validation.Gf.Model.Factory.Operators;

namespace VeraPdf.Library.Tests;

public class RenderingModeTests
{
    [Theory]
    [InlineData(0, RenderingMode.Fill, true, false)]
    [InlineData(1, RenderingMode.Stroke, false, true)]
    [InlineData(2, RenderingMode.FillStroke, true, true)]
    [InlineData(3, RenderingMode.Neither, false, false)]
    [InlineData(4, RenderingMode.FillClip, true, false)]
    [InlineData(5, RenderingMode.StrokeClip, false, true)]
    [InlineData(6, RenderingMode.FillStrokeClip, true, true)]
    [InlineData(7, RenderingMode.NeitherClip, false, false)]
    public void MapsPdfRenderingModeValuesAndFlags(
        int value,
        RenderingMode expected,
        bool isFill,
        bool isStroke)
    {
        var mode = RenderingModeExtensions.GetRenderingMode(value);

        Assert.Equal(expected, mode);
        Assert.Equal(value, expected.GetValue());
        Assert.Equal(isFill, expected.IsFill());
        Assert.Equal(isStroke, expected.IsStroke());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(8)]
    public void ReturnsNullForValuesOutsideThePdfModeRange(int value)
    {
        Assert.Null(RenderingModeExtensions.GetRenderingMode(value));
    }

    [Fact]
    public void RejectsUndefinedEnumValuesForClassification()
    {
        var undefined = (RenderingMode)8;

        Assert.Throws<ArgumentOutOfRangeException>(() => undefined.IsFill());
        Assert.Throws<ArgumentOutOfRangeException>(() => undefined.IsStroke());
        Assert.Throws<ArgumentOutOfRangeException>(() => undefined.GetValue());
    }
}
