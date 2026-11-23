using System.IO.Compression;
using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Io;

namespace VeraPdf.Parser.Filters;

public sealed class PdfStreamDecodeOptions
{
    public long MaximumDecodedBytes { get; set; } = 256 * 1024 * 1024;
    public long MaximumExpansionRatio { get; set; } = 1000;
}

public sealed class PdfStreamFilterService
{
    public PdfStreamDecodeOptions Options { get; }

    public PdfStreamFilterService(PdfStreamDecodeOptions? options = null)
    {
        Options = options ?? new PdfStreamDecodeOptions();
    }

    public byte[] Decode(CosStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var data = stream.RawData;
        data = ValidateAndDecode(data);

        if (!stream.Dictionary.TryGetValue("Filter", out var filter))
        {
            return ApplyDictionaryPredictor(data, stream.Dictionary);
        }

        var filters = filter switch
        {
            CosName name => [name],
            CosArray filterArray => filterArray.Values,
            _ => throw new IOException("PDF stream /Filter must be a name or array.")
        };

        CosArray? paramsArray = stream.Dictionary.TryGetValue("DecodeParms", out var parms) &&
            parms is CosArray parmsArray ? parmsArray : null;
        CosDictionary? singleParams = stream.Dictionary.TryGetValue("DecodeParms", out var single) &&
            single is CosDictionary dictionary ? dictionary : null;

        for (var i = 0; i < filters.Count; i++)
        {
            if (filters[i] is not CosName filterName)
            {
                throw new IOException("PDF stream filter names must be COS names.");
            }
            CosDictionary? decodeParams = paramsArray is not null
                ? paramsArray.Values[i] as CosDictionary
                : singleParams;
            var encodedSize = data.LongLength;
            data = DecodeOne(filterName.Text, data, decodeParams);
            data = ValidateAndDecode(data);
            if (encodedSize > 0 && data.LongLength / encodedSize > Options.MaximumExpansionRatio)
            {
                throw new PdfResourceLimitExceededException(
                    $"PDF stream expansion ratio exceeds the configured limit of {Options.MaximumExpansionRatio}.");
            }
        }

        return ApplyDictionaryPredictor(data, stream.Dictionary);
    }

    private byte[] DecodeOne(string name, byte[] encoded, CosDictionary? parameters)
    {
        return name switch
        {
            "ASCIIHexDecode" or "AHx" => DecodeAsciiHex(encoded),
            "ASCII85Decode" or "A85" => DecodeAscii85(encoded),
            "RunLengthDecode" or "RL" => DecodeRunLength(encoded),
            "FlateDecode" or "Fl" => DecodeFlate(encoded),
            "LZWDecode" or "LZW" => DecodeLzw(encoded, parameters),
            "DCTDecode" or "JPXDecode" or "JBIG2Decode" =>
                throw new IOException($"PDF filter '{name}' requires a later external-format decoder."),
            _ => throw new IOException($"Unsupported PDF stream filter '{name}'.")
        };
    }

    private byte[] ValidateAndDecode(byte[] data)
    {
        if (data.LongLength > Options.MaximumDecodedBytes)
        {
            throw new PdfResourceLimitExceededException(
                $"Decoded PDF stream exceeds the {Options.MaximumDecodedBytes} byte limit.");
        }
        return data;
    }

    public static byte[] DecodeAsciiHex(byte[] encoded)
    {
        var output = new MemoryStream();
        int? high = null;
        foreach (var value in encoded)
        {
            if (value is 0x3e)
            {
                break;
            }
            if (IsAsciiWhitespace(value))
            {
                continue;
            }
            var hex = HexValue(value);
            if (hex < 0)
            {
                continue;
            }
            if (high.HasValue)
            {
                output.WriteByte((byte)((high.Value << 4) | hex));
                high = null;
            }
            else
            {
                high = hex;
            }
        }
        if (high.HasValue)
        {
            output.WriteByte((byte)(high.Value << 4));
        }
        return output.ToArray();
    }

    public static byte[] DecodeAscii85(byte[] encoded)
    {
        var output = new MemoryStream();
        var group = new byte[5];
        var groupLength = 0;

        foreach (var value in encoded)
        {
            if (IsAsciiWhitespace(value))
            {
                continue;
            }
            if (value == '~')
            {
                break;
            }
            if (value == 'z')
            {
                if (groupLength != 0)
                {
                    throw new IOException("Invalid ASCII85 'z' inside a partial group.");
                }
                output.Write(new byte[] { 0, 0, 0, 0 });
                continue;
            }
            if (value is < 33 or > 117)
            {
                throw new IOException($"Invalid ASCII85 byte 0x{value:X2}.");
            }

            group[groupLength++] = value;
            if (groupLength == 5)
            {
                WriteAscii85Group(output, group, 5);
                groupLength = 0;
            }
        }

        if (groupLength > 0)
        {
            for (var i = groupLength; i < 5; i++)
            {
                group[i] = (byte)'u';
            }
            WriteAscii85Group(output, group, groupLength);
        }
        return output.ToArray();
    }

