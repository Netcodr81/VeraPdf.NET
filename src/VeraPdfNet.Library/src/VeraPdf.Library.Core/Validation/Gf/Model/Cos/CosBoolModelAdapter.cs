using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosBoolModelAdapter : GenericModelObject, ICosBool
{
    public static ICosBool True { get; } = new CosBoolModelAdapter(true);
    public static ICosBool False { get; } = new CosBoolModelAdapter(false);

    private readonly bool _value;

    private CosBoolModelAdapter(bool value)
        : base("CosBool")
    {
        _value = value;
    }

    public bool? Getvalue() => _value;

    public static ICosBool ValueOf(CosBoolean value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Value ? True : False;
    }

    public static ICosBool? FromParserValue(CosValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value is CosBoolean boolean ? ValueOf(boolean) : null;
    }
}
