using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Fonts;

namespace VeraPdf.Parser.Pd;

public sealed class PdfResources : PdfDictionaryObject
{
    public PdfResources(CosDictionary dictionary)
        : base(dictionary)
    {
    }

    public IReadOnlyList<string> GetFontNames()
    {
        if (!Dictionary.TryGetValue("Font", out var value) || value is not CosDictionary fonts)
        {
            return [];
        }
        return fonts.Entries.Keys.ToArray();
    }

    public IReadOnlyList<string> GetXObjectNames()
    {
        if (!Dictionary.TryGetValue("XObject", out var value) || value is not CosDictionary objects)
        {
            return [];
        }
        return objects.Entries.Keys.ToArray();
    }

    public IReadOnlyList<string> GetColorSpaceNames()
    {
        if (!Dictionary.TryGetValue("ColorSpace", out var value) || value is not CosDictionary spaces)
        {
            return [];
        }
        return spaces.Entries.Keys.ToArray();
    }

    public PdfFont? GetFont(string name, PdfDocument document)
    {
        if (!Dictionary.TryGetValue("Font", out var value) ||
            value is not CosDictionary fonts ||
            !fonts.TryGetValue(name, out var fontValue))
        {
            return null;
        }
        var fontDictionary = document.Reader.ResolveDictionary(fontValue);
        return fontDictionary is null
            ? null
            : new PdfFontFactory().Create(fontDictionary);
    }
}
