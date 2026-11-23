using System.Globalization;
using VeraPdf.Parser.Io;

namespace VeraPdf.Parser.Parser;

public enum PdfTokenType
{
    EndOfFile,
    None,
    Integer,
    Real,
    LiteralString,
    HexString,
    Name,
    Keyword,
    OpenArray,
    CloseArray,
    OpenDictionary,
    CloseDictionary
}

public sealed record PdfToken(
    PdfTokenType Type,
    byte[]? Bytes = null,
    long IntegerValue = 0,
    double RealValue = 0,
    bool ContainsOnlyHex = true,
    long HexCount = 0);

public sealed class PdfLexer
{
    private readonly PdfSeekableStream _source;

    public PdfLexer(PdfSeekableStream source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public PdfSeekableStream Source => _source;

    public PdfToken NextToken()
    {
        SkipWhitespaceAndComments();
        if (_source.IsEndOfStream)
        {
            return new PdfToken(PdfTokenType.EndOfFile);
        }

        var first = (byte)_source.ReadByteOrThrow();
        return first switch
        {
            (byte)'(' => ReadLiteralString(),
            (byte)'<' => ReadLessThan(),
            (byte)'>' => ReadCloseDelimiter(),
            (byte)'[' => new PdfToken(PdfTokenType.OpenArray),
            (byte)']' => new PdfToken(PdfTokenType.CloseArray),
            (byte)'/' => ReadName(),
            (byte)'+' or (byte)'-' or (byte)'.' or >= (byte)'0' and <= (byte)'9'
                => ReadNumber(first),
            _ => ReadRegularToken(first)
        };
    }

    private void SkipWhitespaceAndComments()
    {
        while (!_source.IsEndOfStream)
        {
            var value = _source.Peek();
            if (IsWhitespace(value))
            {
                _source.ReadByte();
                continue;
            }
            if (value == '%')
            {
                _source.ReadByte();
                while (!_source.IsEndOfStream)
                {
                    var commentByte = _source.ReadByte();
                    if (commentByte is '\r' or '\n')
                    {
                        break;
                    }
                }
                continue;
            }
            break;
        }
    }

    private PdfToken ReadLessThan()
    {
        if (_source.IsEndOfStream)
        {
            throw new IOException("Incomplete PDF delimiter.");
        }
        var next = _source.ReadByteOrThrow();
        if (next == '<')
        {
            return new PdfToken(PdfTokenType.OpenDictionary);
        }

        _source.Unread();
        return ReadHexString();
    }

    private PdfToken ReadCloseDelimiter()
    {
        if (_source.ReadByteOrThrow() != '>')
        {
            throw new IOException("Unknown symbol after '>'.");
        }
        return new PdfToken(PdfTokenType.CloseDictionary);
    }

    private PdfToken ReadName()
    {
        var bytes = new List<byte>();
        while (!_source.IsEndOfStream)
        {
            var value = _source.Peek();
            if (IsWhitespace(value) || IsDelimiter(value))
            {
                break;
            }
            var current = (byte)_source.ReadByteOrThrow();
            if (current == '#')
            {
                if (_source.StreamLength - _source.Offset < 2)
                {
                    bytes.Add(current);
                    continue;
                }
                var first = _source.Peek();
                var firstHex = DecodeHex(first);
                if (firstHex >= 0)
                {
                    _source.ReadByte();
                    var second = _source.Peek();
                    var secondHex = DecodeHex(second);
                    if (secondHex >= 0)
                    {
                        _source.ReadByte();
                        bytes.Add((byte)((firstHex << 4) | secondHex));
                        continue;
                    }
                    _source.Unread();
                }
                bytes.Add(current);
                continue;
            }
            bytes.Add(current);
        }
        return new PdfToken(PdfTokenType.Name, bytes.ToArray());
    }

    private PdfToken ReadLiteralString()
    {
        var bytes = new List<byte>();
        var depth = 0;
        var append = true;

        while (!_source.IsEndOfStream)
        {
            var value = _source.ReadByteOrThrow();
            switch (value)
            {
                case '(':
                    depth++;
                    if (append) bytes.Add((byte)'(');
                    break;
                case ')':
                    if (depth == 0)
                    {
                        if (bytes.Count > 65535)
                        {
                            throw new PdfResourceLimitExceededException(
                                "A PDF literal string exceeds the 65535 byte token limit.");
                        }
                        return new PdfToken(PdfTokenType.LiteralString, bytes.ToArray());
                    }
                    depth--;
                    if (append) bytes.Add((byte)')');
                    break;
                case '\\':
                    if (_source.IsEndOfStream)
                    {
                        break;
                    }
                    var escaped = _source.ReadByteOrThrow();
                    if (escaped is (byte)'n') { if (append) bytes.Add((byte)'\n'); }
                    else if (escaped is (byte)'r') { if (append) bytes.Add((byte)'\r'); }
                    else if (escaped is (byte)'t') { if (append) bytes.Add((byte)'\t'); }
                    else if (escaped is (byte)'b') { if (append) bytes.Add((byte)'\b'); }
                    else if (escaped is (byte)'f') { if (append) bytes.Add((byte)'\f'); }
                    else if (escaped == '(' || escaped == ')' || escaped == '\\')
                    {
                        if (append) bytes.Add((byte)escaped);
                    }
                    else if (escaped is >= (byte)'0' and <= (byte)'7')
                    {
                        var octal = escaped - (byte)'0';
                        for (var index = 0; index < 2; index++)
                        {
                            if (_source.IsEndOfStream) break;
                            var next = _source.Peek();
                            if (next is >= (byte)'0' and <= (byte)'7')
                            {
                                _source.ReadByte();
                                octal = (octal << 3) + next - (byte)'0';
                            }
                            else break;
                        }
                        if (append) bytes.Add((byte)octal);
                    }
                    else if (escaped is '\r')
                    {
                        if (_source.Peek() == '\n') _source.ReadByte();
                    }
                    else if (escaped is '\n')
                    {
                    }
                    else if (append)
                    {
                        bytes.Add((byte)escaped);
                    }
                    break;
                default:
                    if (append) bytes.Add((byte)value);
                    break;
            }

            if (bytes.Count > 65535)
            {
                append = false;
            }
        }

        return new PdfToken(PdfTokenType.LiteralString, bytes.ToArray());
    }

    private PdfToken ReadHexString()
    {
        var bytes = new List<byte>();
        var containsOnlyHex = true;
        long hexCount = 0;
        int? firstNibble = null;

        while (!_source.IsEndOfStream)
        {
            var value = _source.ReadByteOrThrow();
            if (value == '>')
            {
                if (firstNibble.HasValue)
                {
                    bytes.Add((byte)(firstNibble.Value << 4));
                }
                return new PdfToken(PdfTokenType.HexString, bytes.ToArray(), 0, 0,
                    containsOnlyHex, hexCount);
            }
            if (IsWhitespace(value))
            {
                continue;
            }

            hexCount++;
            var hex = DecodeHex(value);
            if (hex < 0)
            {
                containsOnlyHex = false;
                continue;
            }
            if (firstNibble.HasValue)
            {
                bytes.Add((byte)((firstNibble.Value << 4) | hex));
                firstNibble = null;
            }
            else
            {
                firstNibble = hex;
            }
        }

        return new PdfToken(PdfTokenType.HexString, bytes.ToArray(), 0, 0,
            containsOnlyHex, hexCount);
    }

    private PdfToken ReadNumber(byte first)
    {
        var bytes = new List<byte> { first };
        var isReal = first == '.';

        while (!_source.IsEndOfStream)
        {
            var value = _source.Peek();
            if (IsDelimiter(value) || IsWhitespace(value))
            {
                break;
            }
            if (value is >= (byte)'0' and <= (byte)'9')
            {
                bytes.Add((byte)_source.ReadByteOrThrow());
            }
            else if (value == '.')
            {
                isReal = true;
                bytes.Add((byte)_source.ReadByteOrThrow());
            }
            else
            {
                break;
            }
        }

        var text = System.Text.Encoding.ASCII.GetString(bytes.ToArray());
        if (isReal)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var real))
            {
                return new PdfToken(PdfTokenType.Real, null, checked((long)Math.Round(real)), real);
            }
        }
        else if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
        {
            var sign = integer;
            if (first == '-' && integer == 0)
            {
                sign = 0;
            }
            return new PdfToken(PdfTokenType.Integer, null, sign, sign);
        }

        return new PdfToken(PdfTokenType.Real, null, long.MaxValue, double.MaxValue);
    }

    private PdfToken ReadRegularToken(byte first)
    {
        var bytes = new List<byte> { first };
        while (!_source.IsEndOfStream)
        {
            var value = _source.Peek();
            if (IsDelimiter(value) || IsWhitespace(value))
            {
                break;
            }
            bytes.Add((byte)_source.ReadByteOrThrow());
        }

        var text = System.Text.Encoding.Latin1.GetString(bytes.ToArray());
        return text switch
        {
            "null" or "true" or "false" or "stream" or "endstream" or "obj" or "endobj"
            or "R" or "n" or "f" or "xref" or "startxref" or "trailer"
                => new PdfToken(PdfTokenType.Keyword, bytes.ToArray()),
            _ => new PdfToken(PdfTokenType.None, bytes.ToArray())
        };
    }

    private static bool IsWhitespace(int value) =>
        value is 0 or 9 or 10 or 12 or 13 or 32;

    private static bool IsDelimiter(int value) =>
        value is '(' or ')' or '<' or '>' or '[' or ']' or '{' or '}' or '/' or '%';

    private static int DecodeHex(int value) =>
        value switch
        {
            >= (byte)'0' and <= (byte)'9' => value - '0',
            >= (byte)'A' and <= (byte)'F' => value - 'A' + 10,
            >= (byte)'a' and <= (byte)'f' => value - 'a' + 10,
            _ => -1
        };
}
