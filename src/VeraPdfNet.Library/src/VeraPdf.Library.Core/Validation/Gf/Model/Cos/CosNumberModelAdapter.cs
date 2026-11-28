using VeraPdf.Library.Xmp;
using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public abstract class CosNumberModelAdapter : GenericModelObject, ICosNumber
{
    private readonly long _integerValue;
    private readonly double _realValue;

    private CosNumberModelAdapter(string objectType, long integerValue, double realValue)
        : base(objectType)
    {
        _integerValue = integerValue;
        _realValue = realValue;
    }

    public string GetstringValue() => XmpScalarConverter.FromDouble(_realValue);
    public long? GetintValue() => _integerValue;
    public double? GetrealValue() => _realValue;

    public static ICosNumber? FromParserValue(CosValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            CosInteger integer => new IntegerAdapter(integer.Value),
            CosReal real => new RealAdapter(real.Value),
            _ => null
        };
    }

    private static long TruncateToInt64(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }

        if (value >= long.MaxValue)
        {
            return long.MaxValue;
        }

        if (value <= long.MinValue)
        {
            return long.MinValue;
        }

        return (long)value;
    }

    private sealed class IntegerAdapter(long value)
        : CosNumberModelAdapter("CosInteger", value, value), ICosInteger
    {
    }

    private sealed class RealAdapter(double value)
        : CosNumberModelAdapter("CosReal", TruncateToInt64(value), value), ICosReal
    {
    }
}
