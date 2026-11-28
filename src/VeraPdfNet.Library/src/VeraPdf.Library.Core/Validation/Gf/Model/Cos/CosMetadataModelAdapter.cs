using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.PDLayer;
using VeraPdf.Library.Xmp;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosMetadataModelAdapter : GenericModelObject, IPDMetadata
{
    private readonly CosStream _stream;
    private readonly PdfDocumentReader? _reader;
    private readonly string? _objectKey;
    private readonly bool _isCatalogMetadata;

    internal CosMetadataModelAdapter(
        CosStream stream,
        PdfDocumentReader? reader,
        string? objectKey,
        bool isCatalogMetadata)
        : base("PDMetadata")
    {
        _stream = stream;
        _reader = reader;
        _objectKey = objectKey;
        _isCatalogMetadata = isCatalogMetadata;
    }

    public string? GetFilter()
    {
        var filters = GetStreamAdapter().GetLinkedObjects("filters")
            .Cast<VeraPdf.Model.CosLayer.ICosFilter>()
            .Select(filter => filter.GetinternalRepresentation())
            .Where(name => name is not null);
        var result = string.Join(" ", filters);
        return result.Length == 0 ? null : result;
    }

    public bool? GetisCatalogMetadata() => _isCatalogMetadata;
    public string? Getentries() => string.Join("&", _stream.Dictionary.Entries.Keys);
    public string? GetobjectKey() => _objectKey;
    public override string? GetID() => _objectKey is null ? null : $"{_objectKey} PDMetadata";

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
        linkName switch
        {
            "stream" => [GetStreamAdapter()],
            "XMPPackage" =>
            [
                CosXmpPackageModelAdapter.FromStream(
                    _stream,
                    _reader,
                    _isCatalogMetadata,
                    GetMainPackageSchemas())
            ],
            _ => base.GetLinkedObjects(linkName)
        };

    private (bool Known, XmpNode? Schemas) GetMainPackageSchemas()
    {
        if (_isCatalogMetadata)
        {
            return (true, null);
        }

        if (_reader is null
            || !_reader.Trailer.TryGetValue("Root", out var rootReference)
            || _reader.ResolveDictionary(rootReference) is not { } catalog)
        {
            return (false, null);
        }

        if (!catalog.TryGetValue("Metadata", out var rawMetadata)
            || _reader.ResolveReference(rawMetadata) is not CosStream mainMetadata)
        {
            return (true, null);
        }

        var mainPackage = CosXmpPackageModelAdapter.FromStream(mainMetadata, _reader);
        return mainPackage.IsParsed
            ? (true, mainPackage.ExtensionSchemas)
            : (false, null);
    }

    private VeraPdf.Model.CosLayer.ICosStream GetStreamAdapter() =>
        CosStreamModelAdapter.FromParserValue(_stream, _reader);
}