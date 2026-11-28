using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosNameModelAdapter : GenericModelObject, ICosName
{
    private readonly string _internalRepresentation;

    private CosNameModelAdapter(string internalRepresentation)
        : base("CosName")
    {
        _internalRepresentation = internalRepresentation;
    }

    public string? GetinternalRepresentation() => _internalRepresentation;

    public static ICosName? FromParserValue(CosValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            CosName name => FromInternalRepresentation(name.Text),
            _ => null
        };
    }

    internal static ICosName FromInternalRepresentation(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new CosNameModelAdapter(value);
    }
}