    private static void WriteAscii85Group(Stream output, byte[] group, int count)
    {
        ulong value = 0;
        for (var i = 0; i < 5; i++)
        {
            value = checked(value * 85 + (uint)(group[i] - '!'));
        }

        var bytes = new byte[]
        {
            (byte)(value >> 24),
            (byte)(value >> 16),
            (byte)(value >> 8),
            (byte)value
        };
        output.Write(bytes, 0, count - 1);
    }

    public static byte[] DecodeRunLength(byte[] encoded)
    {
        var output = new MemoryStream();
        for (var i = 0; i < encoded.Length;)
        {
            var length = encoded[i++];
            if (length == 128)
            {
                break;
            }
            if (length <= 127)
            {
                var count = length + 1;
                if (i + count > encoded.Length)
                {
                    throw new IOException("Truncated PDF RunLength literal run.");
                }
                output.Write(encoded, i, count);
                i += count;
            }
            else
            {
                var count = 257 - length;
                if (i >= encoded.Length)
                {
                    throw new IOException("Truncated PDF RunLength repeat run.");
                }
                var value = encoded[i++];
                for (var repeat = 0; repeat < count; repeat++)
                {
                    output.WriteByte(value);
                }
            }
        }
        return output.ToArray();
    }

    public byte[] DecodeFlate(byte[] encoded)
    {
        try
        {
            using var input = new MemoryStream(encoded);
            using var zlib = new ZLibStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            CopyWithLimit(zlib, output);
            return output.ToArray();
        }
        catch (InvalidDataException)
        {
            using var input = new MemoryStream(encoded);
            using var raw = new DeflateStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            CopyWithLimit(raw, output);
            return output.ToArray();
        }
    }

    private void CopyWithLimit(Stream input, MemoryStream output)
    {
        var buffer = new byte[8192];
        while (true)
        {
            var read = input.Read(buffer, 0, buffer.Length);
            if (read == 0)
            {
                return;
            }
            if (output.Length + read > Options.MaximumDecodedBytes)
            {
                throw new PdfResourceLimitExceededException(
                    $"Decoded PDF stream exceeds the {Options.MaximumDecodedBytes} byte limit.");
            }
            output.Write(buffer, 0, read);
        }
    }

    public static byte[] DecodeLzw(byte[] encoded, CosDictionary? parameters)
    {
        var earlyChange = 1;
        if (parameters?.TryGetValue("EarlyChange", out var early) == true &&
            early is CosInteger earlyValue)
        {
            earlyChange = earlyValue.Value == 0 ? 0 : 1;
        }

        var table = new List<byte[]>(4096);
        for (var i = 0; i < 256; i++)
        {
            table.Add([(byte)i]);
        }
        table.Add([]);
        table.Add([]);

        var output = new MemoryStream();
        var bitPosition = 0;
        var codeWidth = 9;
        int? previous = null;

        while (true)
        {
            var code = ReadLzwCode(encoded, ref bitPosition, codeWidth);
            if (code == -1)
            {
                break;
            }
            if (code == 256)
            {
                table.Clear();
                for (var i = 0; i < 256; i++)
                {
                    table.Add([(byte)i]);
                }
                table.Add([]);
                table.Add([]);
                codeWidth = 9;
                previous = null;
                continue;
            }
            if (code == 257)
            {
                break;
            }

            byte[] sequence;
            if (code >= 0 && code < table.Count)
            {
                sequence = table[code];
            }
            else if (code == table.Count && previous.HasValue)
            {
                var first = table[previous.Value];
                sequence = new byte[first.Length + 1];
                first.CopyTo(sequence, 0);
                sequence[^1] = first[0];
            }
            else
            {
                throw new IOException("Invalid PDF LZW code.");
            }

            output.Write(sequence);
            if (previous.HasValue && table.Count < 4096)
            {
                var previousSequence = table[previous.Value];
                var newSequence = new byte[previousSequence.Length + 1];
                previousSequence.CopyTo(newSequence, 0);
                newSequence[^1] = sequence[0];
                table.Add(newSequence);
            }
            previous = code;

            var thresholdSize = table.Count + earlyChange;
            codeWidth = thresholdSize switch
            {
                >= 2048 => 12,
                >= 1024 => 11,
                >= 512 => 10,
                _ => 9
            };
        }
        return output.ToArray();
    }

    private static int ReadLzwCode(byte[] data, ref int bitPosition, int width)
    {
        if (bitPosition + width > data.Length * 8)
        {
            return -1;
        }
        var code = 0;
        for (var i = 0; i < width; i++)
        {
            var absolute = bitPosition++;
            var value = (data[absolute >> 3] >> (7 - (absolute & 7))) & 1;
            code = (code << 1) | value;
        }
        return code;
    }

    private byte[] ApplyDictionaryPredictor(byte[] data, CosDictionary dictionary)
    {
        if (!dictionary.TryGetValue("DecodeParms", out var value))
        {
            return data;
        }
        if (value is CosDictionary parameters)
        {
            return ApplyPredictor(data, parameters);
        }
        if (value is CosArray parameterArray)
        {
            foreach (var item in parameterArray.Values)
            {
                if (item is CosDictionary itemParameters)
                {
                    data = ApplyPredictor(data, itemParameters);
                }
            }
        }
        return data;
    }

