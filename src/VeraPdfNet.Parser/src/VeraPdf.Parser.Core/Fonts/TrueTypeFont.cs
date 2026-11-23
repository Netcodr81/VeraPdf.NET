using System.Text;

namespace VeraPdf.Parser.Fonts;

public sealed class TrueTypeFontInfo
{
    public byte[] Data { get; }
    public double UnitsPerEm { get; private set; } = 2048;
    public int NumberOfGlyphs { get; private set; }
    public IReadOnlyDictionary<int, int> CharacterToGlyph { get; }

    private readonly Dictionary<int, int> _characterToGlyph = [];

    public TrueTypeFontInfo(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length < 12)
        {
            throw new IOException("TrueType font data is truncated.");
        }
        Data = data.ToArray();
        CharacterToGlyph = _characterToGlyph;
        Parse();
    }

    private void Parse()
    {
        var numberOfTables = ReadUInt16(4);
        var headOffset = -1L;
        var maxpOffset = -1L;
        var cmapOffset = -1L;

        for (var i = 0; i < numberOfTables; i++)
        {
            var directoryOffset = 12 + i * 16;
            if (directoryOffset + 16 > Data.Length)
            {
                throw new IOException("TrueType table directory is truncated.");
            }
            var tag = Encoding.ASCII.GetString(Data, checked((int)directoryOffset), 4);
            var offset = ReadUInt32(directoryOffset + 8);
            if (tag == "head") headOffset = offset;
            if (tag == "maxp") maxpOffset = offset;
            if (tag == "cmap") cmapOffset = offset;
        }

        if (headOffset >= 0)
        {
            if (headOffset + 18 > Data.Length)
            {
                throw new IOException("TrueType head table is truncated.");
            }
            var units = ReadUInt16(headOffset + 18);
            if (units > 0)
            {
                UnitsPerEm = units;
            }
        }
        if (maxpOffset >= 0)
        {
            if (maxpOffset + 6 > Data.Length)
            {
                throw new IOException("TrueType maxp table is truncated.");
            }
            NumberOfGlyphs = ReadUInt16(maxpOffset + 4);
        }
        if (cmapOffset >= 0)
        {
            ParseCmap(cmapOffset);
        }
    }

    private void ParseCmap(long cmapOffset)
    {
        if (cmapOffset + 4 > Data.Length)
        {
            return;
        }
        var numberOfTables = ReadUInt16(cmapOffset + 2);
        for (var i = 0; i < numberOfTables; i++)
        {
            var recordOffset = cmapOffset + 4 + i * 8;
            if (recordOffset + 8 > Data.Length)
            {
                return;
            }
            var platform = ReadUInt16(recordOffset);
            var encoding = ReadUInt16(recordOffset + 2);
            var subtableOffset = cmapOffset + ReadUInt32(recordOffset + 4);
            if (subtableOffset + 6 > Data.Length)
            {
                continue;
            }
            var format = ReadUInt16(subtableOffset);
            if (format == 4)
            {
                ParseFormatFour(subtableOffset);
                if (platform == 3 || platform == 0)
                {
                    break;
                }
            }
            if (format == 12)
            {
                ParseFormatTwelve(subtableOffset);
                break;
            }
        }
    }

    private void ParseFormatFour(long offset)
    {
        var length = ReadUInt16(offset + 2);
        if (offset + length > Data.Length)
        {
            return;
        }
        var segCount = ReadUInt16(offset + 6) / 2;
        var endCodesOffset = offset + 14;
        var startCodesOffset = endCodesOffset + segCount * 2 + 2;
        var idDeltaOffset = startCodesOffset + segCount * 2;
        var idRangeOffsetOffset = idDeltaOffset + segCount * 2;

        for (var segment = 0; segment < segCount; segment++)
        {
            var endCode = ReadUInt16(endCodesOffset + segment * 2);
            var startCode = ReadUInt16(startCodesOffset + segment * 2);
            var idDelta = (short)ReadUInt16(idDeltaOffset + segment * 2);
            var idRangeOffset = ReadUInt16(idRangeOffsetOffset + segment * 2);

            for (var character = startCode; character <= endCode; character++)
            {
                int glyph;
                if (idRangeOffset == 0)
                {
                    glyph = (character + idDelta) & 0xffff;
                }
                else
                {
                    var glyphAddress = idRangeOffsetOffset + segment * 2 + idRangeOffset +
                        (character - startCode) * 2;
                    if (glyphAddress + 2 > Data.Length)
                    {
                        continue;
                    }
                    var glyphIndex = ReadUInt16(glyphAddress);
                    glyph = glyphIndex == 0 ? 0 : (glyphIndex + idDelta) & 0xffff;
                }
                _characterToGlyph[character] = glyph;
            }
        }
    }

    private void ParseFormatTwelve(long offset)
    {
        if (offset + 16 > Data.Length)
        {
            return;
        }
        var groupCount = checked((int)ReadUInt32(offset + 12));
        for (var i = 0; i < groupCount; i++)
        {
            var groupOffset = offset + 16 + i * 12;
            if (groupOffset + 12 > Data.Length)
            {
                return;
            }
            var start = checked((int)ReadUInt32(groupOffset));
            var end = checked((int)ReadUInt32(groupOffset + 4));
            var glyph = checked((int)ReadUInt32(groupOffset + 8));
            for (var character = start; character <= end; character++)
            {
                _characterToGlyph[character] = glyph + character - start;
                if (_characterToGlyph.Count >= 100000)
                {
                    throw new IOException("TrueType cmap contains too many entries.");
                }
            }
        }
    }

    private ushort ReadUInt16(long offset)
    {
        if (offset + 2 > Data.Length)
        {
            throw new IOException("TrueType font offset is outside the font.");
        }
        return (ushort)((Data[offset] << 8) | Data[offset + 1]);
    }

    private uint ReadUInt32(long offset)
    {
        if (offset + 4 > Data.Length)
        {
            throw new IOException("TrueType font offset is outside the font.");
        }
        return (uint)((Data[offset] << 24) | (Data[offset + 1] << 16) |
            (Data[offset + 2] << 8) | Data[offset + 3]);
    }
}
