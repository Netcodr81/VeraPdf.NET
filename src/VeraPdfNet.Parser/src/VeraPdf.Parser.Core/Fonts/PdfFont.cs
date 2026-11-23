using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Fonts;

public enum PdfFontKind
{
    Type1,
    TrueType,
    Type0,
    Type3,
    CIDFont,
    Unknown
}

public sealed class PdfFont
{
    private readonly CosDictionary _dictionary;
    private readonly IReadOnlyDictionary<long, string> _unicodeMap;

    public PdfFont(CosDictionary dictionary,
        IReadOnlyDictionary<long, string>? unicodeMap = null,
        double[]? boundingBox = null)
    {
        _dictionary = dictionary;
        _unicodeMap = unicodeMap ?? new Dictionary<long, string>();
        BoundingBox = boundingBox ?? [0, 0, 1000, 1000];
    }

    public CosDictionary Dictionary => _dictionary;
    public string? BaseFont => _dictionary.TryGetValue("BaseFont", out var value) &&
        value is CosName name ? name.Text : null;
    public string? Subtype => _dictionary.TryGetValue("Subtype", out var value) &&
        value is CosName name ? name.Text : null;
    public double[] BoundingBox { get; }

    public PdfFontKind Kind => Subtype switch
    {
        "Type0" => PdfFontKind.Type0,
        "CIDFontType0" or "CIDFontType2" => PdfFontKind.CIDFont,
        "TrueType" => PdfFontKind.TrueType,
        "Type3" => PdfFontKind.Type3,
        "Type1" => PdfFontKind.Type1,
        _ => PdfFontKind.Unknown
    };

    public string? ToUnicode(long characterCode) =>
        _unicodeMap.TryGetValue(characterCode, out var text) ? text : null;

    public double GetWidth(long characterCode)
    {
        if (_dictionary.TryGetValue("Widths", out var widthValue) &&
            widthValue is CosArray widths)
        {
            if (_dictionary.TryGetValue("FirstChar", out var firstValue) &&
                firstValue is CosInteger first &&
                characterCode >= first.Value &&
                characterCode - first.Value < widths.Values.Count)
            {
                var width = widths.Values[(int)(characterCode - first.Value)];
                if (width is CosInteger integer) return integer.Value;
                if (width is CosReal real) return real.Value;
            }
        }
        return StandardFontMetrics.GetWidth(BaseFont, characterCode);
    }
}
