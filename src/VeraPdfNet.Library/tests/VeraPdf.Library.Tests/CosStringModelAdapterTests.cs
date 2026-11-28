using System.Text;
using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosStringModelAdapterTests
{
    [Fact]
    public void AdaptsStringBytesAndHexMetadata()
    {
        var adapted = CosStringModelAdapter.FromParserValue(
            new CosString(Encoding.UTF8.GetBytes("caf\u00E9"), true, false, 7));

        Assert.IsAssignableFrom<ICosString>(adapted);
        Assert.Equal("CosString", ((IModelObject)adapted!).GetObjectType());
        Assert.Equal("caf\u00E9", adapted!.Getvalue());
        Assert.True(adapted.GetisHex());
        Assert.False(adapted.GetcontainsOnlyHex());
        Assert.Equal(7, adapted.GethexCount());
        Assert.False(adapted.GetcontainsPUA());
    }

    [Fact]
    public void DetectsPrivateUseCharactersUsingPdfStringTextDecoding()
    {
        var bmp = CosStringModelAdapter.FromParserValue(
            new CosString([0xFE, 0xFF, 0xE0, 0x00]));
        var supplementary = CosStringModelAdapter.FromParserValue(
            new CosString([0xFE, 0xFF, 0xDB, 0x80, 0xDC, 0x00]));

        Assert.True(bmp!.GetcontainsPUA());
        Assert.True(supplementary!.GetcontainsPUA());
    }

    [Fact]
    public void ReturnsNullForNonStringParserValues()
    {
        Assert.Null(CosStringModelAdapter.FromParserValue(new CosBoolean(false)));
        Assert.Throws<ArgumentNullException>(() => CosStringModelAdapter.FromParserValue(null!));
    }
}
