using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosDictionaryModelAdapter : GenericModelObject, ICosDict
{
    private readonly CosDictionary _dictionary;
    private readonly long _size;
    private readonly PdfDocumentReader? _reader;

    private CosDictionaryModelAdapter(CosDictionary dictionary, PdfDocumentReader? reader)
        : base("CosDict")
    {
        _dictionary = dictionary;
        _size = dictionary.Entries.Count;
        _reader = reader;
    }

    public long? Getsize() => _size;

    public string? GetkeysString() => string.Join("&", _dictionary.Entries.Keys);

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
        linkName switch
        {
            "keys" => GetKeys(),
            "values" => GetValues(),
            "metadata" => GetMetadata(),
            _ => base.GetLinkedObjects(linkName)
        };

    public static ICosDict FromParserValue(CosDictionary dictionary, PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return new CosDictionaryModelAdapter(dictionary, reader);
    }

    private IReadOnlyList<IModelObject> GetKeys()
    {
        var keys = new List<IModelObject>(_dictionary.Entries.Count);
        foreach (var key in _dictionary.Entries.Keys)
        {
            keys.Add(CosNameModelAdapter.FromInternalRepresentation(key));
        }

        return keys.AsReadOnly();
    }

    private IReadOnlyList<IModelObject> GetValues()
    {
        var values = new List<IModelObject>(_dictionary.Entries.Count);
        foreach (var value in _dictionary.Entries.Values)
        {
            if (value is not null)
            {
                values.Add(CosObjectModelAdapter.FromParserValue(value, _reader));
            }
        }

        return values.AsReadOnly();
    }

    private IReadOnlyList<IModelObject> GetMetadata()
    {
        var isCatalog = _dictionary.TryGetValue("Type", out var rawType)
                        && Resolve(rawType) is CosName { Text: "Catalog" };

        if (!_dictionary.TryGetValue("Metadata", out var rawMetadata)
            || Resolve(rawMetadata) is not CosStream stream
            || !stream.Dictionary.TryGetValue("Type", out var rawMetadataType)
            || Resolve(rawMetadataType) is not CosName {Text: "Metadata"}
            || !stream.Dictionary.TryGetValue("Subtype", out var rawSubtype)
            || Resolve(rawSubtype) is not CosName {Text: "XML"})
        {
            return Array.Empty<IModelObject>();
        }

        var objectKey = rawMetadata is CosIndirectReference reference
            ? $"{reference.ObjectNumber} {reference.Generation} obj"
            : null;
        return [new CosMetadataModelAdapter(stream, _reader, objectKey, isCatalog)];
    }

    private CosValue? Resolve(CosValue value)
    {
        if (value is CosIndirectReference && _reader is null)
        {
            throw new InvalidOperationException(
                "A PdfDocumentReader is required to resolve an indirect COS metadata value.");
        }

        return _reader is null ? value : _reader.ResolveReference(value);
    }
}