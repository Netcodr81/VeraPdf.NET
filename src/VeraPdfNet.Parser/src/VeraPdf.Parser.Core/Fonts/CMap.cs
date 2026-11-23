namespace VeraPdf.Parser.Fonts;

public sealed record CMapCodeRange(byte[] Start, byte[] End);

public sealed record CMapSingleMapping(byte[] Code, long Target);

public sealed record CMapRangeMapping(byte[] Start, byte[] End, long FirstTarget);

public sealed class PdfCMap
{
    private readonly List<CMapCodeRange> _codeRanges = [];
    private readonly List<CMapSingleMapping> _cidMappings = [];
    private readonly List<CMapRangeMapping> _cidRanges = [];
    private readonly Dictionary<long, string> _unicodeMappings = [];

    public IReadOnlyList<CMapCodeRange> CodeRanges => _codeRanges;
    public IReadOnlyDictionary<long, string> UnicodeMappings => _unicodeMappings;
    public int WritingMode { get; private set; }

    public void SetWritingMode(int mode) => WritingMode = mode;

    public void AddCodeRange(byte[] start, byte[] end) =>
        _codeRanges.Add(new CMapCodeRange(start, end));

    public void AddCidMapping(byte[] code, long target) =>
        _cidMappings.Add(new CMapSingleMapping(code, target));

    public void AddCidRange(byte[] start, byte[] end, long firstTarget) =>
        _cidRanges.Add(new CMapRangeMapping(start, end, firstTarget));

    public void AddUnicodeMapping(byte[] code, string unicode) =>
        _unicodeMappings[BytesToLong(code)] = unicode;

    public void AddUnicodeRange(byte[] start, byte[] end, long firstTarget)
    {
        var firstCode = BytesToLong(start);
        var finalCode = BytesToLong(end);
        for (var code = firstCode; code <= finalCode; code++)
        {
            var target = firstTarget + code - firstCode;
            byte[] bytes = target <= 0xffff
                ? new[] { (byte)(target >> 8), (byte)target }
                : new[] { (byte)(target >> 24), (byte)(target >> 16),
                    (byte)(target >> 8), (byte)target };
            _unicodeMappings[code] = DecodeUnicodeBytes(bytes);
        }
    }

    private static string DecodeUnicodeBytes(byte[] bytes)
    {
        if (bytes.Length == 2)
        {
            return ((char)((bytes[0] << 8) | bytes[1])).ToString();
        }
        return System.Text.Encoding.UTF32.GetString(bytes);
    }

    public long ToCid(long characterCode)
    {
        foreach (var mapping in _cidMappings)
        {
            if (BytesToLong(mapping.Code) == characterCode)
            {
                return mapping.Target;
            }
        }
        foreach (var range in _cidRanges)
        {
            var start = BytesToLong(range.Start);
            var end = BytesToLong(range.End);
            if (characterCode >= start && characterCode <= end)
            {
                return range.FirstTarget + characterCode - start;
            }
        }
        return 0;
    }

    public string? ToUnicode(long characterCode) =>
        _unicodeMappings.TryGetValue(characterCode, out var text) ? text : null;

    public static long BytesToLong(byte[] bytes)
    {
        long value = 0;
        foreach (var valueByte in bytes)
        {
            value = (value << 8) | valueByte;
        }
        return value;
    }
}
