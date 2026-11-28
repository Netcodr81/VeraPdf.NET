using System.Text;
using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosNameModelAdapterTests
{
    [Fact]
    public void AdaptsDecodedNameBytesToTheCosNameContract()
    {
        var adapted = CosNameModelAdapter.FromParserValue(new CosName("Type Name"u8.ToArray()));

        Assert.IsAssignableFrom<ICosName>(adapted);
        Assert.Equal("CosName", ((IModelObject)adapted!).GetObjectType());
        Assert.Equal("Type Name", adapted!.GetinternalRepresentation());
    }

    [Fact]
    public void PreservesTheParserAndJavaInternalLatin1Representation()
    {
        var bytes = new byte[] { 0xC3, 0xA9 };
        var adapted = CosNameModelAdapter.FromParserValue(new CosName(bytes));

        Assert.Equal(Encoding.Latin1.GetString(bytes), adapted!.GetinternalRepresentation());
    }

    [Fact]
    public void ReturnsNullForNonNameParserValues()
    {
        Assert.Null(CosNameModelAdapter.FromParserValue(new CosBoolean(true)));
        Assert.Throws<ArgumentNullException>(() => CosNameModelAdapter.FromParserValue(null!));
    }
}
