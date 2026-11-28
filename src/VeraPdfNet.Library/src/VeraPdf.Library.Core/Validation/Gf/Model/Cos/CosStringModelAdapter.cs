using System.Text;
using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosStringModelAdapter : GenericModelObject, ICosString
{
    private readonly string _value;
    private readonly bool _isHex;
    private readonly bool _containsOnlyHex;
    private readonly long _hexCount;
    private readonly bool _containsPua;

    private CosStringModelAdapter(CosString value)
        : base("CosString")
    {
        ArgumentNullException.ThrowIfNull(value.Value);
        // GFCosString.getvalue uses Java's default String(byte[]) projection.
        _value = Encoding.UTF8.GetString(value.Value);
        _isHex = value.IsHex;
        _containsOnlyHex = value.ContainsOnlyHex;
        _hexCount = value.HexCount;
        _containsPua = ContainsPrivateUseArea(value.Text);
    }

    public string? Getvalue() => _value;
    public bool? GetisHex() => _isHex;
    public bool? GetcontainsOnlyHex() => _containsOnlyHex;
    public long? GethexCount() => _hexCount;
    public bool? GetcontainsPUA() => _containsPua;

    public static ICosString? FromParserValue(CosValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value is CosString text ? new CosStringModelAdapter(text) : null;
    }

    private static bool ContainsPrivateUseArea(string value)
    {
        foreach (var rune in value.EnumerateRunes())
        {
            if (rune.Value is >= 0xE000 and <= 0xF8FF
                or >= 0xF0000 and <= 0xFFFFD
                or >= 0x100000 and <= 0x10FFFD)
            {
                return true;
            }
        }

        return false;
    }
}
