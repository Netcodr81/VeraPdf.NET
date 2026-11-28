using System.Globalization;
using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosIndirectModelAdapter : GenericModelObject, ICosIndirect
{
    private readonly string _id;
    private readonly CosIndirectObject _value;
    private readonly PdfDocumentReader? _reader;
    private ICosObject? _directObject;

    private CosIndirectModelAdapter(CosIndirectObject value, PdfDocumentReader? reader)
        : base("CosIndirect")
    {
        _id = string.Create(
            CultureInfo.InvariantCulture,
            $"{value.Key.ObjectNumber} {value.Key.Generation}");
        _value = value;
        _reader = reader;
    }

    public override string GetID() => _id;

    public bool? GetspacingCompliesPDFA() => null;

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
        linkName == "directObject"
            ? [GetDirectObject()]
            : base.GetLinkedObjects(linkName);

    public static ICosIndirect FromParserValue(
        CosIndirectObject value,
        PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (reader is null && value.Value is CosIndirectReference)
        {
            throw new InvalidOperationException(
                "A PdfDocumentReader is required to resolve the indirect object's direct value.");
        }

        return new CosIndirectModelAdapter(value, reader);
    }

    private ICosObject GetDirectObject()
    {
        if (_directObject is not null)
        {
            return _directObject;
        }

        var directValue = _reader is null
            ? _value.Value
            : _reader.ResolveReference(_value.Value);

        if (directValue is null)
        {
            return _directObject = CosNullModelAdapter.Instance;
        }

        return _directObject = CosObjectModelAdapter.FromParserValue(directValue, _reader);
    }
}
