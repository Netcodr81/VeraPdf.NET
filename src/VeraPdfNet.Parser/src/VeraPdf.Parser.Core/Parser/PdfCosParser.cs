using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Io;

namespace VeraPdf.Parser.Parser;

public sealed class PdfCosParser
{
    private readonly PdfLexer _lexer;
    private readonly Queue<CosValue> _queuedValues = new();
    private readonly Queue<long> _integers = new();

    public PdfCosParser(PdfSeekableStream source)
        : this(new PdfLexer(source))
    {
    }

    public PdfCosParser(PdfLexer lexer)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
    }

    public PdfLexer Lexer => _lexer;

    public CosValue NextValue()
    {
        if (_queuedValues.Count > 0)
        {
            return _queuedValues.Dequeue();
        }

        var token = _lexer.NextToken();
        if (token.Type == PdfTokenType.Integer)
        {
            _integers.Enqueue(token.IntegerValue);
            return NextValue();
        }

        if (token.Type == PdfTokenType.Keyword &&
            System.Text.Encoding.Latin1.GetString(token.Bytes ?? []) == "R" &&
            _integers.Count == 2)
        {
            var objectNumber = _integers.Dequeue();
            var generation = _integers.Dequeue();
            return new CosIndirectReference(objectNumber, generation);
        }

        if (_integers.Count > 0)
        {
            var first = new CosInteger(_integers.Dequeue());
            while (_integers.Count > 0)
            {
                _queuedValues.Enqueue(new CosInteger(_integers.Dequeue()));
            }

            var current = ParseToken(token);
            _queuedValues.Enqueue(current);
            return first;
        }

        return ParseToken(token);
    }

    private CosValue ParseToken(PdfToken token)
    {
        return token.Type switch
        {
            PdfTokenType.Integer => new CosInteger(token.IntegerValue),
            PdfTokenType.Real => new CosReal(token.RealValue),
            PdfTokenType.LiteralString => new CosString(
                token.Bytes ?? [], false, token.ContainsOnlyHex, token.HexCount),
            PdfTokenType.HexString => new CosString(
                token.Bytes ?? [], true, token.ContainsOnlyHex, token.HexCount),
            PdfTokenType.Name => new CosName(token.Bytes ?? []),
            PdfTokenType.Keyword => ReadKeyword(token),
            PdfTokenType.OpenArray => ReadArray(),
            PdfTokenType.OpenDictionary => ReadDictionary(),
            PdfTokenType.CloseArray or PdfTokenType.CloseDictionary or PdfTokenType.EndOfFile
                => new CosEndMarker(),
            _ => new CosNull()
        };
    }

    private CosValue ReadKeyword(PdfToken token)
    {
        var text = System.Text.Encoding.Latin1.GetString(token.Bytes ?? []);
        return text switch
        {
            "null" => new CosNull(),
            "true" => new CosBoolean(true),
            "false" => new CosBoolean(false),
            _ => new CosNull()
        };
    }

    private CosArray ReadArray()
    {
        var array = new CosArray();
        while (true)
        {
            var value = NextValue();
            if (value is CosEndMarker)
            {
                break;
            }
            array.Add(value);
        }
        return array;
    }

    private CosDictionary ReadDictionary()
    {
        var dictionary = new CosDictionary();
        while (true)
        {
            var key = NextValue();
            if (key is CosEndMarker)
            {
                break;
            }
            if (key is not CosName name)
            {
                throw new IOException("A PDF dictionary key must be a name.");
            }
            var value = NextValue();
            if (value is CosEndMarker)
            {
                break;
            }
            dictionary.Set(name.Text, value);
        }
        return dictionary;
    }

    public CosStream ReadStream(CosDictionary dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        if (!dictionary.TryGetValue("Length", out var lengthValue) ||
            lengthValue is not CosInteger length)
        {
            throw new IOException("A PDF stream with no direct /Length is not supported in Phase 2.");
        }

        var streamKeywordCompliant = SkipOneEndOfLine();
        var data = new byte[length.Value];
        var read = 0;
        while (read < data.Length)
        {
            var count = _lexer.Source.Read(data, read, data.Length - read);
            if (count == 0)
            {
                throw new EndOfStreamException("The PDF stream ended before /Length bytes were read.");
            }
            read += count;
        }
        var endstreamKeywordCompliant = HasEndstreamEol();
        return new CosStream(dictionary, data)
        {
            StreamKeywordCRLFCompliant = streamKeywordCompliant,
            EndstreamKeywordEOLCompliant = endstreamKeywordCompliant
        };
    }

    private bool SkipOneEndOfLine()
    {
        if (_lexer.Source.IsEndOfStream)
        {
            return false;
        }

        var first = _lexer.Source.ReadByteOrThrow();
        if (first == '\r')
        {
            if (!_lexer.Source.IsEndOfStream && _lexer.Source.Peek() == '\n')
            {
                _lexer.Source.ReadByte();
                return true;
            }

            return false;
        }

        if (first == '\n')
        {
            return true;
        }

        _lexer.Source.Unread();
        return false;
    }

    private bool HasEndstreamEol()
    {
        var source = _lexer.Source;
        var dataEnd = source.Offset;
        var lastWhitespace = -1;
        while (!source.IsEndOfStream && IsWhitespace(source.Peek()))
        {
            lastWhitespace = source.ReadByte();
        }

        source.Seek(dataEnd);
        return lastWhitespace is '\r' or '\n';
    }

    private static bool IsWhitespace(int value) => value is 0 or 9 or 10 or 12 or 13 or 32;
}
