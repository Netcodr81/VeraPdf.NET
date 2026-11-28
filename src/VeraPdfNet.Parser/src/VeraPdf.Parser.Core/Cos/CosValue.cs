namespace VeraPdf.Parser.Cos;

public abstract record CosValue;
public sealed record CosBoolean(bool Value) : CosValue;
public sealed record CosInteger(long Value) : CosValue;
public sealed record CosReal(double Value) : CosValue;
public sealed record CosName(byte[] Name) : CosValue
{
    public string Text => System.Text.Encoding.Latin1.GetString(Name);
}
public sealed record CosString(byte[] Value) : CosValue
{
    public bool IsHex { get; }
    public bool ContainsOnlyHex { get; } = true;
    public long HexCount { get; }
    public string Text => PdfStringTextDecoder.GetString(Value);

    public CosString(byte[] value, bool isHex, bool containsOnlyHex, long hexCount)
        : this(value)
    {
        IsHex = isHex;
        ContainsOnlyHex = containsOnlyHex;
        HexCount = hexCount;
    }

}
public sealed record CosNull() : CosValue;
public sealed record CosIndirectReference(long ObjectNumber, long Generation) : CosValue;
public sealed record CosStream(CosDictionary Dictionary, byte[] RawData) : CosValue
{
    public bool? StreamKeywordCRLFCompliant { get; init; }
    public bool? EndstreamKeywordEOLCompliant { get; init; }
}
public readonly record struct CosKey(long ObjectNumber, long Generation);
public sealed record CosIndirectObject(CosKey Key, CosValue Value) : CosValue;

public sealed record CosArray : CosValue
{
    private readonly List<CosValue> _values = [];
    public IReadOnlyList<CosValue> Values => _values;
    public void Add(CosValue value) => _values.Add(value);
}

public sealed record CosDictionary : CosValue
{
    private readonly Dictionary<string, CosValue> _entries = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, CosValue> Entries => _entries;

    public bool TryGetValue(string name, out CosValue value) =>
        _entries.TryGetValue(name, out value!);

    public void Set(string name, CosValue value) => _entries[name] = value;
}