    public static byte[] ApplyPredictor(byte[] data, CosDictionary parameters)
    {
        var predictor = GetInteger(parameters, "Predictor", 1);
        if (predictor == 1)
        {
            return data;
        }
        var colors = GetInteger(parameters, "Colors", 1);
        var bits = GetInteger(parameters, "BitsPerComponent", 8);
        var columns = GetInteger(parameters, "Columns", 1);
        if (bits is not (8 or 16))
        {
            throw new IOException($"PDF predictor cannot use {bits} bits per component.");
        }
        if (colors < 1 || columns < 1)
        {
            throw new IOException("PDF predictor dimensions are invalid.");
        }

        var bytesPerPixel = checked((colors * bits + 7) / 8);
        var rowLength = checked((columns * colors * bits + 7) / 8);
        var rowCount = checked(data.Length / (predictor >= 10 ? rowLength + 1 : rowLength));
        var output = new byte[rowCount * rowLength];
        var previous = new byte[rowLength];
        var dataOffset = 0;
        var outputOffset = 0;

        for (var row = 0; row < rowCount; row++)
        {
            var rowPredictor = predictor;
            if (predictor >= 10)
            {
                if (dataOffset >= data.Length)
                {
                    throw new IOException("PDF PNG predictor is missing a row tag.");
                }
                rowPredictor = 10 + data[dataOffset++];
                if (rowPredictor is < 10 or > 14)
                {
                    throw new IOException($"Unknown PDF PNG predictor tag {rowPredictor - 10}.");
                }
            }
            if (dataOffset + rowLength > data.Length)
            {
                throw new IOException("PDF predictor data is truncated.");
            }
            var current = new byte[rowLength];
            Array.Copy(data, dataOffset, current, 0, rowLength);
            dataOffset += rowLength;
            DecodePredictorRow(current, previous, rowPredictor, bytesPerPixel, bits);
            Array.Copy(current, 0, output, outputOffset, rowLength);
            outputOffset += rowLength;
            previous = current;
        }

        if (outputOffset != output.Length)
        {
            throw new IOException("PDF predictor row length is invalid.");
        }
        return output;
    }

    private static void DecodePredictorRow(byte[] row, byte[] previous, int predictor,
        int bytesPerPixel, int bitsPerComponent)
    {
        if (predictor == 2)
        {
            if (bitsPerComponent == 16)
            {
                for (var i = 0; i < row.Length; i += 2)
                {
                    var value = (row[i] << 8) | row[i + 1];
                    var left = i >= bytesPerPixel
                        ? (row[i - bytesPerPixel] << 8) | row[i - bytesPerPixel + 1]
                        : 0;
                    var combined = (value + left) & 0xffff;
                    row[i] = (byte)(combined >> 8);
                    row[i + 1] = (byte)combined;
                }
            }
            else
            {
                for (var i = 0; i < row.Length; i++)
                {
                    var left = i >= bytesPerPixel ? row[i - bytesPerPixel] : 0;
                    row[i] = (byte)((row[i] & 0xff) + (left & 0xff));
                }
            }
            return;
        }

        for (var i = 0; i < row.Length; i++)
        {
            var value = row[i] & 0xff;
            var left = i >= bytesPerPixel ? row[i - bytesPerPixel] & 0xff : 0;
            var up = previous[i] & 0xff;
            var upperLeft = i >= bytesPerPixel ? previous[i - bytesPerPixel] & 0xff : 0;
            var result = predictor switch
            {
                10 => value,
                11 => value + left,
                12 => value + up,
                13 => value + (left + up) / 2,
                14 => Paeth(value, left, up, upperLeft),
                _ => throw new IOException($"Unsupported PDF predictor {predictor}.")
            };
            row[i] = (byte)result;
        }
    }

    private static int Paeth(int value, int left, int up, int upperLeft)
    {
        var predictor = left + up - upperLeft;
        var leftDistance = Math.Abs(predictor - left);
        var upDistance = Math.Abs(predictor - up);
        var upperLeftDistance = Math.Abs(predictor - upperLeft);
        var add = leftDistance <= upDistance && leftDistance <= upperLeftDistance
            ? left
            : upDistance <= upperLeftDistance ? up : upperLeft;
        return value + add;
    }

    private static int GetInteger(CosDictionary dictionary, string name, int defaultValue) =>
        dictionary.TryGetValue(name, out var value) && value is CosInteger integer
            ? checked((int)integer.Value)
            : defaultValue;

    private static bool IsAsciiWhitespace(byte value) =>
        value is 0 or 9 or 10 or 12 or 13 or 32;

    private static int HexValue(byte value) =>
        value switch
        {
            >= (byte)'0' and <= (byte)'9' => value - '0',
            >= (byte)'A' and <= (byte)'F' => value - 'A' + 10,
            >= (byte)'a' and <= (byte)'f' => value - 'a' + 10,
            _ => -1
        };
}
