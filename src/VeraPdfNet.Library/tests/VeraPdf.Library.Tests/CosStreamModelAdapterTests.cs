using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosStreamModelAdapterTests
{
    [Fact]
    public void ExposesDeclaredAndStoredLengthAndDictionaryLinks()
    {
        var dictionary = new CosDictionary();
        dictionary.Set("Length", new CosInteger(9));
        dictionary.Set("Type", new CosName("XRef"u8.ToArray()));
        var stream = new CosStream(dictionary, [1, 2, 3])
        {
            StreamKeywordCRLFCompliant = true,
            EndstreamKeywordEOLCompliant = false
        };

        var adapted = CosStreamModelAdapter.FromParserValue(stream);

        Assert.Equal(2, adapted.Getsize());
        Assert.Equal("Length&Type", adapted.GetkeysString());
        Assert.Equal(9, adapted.GetLength());
        Assert.Equal(3, adapted.GetrealLength());
        Assert.Null(adapted.GetF());
        Assert.Null(adapted.GetFFilter());
        Assert.Null(adapted.GetFDecodeParms());
        Assert.True(adapted.GetstreamKeywordCRLFCompliant());
        Assert.False(adapted.GetendstreamKeywordEOLCompliant());
        Assert.Equal(2, adapted.GetLinkedObjects("keys").Count);
        Assert.Equal(2, adapted.GetLinkedObjects("values").Count);
        Assert.Empty(adapted.GetLinkedObjects("filters"));
    }

    [Fact]
    public void HandlesMissingOrNonNumericDeclaredLength()
    {
        var dictionary = new CosDictionary();
        dictionary.Set("Length", new CosName("invalid"u8.ToArray()));

        var adapted = CosStreamModelAdapter.FromParserValue(new CosStream(dictionary, [1, 2]));

        Assert.Null(adapted.GetLength());
        Assert.Equal(2, adapted.GetrealLength());
        Assert.Null(adapted.GetstreamKeywordCRLFCompliant());
        Assert.Null(adapted.GetendstreamKeywordEOLCompliant());
    }

    [Fact]
    public void ExposesExternalFileFields()
    {
        var dictionary = new CosDictionary();
        dictionary.Set("Filter", new CosName("FlateDecode"u8.ToArray()));
        dictionary.Set("F", new CosString("external"u8.ToArray()));
        var externalFilters = new CosArray();
        externalFilters.Add(new CosName("FlateDecode"u8.ToArray()));
        externalFilters.Add(new CosName("ASCII85Decode"u8.ToArray()));
        dictionary.Set("FFilter", externalFilters);
        var externalDecodeParameters = new CosDictionary();
        externalDecodeParameters.Set("Predictor", new CosInteger(12));
        dictionary.Set("FDecodeParms", externalDecodeParameters);
        var adapted = CosStreamModelAdapter.FromParserValue(new CosStream(dictionary, []));

        Assert.Equal("external", adapted.GetF());
        Assert.Equal("FlateDecode ASCII85Decode", adapted.GetFFilter());
        Assert.Equal("dictionary(size = 1)", adapted.GetFDecodeParms());
        var filter = Assert.IsAssignableFrom<ICosFilter>(
            Assert.Single(adapted.GetLinkedObjects("filters")));
        Assert.Equal("FlateDecode", filter.GetinternalRepresentation());
        Assert.Equal("Default", filter.GetdecodeParms());
    }

    [Fact]
    public void PreservesFilterOrderAndDecodeParameterClassification()
    {
        var cryptParameters = new CosDictionary();
        cryptParameters.Set("Name", new CosName("Identity"u8.ToArray()));
        var customParameters = new CosDictionary();
        customParameters.Set("Predictor", new CosInteger(12));
        var parameters = new CosArray();
        parameters.Add(cryptParameters);
        parameters.Add(customParameters);
        var names = new CosArray();
        names.Add(new CosName("Crypt"u8.ToArray()));
        names.Add(new CosName("FlateDecode"u8.ToArray()));
        var dictionary = new CosDictionary();
        dictionary.Set("Filter", names);
        dictionary.Set("DecodeParms", parameters);
        var adapted = CosStreamModelAdapter.FromParserValue(new CosStream(dictionary, []));

        var filters = adapted.GetLinkedObjects("filters")
            .Cast<ICosFilter>()
            .ToArray();

        Assert.Collection(
            filters,
            filter =>
            {
                Assert.Equal("Crypt", filter.GetinternalRepresentation());
                Assert.Equal("Identity", filter.GetdecodeParms());
            },
            filter =>
            {
                Assert.Equal("FlateDecode", filter.GetinternalRepresentation());
                Assert.Equal("Custom", filter.GetdecodeParms());
            });
    }

    [Fact]
    public void RejectsMalformedFilterNames()
    {
        var names = new CosArray();
        names.Add(new CosInteger(1));
        var dictionary = new CosDictionary();
        dictionary.Set("Filter", names);
        var adapted = CosStreamModelAdapter.FromParserValue(new CosStream(dictionary, []));

        Assert.Throws<InvalidDataException>(() => adapted.GetLinkedObjects("filters"));
    }
}
