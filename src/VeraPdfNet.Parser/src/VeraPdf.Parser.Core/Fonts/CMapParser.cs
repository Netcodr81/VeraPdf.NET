using System.Globalization;
using System.Text;

namespace VeraPdf.Parser.Fonts;

public sealed class CMapParser
{
    public PdfCMap Parse(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var text = Encoding.Latin1.GetString(bytes);
        var cmap = new PdfCMap();
        ParseWritingMode(text, cmap);

        ParseEntries(text, "begincodespacerange", "endcodespacerange",
            (start, end) => cmap.AddCodeRange(start, end));
        ParseEntries(text, "begincidchar", "endcidchar",
            (code, target) => cmap.AddCidMapping(code, target));
        ParseUnicodeMappings(text, cmap);

        ParseRanges(text, "begincidrange", "endcidrange",
            (start, end, target) => cmap.AddCidRange(start, end, target));
        ParseUnicodeRanges(text, cmap);
        return cmap;
    }

    private static void ParseWritingMode(string text, PdfCMap cmap)
    {
        const string wMode = "/WMode";
        var index = text.IndexOf(wMode, StringComparison.Ordinal);
        if (index >= 0)
        {
            var remainder = text[(index + wMode.Length)..];
            foreach (var character in remainder)
            {
                if (char.IsDigit(character))
                {
                    cmap.SetWritingMode(character - '0');
                    break;
                }
                if (!char.IsWhiteSpace(character) && character != '[')
                {
                    break;
                }
            }
        }
    }

    private static void ParseEntries(string text, string begin, string end,
        Action<byte[], long> add)
    {
        ParseSections(text, begin, end, section =>
        {
            foreach (var line in SectionLines(section))
            {
                var hex = GetHexTokens(line);
                if (hex.Count >= 2)
                {
                    var code = hex[0];
                    if (TryParseTarget(hex[1], out var target))
                    {
                        add(code, target);
                    }
                }
            }
        });
    }

    private static void ParseRanges(string text, string begin, string end,
        Action<byte[], byte[], long> add)
    {
        ParseSections(text, begin, end, section =>
        {
            foreach (var line in SectionLines(section))
            {
                var hex = GetHexTokens(line);
                if (hex.Count >= 3 && TryParseTarget(hex[2], out var target))
                {
                    add(hex[0], hex[1], target);
                }
            }
        });
    }

    private static void ParseSections(string text, string begin, string end,
        Action<string> process)
    {
        var position = 0;
        while (position < text.Length)
        {
            var start = text.IndexOf(begin, position, StringComparison.Ordinal);
            if (start < 0)
            {
                break;
            }
            var sectionStart = start + begin.Length;
            var stop = text.IndexOf(end, sectionStart, StringComparison.Ordinal);
            if (stop < 0)
            {
                break;
            }
            process(text[sectionStart..stop]);
            position = stop + end.Length;
        }
    }

    private static IEnumerable<string> SectionLines(string section) =>
        section.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

    private static void ParseUnicodeMappings(string text, PdfCMap cmap)
    {
        ParseSections(text, "beginbfchar", "endbfchar", section =>
        {
            foreach (var line in SectionLines(section))
            {
                var hex = GetHexTokens(line);
                if (hex.Count >= 2)
                {
                    cmap.AddUnicodeMapping(hex[0], DecodePdfHexString(hex[1]));
                }
            }
        });
    }

    private static void ParseUnicodeRanges(string text, PdfCMap cmap)
    {
        ParseSections(text, "beginbfrange", "endbfrange", section =>
        {
            foreach (var line in SectionLines(section))
            {
                var hex = GetHexTokens(line);
                if (hex.Count >= 3 && TryParseTarget(hex[2], out var target))
                {
                    cmap.AddUnicodeRange(hex[0], hex[1], target);
                }
            }
        });
    }

    private static List<byte[]> GetHexTokens(string line)
    {
        var result = new List<byte[]>();
        var position = 0;
        while (position < line.Length)
        {
            var start = line.IndexOf('<', position);
            if (start < 0)
            {
                break;
            }
            var stop = line.IndexOf('>', start + 1);
            if (stop < 0)
            {
                break;
            }
            result.Add(DecodeHexBytes(line[(start + 1)..stop]));
            position = stop + 1;
        }
        return result;
    }

    private static byte[] DecodeHexBytes(string text)
    {
        var digits = new string(text.Where(character =>
            char.IsAsciiHexDigit(character)).ToArray());
        if (digits.Length % 2 != 0)
        {
            digits += "0";
        }
        var result = new byte[digits.Length / 2];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = byte.Parse(digits.AsSpan(i * 2, 2),
                NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        }
        return result;
    }

    private static bool TryParseTarget(byte[] target, out long value)
    {
        if (target.Length == 0)
        {
            value = 0;
            return false;
        }
        value = 0;
        foreach (var targetByte in target)
        {
            value = (value << 8) | targetByte;
        }
        return true;
    }

    private static string DecodePdfHexString(byte[] bytes) =>
        Encoding.BigEndianUnicode.GetString(bytes.Length % 2 == 0
            ? bytes
            : bytes.Concat(new byte[] { 0 }).ToArray());

    private static void ParseEntries(string text, string begin, string end,
        Action<byte[], byte[]> addRange)
    {
        ParseSections(text, begin, end, section =>
        {
            foreach (var line in SectionLines(section))
            {
                var hex = GetHexTokens(line);
                if (hex.Count >= 2)
                {
                    addRange(hex[0], hex[1]);
                }
            }
        });
    }
}
