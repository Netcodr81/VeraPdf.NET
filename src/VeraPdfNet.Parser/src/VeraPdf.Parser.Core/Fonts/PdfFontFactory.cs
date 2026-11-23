using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Filters;

namespace VeraPdf.Parser.Fonts;

public sealed class PdfFontFactory
{
    public PdfFont Create(CosDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        var descriptor = dictionary.TryGetValue("FontDescriptor", out var descriptorValue)
            ? descriptorValue
            : null;
        double[]? boundingBox = null;
        if (descriptor is CosDictionary descriptorDictionary &&
            descriptorDictionary.TryGetValue("FontBBox", out var boxValue) &&
            boxValue is CosArray box &&
            box.Values.Count >= 4)
        {
            boundingBox = box.Values.Take(4).Select(ReadNumber).ToArray();
        }

        IReadOnlyDictionary<long, string>? unicodeMap = null;
        if (dictionary.TryGetValue("ToUnicode", out var unicodeValue) &&
            unicodeValue is CosStream unicodeStream)
        {
            unicodeMap = ParseToUnicode(unicodeStream);
        }
        return new PdfFont(dictionary, unicodeMap, boundingBox);
    }

    private static IReadOnlyDictionary<long, string> ParseToUnicode(CosStream stream)
    {
        var result = new Dictionary<long, string>();
        var decoded = new PdfStreamFilterService().Decode(stream);
        var cmap = new CMapParser().Parse(decoded);
        foreach (var pair in cmap.UnicodeMappings)
        {
            result[pair.Key] = pair.Value;
        }
        return result;
    }

    private static double ReadNumber(CosValue value) =>
        value switch
        {
            CosInteger integer => integer.Value,
            CosReal real => real.Value,
            _ => 0
        };
}
