using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosIndirectModelAdapterTests
{
    [Fact]
    public void ExposesStableIdAndDirectObjectLink()
    {
        var adapted = CosIndirectModelAdapter.FromParserValue(
            new CosIndirectObject(new CosKey(12, 3), new CosInteger(42)));

        Assert.Equal("12 3", ((IModelObject)adapted).GetID());
        Assert.Null(adapted.GetspacingCompliesPDFA());

        var directObject = Assert.Single(adapted.GetLinkedObjects("directObject"));
        Assert.IsAssignableFrom<ICosInteger>(directObject);
        Assert.Equal(42, ((ICosInteger)directObject).GetintValue());
    }

    [Fact]
    public void MapsExplicitNullAndBooleanValuesToDirectObjectLinks()
    {
        var explicitNull = CosIndirectModelAdapter.FromParserValue(
            new CosIndirectObject(new CosKey(1, 0), new CosNull()));
        Assert.Same(CosNullModelAdapter.Instance, Assert.Single(explicitNull.GetLinkedObjects("directObject")));

        var unresolved = CosIndirectModelAdapter.FromParserValue(
            new CosIndirectObject(new CosKey(2, 0), new CosBoolean(false)));
        Assert.IsAssignableFrom<ICosBool>(Assert.Single(unresolved.GetLinkedObjects("directObject")));
    }

    [Fact]
    public void RequiresReaderForNestedIndirectReference()
    {
        var indirect = new CosIndirectObject(
            new CosKey(3, 0),
            new CosIndirectReference(4, 0));

        Assert.Throws<InvalidOperationException>(() => CosIndirectModelAdapter.FromParserValue(indirect));
    }

    [Fact]
    public void ExposesNestedArraysAsDirectObjects()
    {
        var indirect = new CosIndirectObject(new CosKey(4, 0), new CosArray());

        var adapted = CosIndirectModelAdapter.FromParserValue(indirect);

        Assert.IsAssignableFrom<ICosArray>(Assert.Single(adapted.GetLinkedObjects("directObject")));
    }
}
