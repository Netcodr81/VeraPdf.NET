using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosNullModelAdapter : GenericModelObject, ICosNull
{
    public static ICosNull Instance { get; } = new CosNullModelAdapter();

    private CosNullModelAdapter()
        : base("CosNull")
    {
    }

    public static ICosNull? FromParserValue(CosValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value is CosNull ? Instance : null;
    }
}
