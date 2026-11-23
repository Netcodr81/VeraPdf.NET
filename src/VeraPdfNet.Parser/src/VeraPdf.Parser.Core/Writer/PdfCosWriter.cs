using System.Globalization;
using System.Text;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Writer;

public static class PdfCosWriter
{
    public static void Write(Stream output, CosValue value)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(value);
        var bytes = Encoding.ASCII.GetBytes(ToPdf(value));
        output.Write(bytes);
    }

    public static string ToPdf(CosValue value)
    {
        return value switch
        {
            CosBoolean boolean => boolean.Value ? "true" : "false",
            CosInteger integer => integer.Value.ToString(CultureInfo.InvariantCulture),
            CosReal real => real.Value.ToString("R", CultureInfo.InvariantCulture),
            CosName name => "/" + EscapeName(Encoding.Latin1.GetString(name.Name)),
            CosString text => "<" + Convert.ToHexString(text.Value).ToLowerInvariant() + ">",
            CosNull => "null",
            CosIndirectReference reference =>
                $"{reference.ObjectNumber} {reference.Generation} R",
            CosIndirectObject indirect => ToPdf(indirect.Value),
            CosArray array => "[ " +
                string.Join(" ", array.Values.Select(ToPdf)) + " ]",
            CosDictionary dictionary => "<< " +
                string.Join(" ", dictionary.Entries
                    .Select(entry => "/" + EscapeName(entry.Key) + " " + ToPdf(entry.Value))) +
                " >>",
            CosStream stream => ToPdfStream(stream),
            _ => throw new IOException($"Cannot serialize COS value of type {value.GetType().Name}.")
        };
    }

    public static string ToPdfStream(CosStream stream)
    {
        var dictionary = new CosDictionary();
        foreach (var entry in stream.Dictionary.Entries)
        {
            dictionary.Set(entry.Key, entry.Value);
        }
        dictionary.Set("Length", new CosInteger(stream.RawData.Length));
        return ToPdf(dictionary) + "\nstream\n" +
            Encoding.Latin1.GetString(stream.RawData) +
            "\nendstream";
    }

    private static string EscapeName(string name)
    {
        var output = new StringBuilder();
        foreach (var character in name)
        {
            if (character is <= (char)32 or '/' or '#')
            {
                output.Append('#');
                output.Append(((byte)character).ToString("X2", CultureInfo.InvariantCulture));
            }
            else
            {
                output.Append(character);
            }
        }
        return output.ToString();
    }
}
