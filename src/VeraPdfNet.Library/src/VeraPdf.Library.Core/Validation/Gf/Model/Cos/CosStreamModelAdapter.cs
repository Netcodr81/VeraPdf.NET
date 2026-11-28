using System.Globalization;
using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosStreamModelAdapter : GenericModelObject, ICosStream
{
    private readonly CosStream _stream;
    private readonly CosDictionary _dictionary;
    private readonly PdfDocumentReader? _reader;
    private readonly long? _length;

    private CosStreamModelAdapter(CosStream stream, PdfDocumentReader? reader)
        : base("CosStream")
    {
        _stream = stream;
        _dictionary = stream.Dictionary;
        _reader = reader;
        _length = ParseLength(stream.Dictionary);
    }

    public long? Getsize() => _dictionary.Entries.Count;
    public string? GetkeysString() => string.Join("&", _dictionary.Entries.Keys);
    public long? GetLength() => _length;
    public long? GetrealLength() => _stream.RawData.LongLength;
    public string? GetF() => GetExternalFieldString("F");
    public string? GetFFilter() => GetExternalFilterString();
    public string? GetFDecodeParms() => GetExternalFieldString("FDecodeParms");
    public bool? GetstreamKeywordCRLFCompliant() => _stream.StreamKeywordCRLFCompliant;
    public bool? GetendstreamKeywordEOLCompliant() => _stream.EndstreamKeywordEOLCompliant;

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName)
    {
        if (linkName == "filters")
        {
            return GetFilters();
        }

        return linkName switch
        {
            "keys" or "values" or "metadata" => GetDictionaryAdapter().GetLinkedObjects(linkName),
            _ => base.GetLinkedObjects(linkName)
        };
    }

    public static ICosStream FromParserValue(CosStream stream, PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(stream.Dictionary);
        ArgumentNullException.ThrowIfNull(stream.RawData);
        return new CosStreamModelAdapter(stream, reader);
    }

    private static long? ParseLength(CosDictionary dictionary)
    {
        if (!dictionary.TryGetValue("Length", out var value)
            || value is not (CosInteger or CosReal))
        {
            return null;
        }

        return CosNumberModelAdapter.FromParserValue(value)?.GetintValue();
    }

    private string? GetExternalFieldString(string key)
    {
        if (!_dictionary.TryGetValue(key, out var value))
        {
            return null;
        }

        var resolved = Resolve(value);
        return resolved is null ? null : ToJavaString(resolved);
    }

    private string? GetExternalFilterString()
    {
        if (!_dictionary.TryGetValue("FFilter", out var value))
        {
            return null;
        }

        return Resolve(value) switch
        {
            CosName name => name.Text,
            CosArray array => string.Join(" ", array.Values.Select(item =>
                Resolve(item) is CosName filterName
                    ? filterName.Text
                    : throw new InvalidDataException(
                        "Every COS stream /FFilter array item must be a name."))),
            _ => throw new InvalidDataException("The COS stream /FFilter must be a name or an array of names.")
        };
    }

    private string ToJavaString(CosValue value)
    {
        return value switch
        {
            CosBoolean boolean => boolean.Value ? "true" : "false",
            CosInteger integer => integer.Value.ToString(CultureInfo.InvariantCulture),
            CosReal real => real.Value.ToString("R", CultureInfo.InvariantCulture),
            CosName name => name.Text,
            CosString text => text.IsHex
                ? $"<{Convert.ToHexString(text.Value)}>"
                : text.Text,
            CosNull => "null",
            CosArray array => "[" + string.Join(", ", array.Values.Select(item =>
                item is null ? "null" : ToJavaString(Resolve(item) ?? new CosNull()))) + "]",
            CosDictionary dictionary => $"dictionary(size = {dictionary.Entries.Count})",
            CosStream stream => $"stream(size = {stream.RawData.LongLength})",
            CosIndirectObject indirect => ToJavaString(indirect.Value),
            _ => throw new NotSupportedException(
                $"COS external-file field formatting is not implemented for '{value.GetType().Name}'.")
        };
    }

    private IReadOnlyList<IModelObject> GetFilters()
    {
        if (!_dictionary.TryGetValue("Filter", out var rawFilter))
        {
            return Array.Empty<IModelObject>();
        }

        var filterValue = Resolve(rawFilter);
        var names = filterValue switch
        {
            CosName name => new List<CosName> { name },
            CosArray array => ReadFilterNames(array),
            _ => throw new InvalidDataException("The COS stream /Filter must be a name or an array of names.")
        };
        if (names.Count == 0)
        {
            return Array.Empty<IModelObject>();
        }

        var parameters = _dictionary.TryGetValue("DecodeParms", out var rawParameters)
            ? Resolve(rawParameters)
            : null;
        var filters = new List<IModelObject>(names.Count);

        if (names.Count == 1)
        {
            var parameter = parameters is CosArray parameterArray && parameterArray.Values.Count > 0
                ? Resolve(parameterArray.Values[0])
                : parameters is CosArray ? null : parameters;
            filters.Add(CosFilterModelAdapter.FromParserValue(
                names[0],
                parameter as CosDictionary,
                _reader));
            return filters.AsReadOnly();
        }

        if (parameters is null || parameters is CosNull)
        {
            foreach (var name in names)
            {
                filters.Add(CosFilterModelAdapter.FromParserValue(name, reader: _reader));
            }

            return filters.AsReadOnly();
        }

        if (parameters is not CosArray parametersArray)
        {
            throw new InvalidDataException(
                "The COS stream /DecodeParms for multiple filters must be an array.");
        }

        for (var index = 0; index < names.Count && index < parametersArray.Values.Count; index++)
        {
            var parameter = Resolve(parametersArray.Values[index]);
            filters.Add(CosFilterModelAdapter.FromParserValue(
                names[index],
                parameter as CosDictionary,
                _reader));
        }

        return filters.AsReadOnly();
    }

    private List<CosName> ReadFilterNames(CosArray array)
    {
        var names = new List<CosName>(array.Values.Count);
        foreach (var value in array.Values)
        {
            var resolved = Resolve(value);
            if (resolved is not CosName name)
            {
                throw new InvalidDataException("Every COS stream /Filter array item must be a name.");
            }

            names.Add(name);
        }

        return names;
    }

    private CosValue? Resolve(CosValue value)
    {
        if (value is CosIndirectReference && _reader is null)
        {
            throw new InvalidOperationException(
                "A PdfDocumentReader is required to resolve indirect COS stream filter values.");
        }

        return _reader is null ? value : _reader.ResolveReference(value);
    }

    private ICosDict GetDictionaryAdapter() =>
        CosDictionaryModelAdapter.FromParserValue(_dictionary, _reader);
}
