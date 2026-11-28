using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosNullModelAdapterTests
{
    [Fact]
    public void AdaptsPdfNullToTheCanonicalCosNullInstance()
    {
        var adapted = CosNullModelAdapter.FromParserValue(new CosNull());

        Assert.IsAssignableFrom<ICosNull>(adapted);
        Assert.Same(CosNullModelAdapter.Instance, adapted);
        Assert.Equal("CosNull", ((IModelObject)adapted!).GetObjectType());
    }

    [Fact]
    public void ReturnsNullForNonNullParserValues()
    {
        Assert.Null(CosNullModelAdapter.FromParserValue(new CosBoolean(false)));
        Assert.Throws<ArgumentNullException>(() => CosNullModelAdapter.FromParserValue(null!));
    }
}
