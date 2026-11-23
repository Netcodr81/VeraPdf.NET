using System.Globalization;
using System.IO.Compression;
using System.Text;
using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Io;
using VeraPdf.Parser.Parser;
using VeraPdf.Parser.Xref;
using VeraPdf.Parser.Filters;

namespace VeraPdf.Parser;

public sealed class PdfDocumentReader : IDisposable
{
    private readonly PdfSeekableStream _source;
    private readonly bool _ownsSource;
    private readonly PdfXrefTable _table = new();
    private readonly Dictionary<CosKey, CosValue> _objectCache = [];
    private readonly Dictionary<long, CosDictionary> _decodedObjectStreams = [];
    private bool _disposed;

    private PdfDocumentReader(PdfSeekableStream source, bool ownsSource)
    {
        _source = source;
        _ownsSource = ownsSource;
    }

    public CosDictionary Trailer { get; private set; } = new();
    public CosDictionary FirstTrailer { get; private set; } = new();
    public CosDictionary LastTrailer { get; private set; } = new();
    public long StartXref { get; private set; }
    public float PdfVersion { get; private set; } = 1.4f;
    public IReadOnlyCollection<CosKey> Keys => _table.Keys;

    public CosValue? ResolveReference(CosValue value)
    {
        if (value is CosIndirectReference reference)
        {
            var resolved = Resolve(new CosKey(reference.ObjectNumber, reference.Generation));
            return resolved is CosIndirectObject indirect ? indirect.Value : resolved;
        }
        return value;
    }

    public CosDictionary? ResolveDictionary(CosValue value)
    {
        var resolved = ResolveReference(value);
        if (resolved is CosIndirectObject indirect)
        {
            resolved = indirect.Value;
        }
        return resolved as CosDictionary;
    }

