using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Model.XMPLayer;

namespace VeraPdf.Model.Tests;

public sealed class ModelHelperTests
{
    [Fact]
    public void InheritedNamesFollowUpstreamDerivedToBaseOrder()
    {
        Assert.Equal(["CosNumber", "CosObject", "Object"], ModelHelper.GetListOfSuperNames("CosInteger"));
        Assert.Equal(["stringValue", "intValue", "realValue"], ModelHelper.GetListOfProperties("CosInteger"));
        Assert.Empty(ModelHelper.GetListOfSuperNames("Object"));
        Assert.Empty(ModelHelper.GetListOfProperties("Object"));
        var metadata = ModelHelper.GetDefinition("CosDict");
        Assert.Equal(["keys", "values", "metadata"], metadata.Links.Select(l => l.Name));
        Assert.Equal("?", metadata.Links[2].Multiplicity);
        Assert.Equal("CosDict", metadata.Name);
    }

    [Fact]
    public void NamesAndMetadataAreImmutableAndOrdinal()
    {
        Assert.Contains("PDDocument", ModelHelper.GetTypes());
        Assert.DoesNotContain("pddocument", ModelHelper.GetTypes());
        Assert.Throws<ArgumentException>(() => ModelHelper.GetDefinition("pddocument"));
        Assert.Throws<ArgumentException>(() => ModelHelper.GetListOfLinks("missing"));
        var names = Assert.IsAssignableFrom<IList<string>>(ModelHelper.GetListOfProperties("CosBool"));
        Assert.Throws<NotSupportedException>(() => names.Add("injected"));
        var definitions = Assert.IsAssignableFrom<IList<ModelPropertyDefinition>>(ModelHelper.GetDefinition("CosBool").Properties);
        Assert.Throws<NotSupportedException>(() => definitions.Clear());
        Assert.Equal(["value"], ModelHelper.GetListOfProperties("CosBool"));
    }

    [Fact]
    public void MinimalConsumerPreservesNullableFactsAndContextDefaults()
    {
        ICosBool value = new MissingBoolean();
        Assert.Null(value.Getvalue());
        Assert.Equal("CosBool", value.GetObjectType());
        Assert.Null(value.GetID());
        Assert.Null(value.GetContext());
        Assert.Null(value.GetExtraContext());
        Assert.False(value.IsContextDependent());
        Assert.Throws<InvalidOperationException>(() => value.GetLinkedObjects("missing"));
        var context = new MissingBoolean();
        context.MarkUnknown();
        Assert.Null(context.IsContextDependent());
    }

    [Fact]
    public void NamedLinkCanBeEmptyWithoutSynthesizingNominalRequiredObjects()
    {
        ICosIndirect indirect = new EmptyIndirect();
        Assert.Empty(indirect.GetLinkedObjects("directObject"));
        Assert.Equal("", Assert.Single(ModelHelper.GetDefinition("CosIndirect").Links).Multiplicity);
        IModelObject package = new EmptyPackage();
        Assert.Empty(package.GetLinkedObjects("Properties"));
        Assert.Throws<ArgumentException>(() => package.GetLinkedObjects("properties"));
        Assert.Equal(["Properties", "ExtensionSchemasContainers"], package.GetLinks());
    }

    private sealed class MissingBoolean() : GenericModelObject("CosBool"), ICosBool
    {
        public bool? Getvalue() => null;
        public void MarkUnknown() => ContextDependent = null;
    }

    private sealed class EmptyIndirect() : GenericModelObject("CosIndirect"), ICosIndirect
    {
        public bool? GetspacingCompliesPDFA() => null;
        public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
            linkName == "directObject" ? [] : throw new ArgumentException($"Unknown link '{linkName}'.", nameof(linkName));
    }

    private sealed class EmptyPackage() : GenericModelObject("XMPPackage"), IXMPPackage
    {
        public bool? GetisSerializationValid() => null;
        public string? GetactualEncoding() => null;
        public string? Getbytes() => null;
        public string? Getencoding() => null;
        public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
            linkName is "Properties" or "ExtensionSchemasContainers" ? [] :
            throw new ArgumentException($"Unknown link '{linkName}'.", nameof(linkName));
    }
}
