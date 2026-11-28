// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Text;

namespace VeraPdf.Library.Xmp;

/// <summary>Utilities for XMP arrays.</summary>
public static class XmpArrayUtilities
{
    private enum CharacterKind { Normal, Space, Comma, Semicolon, Quote, Control }

    /// <summary>
    /// Joins the simple items of a non-alternative array using XMP's separator and quoting rules.
    /// A missing array produces an empty string. The packet is not modified.
    /// </summary>
    public static string CatenateArrayItems(XmpPacket packet, XmpSchemaRegistry registry,
        string schemaUri, string arrayPath, string? separator = null, string? quotes = null,
        bool allowCommas = false)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(arrayPath))
            throw new XmpException("Schema and array name are required", XmpError.BadParameter);
        var array = packet.GetPropertyByPath(registry, schemaUri, arrayPath);
        if (array is null) return "";
        if (array.Kind is not (XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray))
            throw new XmpException("Named property must be non-alternate array", XmpError.BadParameter);

        separator = string.IsNullOrEmpty(separator) ? "; " : separator;
        quotes = string.IsNullOrEmpty(quotes) ? "\"" : quotes;
        ValidateSeparator(separator);
        var openQuote = quotes[0];
        var closeQuote = quotes.Length == 1 ? openQuote : quotes[1];
        if (Classify(openQuote) != CharacterKind.Quote || Classify(closeQuote) != CharacterKind.Quote)
            throw new XmpException("Invalid quoting character", XmpError.BadParameter);
        if (ClosingQuote(openQuote) != closeQuote)
            throw new XmpException("Mismatched quote pair", XmpError.BadParameter);

        var result = new StringBuilder();
        var first = true;
        foreach (var item in array.Children)
        {
            if (item.Kind is not (XmpNodeKind.Literal or XmpNodeKind.Uri))
                throw new XmpException("Array items must be simple", XmpError.BadParameter);
            if (!first) result.Append(separator);
            result.Append(ApplyQuotes(item.Value ?? "", openQuote, closeQuote, allowCommas));
            first = false;
        }
        return result.ToString();
    }

    /// <summary>
    /// Separates an edit string into distinct array-item values in first-occurrence order.
    /// This does not create or modify a packet.
    /// </summary>
    public static IReadOnlyList<string> SeparateArrayItems(string value, bool preserveCommas = false)
    {
        if (value is null) throw new XmpException("Parameter must not be null", XmpError.BadParameter);
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var position = 0;
        while (position < value.Length)
        {
            while (position < value.Length && Classify(value[position]) is not (CharacterKind.Normal or CharacterKind.Quote))
                position++;
            if (position == value.Length) break;

            string item;
            if (Classify(value[position]) != CharacterKind.Quote)
            {
                var start = position;
                while (position < value.Length)
                {
                    var kind = Classify(value[position]);
                    if (kind is CharacterKind.Normal or CharacterKind.Quote ||
                        kind == CharacterKind.Comma && preserveCommas)
                    {
                        position++;
                        continue;
                    }
                    if (kind == CharacterKind.Space && position + 1 < value.Length)
                    {
                        var next = Classify(value[position + 1]);
                        if (next is CharacterKind.Normal or CharacterKind.Quote ||
                            next == CharacterKind.Comma && preserveCommas)
                        {
                            position++;
                            continue;
                        }
                    }
                    break;
                }
                item = value[start..position];
            }
            else
            {
                var open = value[position++];
                var close = ClosingQuote(open);
                var builder = new StringBuilder();
                while (position < value.Length)
                {
                    var character = value[position++];
                    if (Classify(character) != CharacterKind.Quote || !IsSurroundingQuote(character, open, close))
                    {
                        builder.Append(character);
                    }
                    else if (position < value.Length && character == value[position])
                    {
                        builder.Append(character);
                        position++;
                    }
                    else if (!IsClosingQuote(character, open, close))
                    {
                        builder.Append(character);
                    }
                    else
                    {
                        break;
                    }
                }
                item = builder.ToString();
            }
            if (seen.Add(item)) result.Add(item);
        }
        return result.AsReadOnly();
    }

    /// <summary>
    /// Returns a packet with separated values appended to a Bag or Seq, preserving existing items.
    /// A missing top-level array or field of an existing structure is created as a Bag by default.
    /// </summary>
    public static XmpPacket AppendSeparatedArrayItems(XmpPacket packet, XmpSchemaRegistry registry,
        string schemaUri, string arrayPath, string value, bool preserveCommas = false,
        XmpNodeKind? arrayKind = null)
    {
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(registry);
        if (string.IsNullOrEmpty(schemaUri) || string.IsNullOrEmpty(arrayPath) || value is null)
            throw new XmpException("Schema, array name and value are required", XmpError.BadParameter);
        if (arrayKind is not (null or XmpNodeKind.UnorderedArray or XmpNodeKind.OrderedArray))
            throw new XmpException("Only Bag and Seq array forms are supported", XmpError.BadOptions);
        return packet.WithSeparatedArrayItems(registry, schemaUri, arrayPath,
            SeparateArrayItems(value, preserveCommas), arrayKind);
    }

    private static void ValidateSeparator(string separator)
    {
        var found = false;
        foreach (var character in separator)
        {
            var kind = Classify(character);
            if (kind == CharacterKind.Semicolon && !found) found = true;
            else if (kind != CharacterKind.Space)
                throw new XmpException("Separator must contain one semicolon and only spaces", XmpError.BadParameter);
        }
        if (!found) throw new XmpException("Separator must contain one semicolon", XmpError.BadParameter);
    }

    private static string ApplyQuotes(string item, char open, char close, bool allowCommas)
    {
        var previousSpace = false;
        for (var index = 0; index < item.Length; index++)
        {
            var kind = Classify(item[index]);
            var needsQuotes = index == 0 && kind == CharacterKind.Quote ||
                kind == CharacterKind.Space && previousSpace ||
                kind is CharacterKind.Semicolon or CharacterKind.Control ||
                kind == CharacterKind.Comma && !allowCommas;
            if (needsQuotes)
            {
                // The reference leaves the prefix through the triggering character undoubled.
                var split = index == 0 && kind == CharacterKind.Quote ? 0 : index + 1;
                var result = new StringBuilder().Append(open).Append(item.AsSpan(0, split));
                foreach (var character in item.AsSpan(split))
                {
                    result.Append(character);
                    if (Classify(character) == CharacterKind.Quote && IsSurroundingQuote(character, open, close))
                        result.Append(character);
                }
                return result.Append(close).ToString();
            }
            previousSpace = kind == CharacterKind.Space;
        }
        return item;
    }

    private static bool IsSurroundingQuote(char character, char open, char close) =>
        character == open || IsClosingQuote(character, open, close);

    private static bool IsClosingQuote(char character, char open, char close) =>
        character == close || open == '\u301d' && character == '\u301e' || character == '\u301f';

    private static CharacterKind Classify(char character)
    {
        if (character is ' ' or '\u3000' or '\u303f' or >= '\u2000' and <= '\u200b')
            return CharacterKind.Space;
        if (",\uff0c\uff64\ufe50\ufe51\u3001\u060c\u055d".Contains(character))
            return CharacterKind.Comma;
        if (";\uff1b\ufe54\u061b\u037e".Contains(character)) return CharacterKind.Semicolon;
        if ("\"\u00ab\u00bb\u301d\u301e\u301f\u2015\u2039\u203a".Contains(character) ||
            character is >= '\u3008' and <= '\u300f' or >= '\u2018' and <= '\u201f')
            return CharacterKind.Quote;
        if (character < '\u0020' || character is '\u2028' or '\u2029') return CharacterKind.Control;
        return CharacterKind.Normal;
    }

    private static char ClosingQuote(char open) => open switch
    {
        '"' or '\u2015' => open,
        '\u00ab' => '\u00bb',
        '\u00bb' => '\u00ab',
        '\u2018' => '\u2019',
        '\u201a' => '\u201b',
        '\u201c' => '\u201d',
        '\u201e' => '\u201f',
        '\u2039' => '\u203a',
        '\u203a' => '\u2039',
        '\u3008' => '\u3009',
        '\u300a' => '\u300b',
        '\u300c' => '\u300d',
        '\u300e' => '\u300f',
        '\u301d' => '\u301f',
        _ => '\0'
    };
}
