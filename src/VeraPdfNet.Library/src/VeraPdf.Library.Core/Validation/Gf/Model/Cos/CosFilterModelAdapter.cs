using VeraPdf.Model;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosFilterModelAdapter : GenericModelObject, ICosFilter
{
    private readonly CosName _name;
    private readonly CosDictionary? _decodeParameters;
    private readonly PdfDocumentReader? _reader;

    private CosFilterModelAdapter(
        CosName name,
        CosDictionary? decodeParameters,
        PdfDocumentReader? reader)
        : base("CosFilter")
    {
        _name = name;
        _decodeParameters = decodeParameters;
        _reader = reader;
    }

    public string? GetinternalRepresentation() => _name.Text;

    public string? GetdecodeParms()
    {
        if (_name.Text != "Crypt")
        {
            return _decodeParameters is null ? "Default" : "Custom";
        }

        if (_decodeParameters is null)
        {
            return "Identity";
        }

        if (!_decodeParameters.TryGetValue("Name", out var name))
        {
            return null;
        }

        return Resolve(name) is CosName cryptName ? cryptName.Text : null;
    }

    public static ICosFilter FromParserValue(
        CosName name,
        CosDictionary? decodeParameters = null,
        PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        return new CosFilterModelAdapter(name, decodeParameters, reader);
    }

    private CosValue? Resolve(CosValue value)
    {
        if (value is CosIndirectReference && _reader is null)
        {
            throw new InvalidOperationException(
                "A PdfDocumentReader is required to resolve the indirect COS filter parameter.");
        }

        return _reader is null ? value : _reader.ResolveReference(value);
    }
}
