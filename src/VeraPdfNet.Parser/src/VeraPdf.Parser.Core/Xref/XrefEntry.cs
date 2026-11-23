using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Xref;

public abstract record XrefEntry
{
    public sealed record Free(long Generation) : XrefEntry;
    public sealed record Direct(long Offset, long Generation) : XrefEntry;
    public sealed record Compressed(long ObjectStreamNumber, long Index) : XrefEntry;
}

public sealed class PdfXrefTable
{
    private readonly Dictionary<CosKey, XrefEntry> _entries = [];

    public IReadOnlyDictionary<CosKey, XrefEntry> Entries => _entries;
    public IReadOnlyCollection<CosKey> Keys => _entries.Keys;

    public void Add(CosKey key, XrefEntry entry) => _entries[key] = entry;
    public bool TryGet(CosKey key, out XrefEntry? entry) => _entries.TryGetValue(key, out entry);
    public bool Contains(CosKey key) => _entries.ContainsKey(key);
}
