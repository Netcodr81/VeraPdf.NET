using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;

namespace VeraPdf.Parser.Fonts;

public sealed class StandardFontMetrics
{
    private static readonly ConcurrentDictionary<string, StandardFontMetrics> Cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _widths = new(StringComparer.Ordinal);

    public string FontName { get; private init; } = "";
    public double[] BoundingBox { get; private set; } = [0, 0, 1000, 1000];
    public IReadOnlyDictionary<string, double> Widths => _widths;

    public static double GetWidth(string? fontName, long characterCode)
    {
        var metrics = Load(fontName);
        if (metrics is null)
        {
            return 600;
        }
        if (characterCode is < 32 or > 126)
        {
            return metrics.Widths.GetValueOrDefault(".notdef", 250);
        }
        var character = (char)characterCode;
        return metrics.Widths.GetValueOrDefault(character.ToString(), 600);
    }

    public static StandardFontMetrics? Load(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
        {
            return null;
        }
        var normalized = fontName.Split(',')[0].Replace(" ", "", StringComparison.Ordinal);
        if (normalized.EndsWith("-Roman", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^6];
        }
        normalized = normalized switch
        {
            "Times-Roman" or "TimesNewRoman" or "Times" => "Times-Roman",
            "Times-Bold" => "Times-Bold",
            "Times-Italic" => "Times-Italic",
            "Times-BoldItalic" => "Times-BoldItalic",
            "Helvetica" or "Arial" => "Helvetica",
            "Helvetica-Bold" or "Arial-Bold" => "Helvetica-Bold",
            "Helvetica-Oblique" or "Arial-Italic" => "Helvetica-Oblique",
            "Helvetica-BoldOblique" => "Helvetica-BoldOblique",
            "Courier" => "Courier",
            "Courier-Bold" => "Courier-Bold",
            "Courier-Oblique" => "Courier-Oblique",
            "Courier-BoldOblique" => "Courier-BoldOblique",
            "Symbol" => "Symbol",
            "ZapfDingbats" => "ZapfDingbats",
            _ => normalized
        };
        return Cache.GetOrAdd(normalized, LoadEmbedded);
    }

    private static StandardFontMetrics LoadEmbedded(string fontName)
    {
        var result = new StandardFontMetrics { FontName = fontName };
        var assembly = typeof(StandardFontMetrics).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith($"/{fontName}.afm",
                StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith($".{fontName}.afm", StringComparison.OrdinalIgnoreCase));
        if (resourceName is null)
        {
            return result;
        }
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return result;
        }
        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (line.StartsWith("FontBBox ", StringComparison.Ordinal))
            {
                var values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1);
                var parsed = values.Select(value =>
                    double.Parse(value, CultureInfo.InvariantCulture)).ToArray();
                if (parsed.Length == 4)
                {
                    result.BoundingBox = parsed;
                }
            }
            if (line.StartsWith("C ", StringComparison.Ordinal))
            {
                var parts = line.Split([' ', ';'], StringSplitOptions.RemoveEmptyEntries);
                var widthIndex = Array.FindIndex(parts, part => part is "WX" or "W");
                var nameIndex = Array.FindIndex(parts, part => part == "N");
                if (widthIndex >= 0 && nameIndex >= 0 &&
                    double.TryParse(parts[widthIndex + 1], CultureInfo.InvariantCulture,
                        out var width))
                {
                    result._widths[parts[nameIndex + 1]] = width;
                }
            }
        }
        return result;
    }
}