    public static PdfDocumentReader Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var file = File.OpenRead(path);
        try
        {
            var seekable = PdfSeekableStream.Create(file);
            file.Dispose();
            var reader = new PdfDocumentReader(seekable, ownsSource: true);
            reader.Initialize();
            return reader;
        }
        catch
        {
            file.Dispose();
            throw;
        }
    }

    public static PdfDocumentReader Open(Stream stream, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var seekable = PdfSeekableStream.Create(stream);
        if (!leaveOpen)
        {
            stream.Dispose();
        }
        var reader = new PdfDocumentReader(seekable, ownsSource: true);
        reader.Initialize();
        return reader;
    }

    private void Initialize()
    {
        ReadHeader();
        var latestOffset = FindLastStartXref();
        StartXref = latestOffset;
        ReadRevisions(latestOffset);
    }

    private void ReadHeader()
    {
        _source.Seek(0);
        var length = (int)Math.Min(_source.StreamLength, 1024);
        var header = new byte[length];
        _source.ReadExactly(header);
        var marker = Encoding.ASCII.GetString(header);
        var index = marker.IndexOf("%PDF-", StringComparison.Ordinal);
        if (index < 0)
        {
            throw new IOException("The PDF header is missing.");
        }
        var versionText = marker[(index + 5)..].TrimStart();
        var end = 0;
        while (end < versionText.Length && (char.IsDigit(versionText[end]) || versionText[end] == '.'))
        {
            end++;
        }
        if (end > 0 && float.TryParse(versionText[..end], NumberStyles.Float,
                CultureInfo.InvariantCulture, out var version))
        {
            PdfVersion = version;
        }
        _source.Seek(0);
    }

    private long FindLastStartXref()
    {
        var searchLength = (int)Math.Min(_source.StreamLength, 4096);
        _source.Seek(_source.StreamLength - searchLength);
        var tail = new byte[searchLength];
        _source.ReadExactly(tail);
        var text = Encoding.Latin1.GetString(tail);
        var markerIndex = text.LastIndexOf("startxref", StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            throw new IOException("The final startxref marker is missing.");
        }
        var remainder = text[(markerIndex + 9)..];
        var digits = new System.Text.StringBuilder();
        foreach (var character in remainder)
        {
            if (char.IsDigit(character))
            {
                digits.Append(character);
            }
            else if (digits.Length > 0)
            {
                break;
            }
        }
        if (!long.TryParse(digits.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var offset))
        {
            throw new IOException("The startxref offset is invalid.");
        }
        return offset;
    }

    private void ReadRevisions(long latestOffset)
    {
        var visited = new HashSet<long>();
        var offsets = new Stack<long>();
        offsets.Push(latestOffset);
        CosDictionary? latestTrailer = null;
        CosDictionary? earliestTrailer = null;

        while (offsets.Count > 0)
        {
            var offset = offsets.Pop();
            if (!visited.Add(offset))
            {
                throw new IOException("Cross-reference loop detected.");
            }

            var revision = ParseRevision(offset);
            earliestTrailer = revision.Trailer;
            latestTrailer ??= revision.Trailer;

            foreach (var entry in revision.Table.Entries)
            {
                if (!_table.Entries.ContainsKey(entry.Key))
                {
                    _table.Add(entry.Key, entry.Value);
                }
            }

            if (revision.Trailer.TryGetValue("Prev", out var prevValue) &&
                prevValue is CosInteger prev)
            {
                offsets.Push(prev.Value);
            }
        }

        Trailer = latestTrailer ?? new CosDictionary();
        FirstTrailer = earliestTrailer ?? new CosDictionary();
        LastTrailer = latestTrailer ?? new CosDictionary();
    }

    private PdfRevision ParseRevision(long offset)
    {
        if (offset < 0 || offset > _source.StreamLength)
        {
            throw new IOException("Cross-reference offset is outside the PDF.");
        }
        _source.Seek(offset);
        var lexer = new PdfLexer(_source);
        var first = lexer.NextToken();

        if (first.Type == PdfTokenType.Keyword)
        {
            var table = ParseTraditionalXref(lexer);
            var trailer = ParseTrailer(lexer);
            return new PdfRevision(offset, trailer, table);
        }

        if (first.Type == PdfTokenType.Integer)
        {
            var xrefObjectNumber = first.IntegerValue;
            var generationToken = lexer.NextToken();
            var objToken = lexer.NextToken();
            if (generationToken.Type != PdfTokenType.Integer ||
                objToken.Type != PdfTokenType.Keyword)
            {
                throw new IOException("Invalid xref stream object header.");
            }

            var cosParser = new PdfCosParser(lexer);
            var dictionary = AssertDictionary(cosParser.NextValue());
            var streamKeyword = lexer.NextToken();
            if (System.Text.Encoding.Latin1.GetString(streamKeyword.Bytes ?? []) != "stream")
            {
                throw new IOException("An xref object does not contain a stream.");
            }
            var stream = cosParser.ReadStream(dictionary);
            var table = ParseXrefStream(stream);
            return new PdfRevision(offset, stream.Dictionary, table);
        }

        throw new IOException("No cross-reference table or stream was found at startxref.");
    }

    private PdfXrefTable ParseTraditionalXref(PdfLexer lexer)
    {
        var table = new PdfXrefTable();
        while (true)
        {
            var first = lexer.NextToken();
            if (first.Type != PdfTokenType.Integer)
            {
                break;
            }
            var count = lexer.NextToken();
            if (count.Type != PdfTokenType.Integer)
            {
                throw new IOException("Invalid xref subsection header.");
            }

            for (long index = 0; index < count.IntegerValue; index++)
            {
                var offset = lexer.NextToken();
                var generation = lexer.NextToken();
                var status = lexer.NextToken();
                if (offset.Type != PdfTokenType.Integer ||
                    generation.Type != PdfTokenType.Integer ||
                    status.Type != PdfTokenType.Keyword)
                {
                    throw new IOException("Invalid traditional xref entry.");
                }

                var key = new CosKey(first.IntegerValue + index, generation.IntegerValue);
                var statusText = Encoding.Latin1.GetString(status.Bytes ?? []);
                XrefEntry entry = statusText == "n"
                    ? new XrefEntry.Direct(offset.IntegerValue, generation.IntegerValue)
                    : new XrefEntry.Free(generation.IntegerValue);
                table.Add(key, entry);
            }
        }
        return table;
    }

    private CosDictionary ParseTrailer(PdfLexer lexer)
    {
        var cosParser = new PdfCosParser(lexer);
        return AssertDictionary(cosParser.NextValue());
    }

    private PdfXrefTable ParseXrefStream(CosStream stream)
    {
        var table = new PdfXrefTable();
        var dictionary = stream.Dictionary;
        if (!dictionary.TryGetValue("W", out var wValue) || wValue is not CosArray w ||
            w.Values.Count != 3 ||
            w.Values.Any(value => value is not CosInteger))
        {
            throw new IOException("xref stream /W must contain three integers.");
        }

        var fieldWidths = w.Values.Cast<CosInteger>().Select(value => (int)value.Value).ToArray();
        if (fieldWidths.Any(value => value < 0))
        {
            throw new IOException("xref stream field width is invalid.");
        }
        var entrySize = fieldWidths.Sum();
        if (entrySize <= 0)
        {
            throw new IOException("xref stream entry size is zero.");
        }

        var indexPairs = new List<(long First, long Count)>();
        if (dictionary.TryGetValue("Index", out var indexValue) && indexValue is CosArray index)
        {
            if (index.Values.Count % 2 != 0)
            {
                throw new IOException("xref stream /Index is malformed.");
            }
            for (var i = 0; i < index.Values.Count; i += 2)
            {
                if (index.Values[i] is not CosInteger first ||
                    index.Values[i + 1] is not CosInteger count)
                {
                    throw new IOException("xref stream /Index must contain integer pairs.");
                }
                indexPairs.Add((first.Value, count.Value));
            }
        }
        else
        {
            if (!dictionary.TryGetValue("Size", out var sizeValue) || sizeValue is not CosInteger size)
            {
                throw new IOException("xref stream /Size is missing.");
            }
            indexPairs.Add((0, size.Value));
        }

        var raw = DecodeStream(stream);
        var rawPosition = 0;
        foreach (var (first, count) in indexPairs)
        {
            for (long objectIndex = 0; objectIndex < count; objectIndex++)
            {
                if (rawPosition + entrySize > raw.Length)
                {
                    throw new IOException("xref stream data is truncated.");
                }
                var type = ReadUnsigned(raw, rawPosition, fieldWidths[0], 1);
                rawPosition += fieldWidths[0];
                var field2 = ReadUnsigned(raw, rawPosition, fieldWidths[1], 0);
                rawPosition += fieldWidths[1];
                var field3 = ReadUnsigned(raw, rawPosition, fieldWidths[2], 0);
                rawPosition += fieldWidths[2];

                var objectNumber = first + objectIndex;
                XrefEntry entry = type switch
                {
                    0 => new XrefEntry.Free(field3),
                    1 => new XrefEntry.Direct(field2, field3),
                    2 => new XrefEntry.Compressed(field2, field3),
                    _ => throw new IOException($"Unknown xref stream entry type {type}.")
                };
                var generation = type == 2 ? 0 : type == 1 ? field3 : field3;
                table.Add(new CosKey(objectNumber, generation), entry);
            }
        }
        return table;
    }

    private static long ReadUnsigned(byte[] bytes, int offset, int width, long defaultValue)
    {
        if (width == 0)
        {
            return defaultValue;
        }
        if (width > 8)
        {
            throw new IOException("xref stream field width exceeds 8 bytes.");
        }
        long value = 0;
        for (var i = 0; i < width; i++)
        {
            value = (value << 8) | bytes[offset + i];
        }
        return value;
    }

    private static byte[] DecodeStream(CosStream stream)
    {
        return new PdfStreamFilterService().Decode(stream);
    }

    public CosValue? Resolve(CosKey key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_objectCache.TryGetValue(key, out var cached))
        {
            return cached;
        }
        if (!_table.TryGet(key, out var entry))
        {
            return null;
        }

        CosValue result;
        switch (entry)
        {
            case XrefEntry.Free:
                return null;
            case XrefEntry.Direct direct:
                result = ResolveDirect(direct.Offset, key);
                break;
            case XrefEntry.Compressed compressed:
                result = ResolveCompressed(key, compressed);
                break;
            default:
                throw new IOException("Unknown cross-reference entry type.");
        }
        _objectCache[key] = result;
        return result;
    }

    private CosValue ResolveDirect(long offset, CosKey expectedKey)
    {
        _source.Seek(offset);
        var lexer = new PdfLexer(_source);
        var objectNumber = lexer.NextToken();
        var generation = lexer.NextToken();
        var obj = lexer.NextToken();
        if (objectNumber.Type != PdfTokenType.Integer ||
            generation.Type != PdfTokenType.Integer ||
            obj.Type != PdfTokenType.Keyword)
        {
            throw new IOException("Indirect object header is invalid.");
        }

        var actualKey = new CosKey(objectNumber.IntegerValue, generation.IntegerValue);
        if (actualKey != expectedKey)
        {
            throw new IOException($"Indirect object key mismatch at offset {offset}.");
        }

        var parser = new PdfCosParser(lexer);
        var value = parser.NextValue();
        if (value is CosDictionary dictionary)
        {
            var streamKeyword = lexer.NextToken();
            if (streamKeyword.Type == PdfTokenType.Keyword &&
                Encoding.Latin1.GetString(streamKeyword.Bytes ?? []) == "stream")
            {
                value = parser.ReadStream(dictionary);
            }
        }
        return new CosIndirectObject(actualKey, value);
    }

    private CosValue ResolveCompressed(CosKey key, XrefEntry.Compressed compressed)
    {
        var streamKey = new CosKey(compressed.ObjectStreamNumber, 0);
        var streamObject = Resolve(streamKey) as CosIndirectObject;
        if (streamObject?.Value is not CosStream stream)
        {
            throw new IOException("Compressed PDF object points to a missing object stream.");
        }

        var decoded = DecodeStream(stream);
        if (!stream.Dictionary.TryGetValue("N", out var nValue) || nValue is not CosInteger count ||
            !stream.Dictionary.TryGetValue("First", out var firstValue) || firstValue is not CosInteger first)
        {
            throw new IOException("Object stream /N or /First is missing.");
        }

        var headerEnd = (int)first.Value;
        var header = Encoding.Latin1.GetString(decoded, 0, headerEnd);
        var headerTokens = header.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (headerTokens.Length != count.Value * 2)
        {
            throw new IOException("Object stream header is malformed.");
        }

        for (long index = 0; index < count.Value; index++)
        {
            var objectNumber = long.Parse(headerTokens[index * 2], CultureInfo.InvariantCulture);
            var offset = long.Parse(headerTokens[index * 2 + 1], CultureInfo.InvariantCulture);
            if (objectNumber != key.ObjectNumber)
            {
                continue;
            }

            var bodyOffset = checked((int)(first.Value + offset));
            using var bodyStream = PdfSeekableStream.Create(new MemoryStream(decoded, bodyOffset,
                decoded.Length - bodyOffset));
            var parser = new PdfCosParser(bodyStream);
            var value = parser.NextValue();
            return new CosIndirectObject(key, value);
        }

        throw new IOException($"Object {key.ObjectNumber} was not found in object stream.");
    }

    private static CosDictionary AssertDictionary(CosValue value) =>
        value as CosDictionary ?? throw new IOException("Expected a PDF dictionary.");

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        if (_ownsSource)
        {
            _source.Dispose();
        }
    }
}
