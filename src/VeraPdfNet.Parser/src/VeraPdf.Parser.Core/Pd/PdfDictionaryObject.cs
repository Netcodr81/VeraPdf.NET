using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public abstract class PdfDictionaryObject
{
    public CosDictionary Dictionary { get; }

    protected PdfDictionaryObject(CosDictionary dictionary)
    {
        Dictionary = dictionary;
    }

    public CosValue? Get(string name) =>
        Dictionary.TryGetValue(name, out var value) ? value : null;

    public string? GetName(string name) =>
        Get(name) is CosName cosName ? cosName.Text : null;

    public long? GetInteger(string name) =>
        Get(name) is CosInteger cosInteger ? cosInteger.Value : null;

    public double? GetReal(string name)
    {
        return Get(name) switch
        {
            CosReal cosReal => cosReal.Value,
            CosInteger cosInteger => cosInteger.Value,
            _ => null
        };
    }

    public CosDictionary? GetDictionary(string name, PdfDocument document) =>
        Get(name) is { } value ? document.Reader.ResolveDictionary(value) : null;
}
