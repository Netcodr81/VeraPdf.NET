using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosArrayModelAdapterTests
{
    [Fact]
    public void ExposesSizeAndReadOnlyElementsLink()
    {
        var value = new CosArray();
        value.Add(new CosInteger(4));
        value.Add(new CosBoolean(true));
        value.Add(new CosNull());

        var adapted = CosArrayModelAdapter.FromParserValue(value);

        Assert.Equal(3, adapted.Getsize());
        var elements = adapted.GetLinkedObjects("elements");
        Assert.Collection(
            elements,
            element => Assert.Equal(4, Assert.IsAssignableFrom<ICosInteger>(element).GetintValue()),
            element => Assert.True(Assert.IsAssignableFrom<ICosBool>(element).Getvalue()),
            element => Assert.IsAssignableFrom<ICosNull>(element));
        Assert.Throws<NotSupportedException>(
            () => ((IList<IModelObject>)elements).Add(CosNullModelAdapter.Instance));
    }

    [Fact]
    public void PreservesDeclaredSizeWhileOmittingNullSlots()
    {
        var value = new CosArray();
        value.Add(new CosInteger(1));
        value.Add(null!);

        var adapted = CosArrayModelAdapter.FromParserValue(value);

        Assert.Equal(2, adapted.Getsize());
        Assert.Single(adapted.GetLinkedObjects("elements"));
    }

    [Fact]
    public void ExposesNestedArraysAndRejectsUnknownLinks()
    {
        var nested = new CosArray();
        nested.Add(new CosInteger(1));
        var value = new CosArray();
        value.Add(nested);

        var adapted = CosArrayModelAdapter.FromParserValue(value);
        var child = Assert.IsAssignableFrom<ICosArray>(
            Assert.Single(adapted.GetLinkedObjects("elements")));
        Assert.Equal(1, child.Getsize());
        Assert.Equal(
            1,
            Assert.IsAssignableFrom<ICosInteger>(
                Assert.Single(child.GetLinkedObjects("elements"))).GetintValue());
        Assert.Throws<InvalidOperationException>(
            () => CosArrayModelAdapter.FromParserValue(new CosArray()).GetLinkedObjects("unknown"));
    }
}
