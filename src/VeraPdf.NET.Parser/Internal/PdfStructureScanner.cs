using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VeraPdf.NET.Parser.ModelMapping;
using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Parser.Internal;

internal static partial class PdfStructureScanner
{
    [GeneratedRegex("%PDF-(?<version>\\d\\.\\d)", RegexOptions.CultureInvariant)]
    private static partial Regex HeaderRegex();

    [GeneratedRegex("(?m)^\\s*\\d+\\s+\\d+\\s+obj\\b", RegexOptions.CultureInvariant)]
    private static partial Regex IndirectObjectRegex();

    [GeneratedRegex("startxref\\s*(?<offset>\\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex StartXrefRegex();

    [GeneratedRegex("/(?<key>[A-Za-z0-9]+)", RegexOptions.CultureInvariant)]
    private static partial Regex DictKeyRegex();

    [GeneratedRegex("/Size\\s+(?<size>\\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex TrailerSizeRegex();

    [GeneratedRegex("/ID\\s*\\[\\s*(?:<(?<firstHex>[^>]+)>|\\((?<firstLit>[^)]*)\\))\\s*(?:<(?<lastHex>[^>]+)>|\\((?<lastLit>[^)]*)\\))", RegexOptions.CultureInvariant)]
    private static partial Regex TrailerIdRegex();

    public static ParsedPdfSnapshot Scan(byte[] bytes, PdfParseOptions options, List<ParseDiagnostic> diagnostics)
    {
        var text = Encoding.ASCII.GetString(bytes);

        var headerMatch = HeaderRegex().Match(text);
        var hasHeader = headerMatch.Success;
        var headerOffset = hasHeader ? headerMatch.Index : -1;
        var header = hasHeader ? headerMatch.Value : null;
        var headerVersion = hasHeader
            ? decimal.Parse(headerMatch.Groups["version"].Value, CultureInfo.InvariantCulture)
            : 0m;

        if (!hasHeader)
        {
            diagnostics.Add(new ParseDiagnostic("PDF001", "PDF header marker '%PDF-' was not found.", ParseDiagnosticSeverity.Error));
        }
        else if (headerOffset != 0)
        {
            diagnostics.Add(new ParseDiagnostic(
                "PDF002",
                $"PDF header marker was found at byte offset {headerOffset}, expected 0.",
                options.StrictHeaderCheck ? ParseDiagnosticSeverity.Error : ParseDiagnosticSeverity.Warning));
        }

        var eofIndex = text.LastIndexOf("%%EOF", StringComparison.Ordinal);
        var hasEof = eofIndex >= 0;

        if (!hasEof)
        {
            diagnostics.Add(new ParseDiagnostic(
                "PDF003",
                "EOF marker '%%EOF' was not found.",
                options.RequireEofMarker ? ParseDiagnosticSeverity.Error : ParseDiagnosticSeverity.Warning));
        }

        var postEofDataSize = ComputePostEofDataSize(text, eofIndex);
        if (postEofDataSize > 0)
        {
            diagnostics.Add(new ParseDiagnostic(
                "PDF004",
                $"Found {postEofDataSize} byte(s) of trailing data after last EOF marker.",
                ParseDiagnosticSeverity.Warning));
        }

        var startXref = TryGetStartXrefOffset(text, out var startXrefOffset);
        if (!startXref)
        {
            diagnostics.Add(new ParseDiagnostic(
                "PDF005",
                "startxref section was not found or did not contain a numeric offset.",
                options.RequireStartXref ? ParseDiagnosticSeverity.Error : ParseDiagnosticSeverity.Warning));
        }

        var indirectObjectCount = IndirectObjectRegex().Matches(text).Count;
        var hasCatalogObject = text.Contains("/Type /Catalog", StringComparison.Ordinal);
        if (!hasCatalogObject)
        {
            diagnostics.Add(new ParseDiagnostic(
                "PDF006",
                "Catalog object marker '/Type /Catalog' was not found.",
                ParseDiagnosticSeverity.Warning));
        }

        var isLinearized = text.Contains("/Linearized", StringComparison.Ordinal);

        var hasInfoEntry = text.Contains("/Info", StringComparison.Ordinal);
        var hasPieceInfoEntry = text.Contains("/PieceInfo", StringComparison.Ordinal);
        var hasEmbeddedFilesEntry = text.Contains("/EmbeddedFiles", StringComparison.Ordinal);
        var hasMarkInfoEntry = text.Contains("/MarkInfo", StringComparison.Ordinal);
        var markedTrue = text.Contains("/Marked true", StringComparison.OrdinalIgnoreCase);
        var suspectsTrue = text.Contains("/Suspects true", StringComparison.OrdinalIgnoreCase);
        var hasViewerPreferencesEntry = text.Contains("/ViewerPreferences", StringComparison.Ordinal);
        var displayDocTitleTrue = text.Contains("/DisplayDocTitle true", StringComparison.OrdinalIgnoreCase);
        var hasMetadataEntry = text.Contains("/Metadata", StringComparison.Ordinal);
        var hasXRefStream = text.Contains("/Type /XRef", StringComparison.Ordinal);

        var (headerByte1, headerByte2, headerByte3, headerByte4) = ExtractHeaderCommentBytes(bytes, headerOffset);

        var hasXrefKeyword = TryFindLastXrefKeyword(text, out var xrefIndex);
        var xrefSubsectionHeaderSpaceSeparated = hasXrefKeyword && TryCheckXrefSubsectionHeaderSpaceSeparated(text, xrefIndex);
        var xrefEolMarkersComplyPdfa = hasXrefKeyword && CheckXrefKeywordEolCompliance(text, xrefIndex);

        var hasTrailer = TryExtractLastTrailerDictionary(text, out var trailerDictionary);
        var trailerContainsEncrypt = hasTrailer && trailerDictionary.Contains("/Encrypt", StringComparison.Ordinal);
        var trailerContainsInfo = hasTrailer && trailerDictionary.Contains("/Info", StringComparison.Ordinal);
        var trailerSize = hasTrailer && TryExtractTrailerSize(trailerDictionary, out var size) ? size : 0;
        var trailerKeysString = hasTrailer ? BuildKeysString(trailerDictionary) : null;
        var trailerRawMarkInfo = ExtractRawSubDictionary(text, "/MarkInfo");
        var trailerRawViewerPreferences = ExtractRawSubDictionary(text, "/ViewerPreferences");
        var (firstId, lastId) = hasTrailer ? ExtractTrailerIds(trailerDictionary) : (null, null);

        return new ParsedPdfSnapshot
        {
            HasHeader = hasHeader,
            HasEofMarker = hasEof,
            HasStartXref = startXref,
            HasXrefKeyword = hasXrefKeyword,
            XrefSubsectionHeaderSpaceSeparated = xrefSubsectionHeaderSpaceSeparated,
            XrefEolMarkersComplyPdfa = xrefEolMarkersComplyPdfa,
            HasCatalogObject = hasCatalogObject,
            HasInfoEntry = hasInfoEntry,
            HasPieceInfoEntry = hasPieceInfoEntry,
            HasEmbeddedFilesEntry = hasEmbeddedFilesEntry,
            HasMarkInfoEntry = hasMarkInfoEntry,
            MarkedTrue = markedTrue,
            SuspectsTrue = suspectsTrue,
            HasViewerPreferencesEntry = hasViewerPreferencesEntry,
            DisplayDocTitleTrue = displayDocTitleTrue,
            HasMetadataEntry = hasMetadataEntry,
            HasXRefStream = hasXRefStream,
            IsLinearized = isLinearized,
            HasTrailer = hasTrailer,
            TrailerContainsEncrypt = trailerContainsEncrypt,
            TrailerContainsInfo = trailerContainsInfo,
            TrailerSize = trailerSize,
            TrailerKeysString = trailerKeysString,
            TrailerRawMarkInfo = trailerRawMarkInfo,
            TrailerRawViewerPreferences = trailerRawViewerPreferences,
            FirstId = firstId,
            LastId = lastId,
            Header = header,
            HeaderVersion = headerVersion,
            HeaderOffset = headerOffset,
            HeaderByte1 = headerByte1,
            HeaderByte2 = headerByte2,
            HeaderByte3 = headerByte3,
            HeaderByte4 = headerByte4,
            PostEofDataSize = postEofDataSize,
            IndirectObjectCount = indirectObjectCount,
            StartXrefOffset = startXrefOffset
        };
    }

    private static bool TryFindLastXrefKeyword(string text, out int index)
    {
        var searchIndex = text.Length - 1;

        while (searchIndex >= 0)
        {
            var candidate = text.LastIndexOf("xref", searchIndex, StringComparison.Ordinal);
            if (candidate < 0)
                break;

            var beforeIsBoundary = candidate == 0 || char.IsWhiteSpace(text[candidate - 1]);
            var afterIndex = candidate + 4;
            var afterIsBoundary = afterIndex >= text.Length || char.IsWhiteSpace(text[afterIndex]);

            if (beforeIsBoundary && afterIsBoundary)
            {
                index = candidate;
                return true;
            }

            searchIndex = candidate - 1;
        }

        index = -1;
        return false;
    }

    private static bool TryCheckXrefSubsectionHeaderSpaceSeparated(string text, int xrefIndex)
    {
        var lineStart = xrefIndex;
        while (lineStart < text.Length && text[lineStart] != '\n' && text[lineStart] != '\r')
            lineStart++;

        while (lineStart < text.Length && (text[lineStart] == '\n' || text[lineStart] == '\r'))
            lineStart++;

        if (lineStart >= text.Length)
            return false;

        var lineEnd = lineStart;
        while (lineEnd < text.Length && text[lineEnd] != '\n' && text[lineEnd] != '\r')
            lineEnd++;

        var headerLine = text[lineStart..lineEnd].Trim();
        if (headerLine.Length == 0)
            return false;

        var parts = headerLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 &&
               int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out _) &&
               int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
    }

    private static bool CheckXrefKeywordEolCompliance(string text, int xrefIndex)
    {
        var afterKeyword = xrefIndex + 4;
        if (afterKeyword >= text.Length)
            return false;

        if (text.AsSpan(afterKeyword).StartsWith("\r\n", StringComparison.Ordinal))
            return true;

        return text[afterKeyword] == '\n' || text[afterKeyword] == '\r';
    }

    private static bool TryExtractLastTrailerDictionary(string text, out string trailerDictionary)
    {
        trailerDictionary = string.Empty;

        var trailerIndex = text.LastIndexOf("trailer", StringComparison.Ordinal);
        if (trailerIndex < 0)
            return false;

        var index = trailerIndex + "trailer".Length;

        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;

        if (index + 1 >= text.Length || text[index] != '<' || text[index + 1] != '<')
            return false;

        var start = index;
        var depth = 0;

        while (index + 1 < text.Length)
        {
            if (text[index] == '<' && text[index + 1] == '<')
            {
                depth++;
                index += 2;
                continue;
            }

            if (text[index] == '>' && text[index + 1] == '>')
            {
                depth--;
                index += 2;

                if (depth == 0)
                {
                    trailerDictionary = text[start..index];
                    return true;
                }

                continue;
            }

            index++;
        }

        return false;
    }

    private static bool TryExtractTrailerSize(string trailerDictionary, out int size)
    {
        size = 0;
        var match = TrailerSizeRegex().Match(trailerDictionary);

        return match.Success && int.TryParse(match.Groups["size"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out size);
    }

    private static string BuildKeysString(string dictionary)
    {
        var keys = DictKeyRegex()
            .Matches(dictionary)
            .Select(m => m.Groups["key"].Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return string.Join("&", keys);
    }

    private static (string? first, string? last) ExtractTrailerIds(string trailerDictionary)
    {
        var match = TrailerIdRegex().Match(trailerDictionary);
        if (!match.Success)
            return (null, null);

        var first = match.Groups["firstHex"].Success
            ? match.Groups["firstHex"].Value
            : match.Groups["firstLit"].Value;

        var last = match.Groups["lastHex"].Success
            ? match.Groups["lastHex"].Value
            : match.Groups["lastLit"].Value;

        return (string.IsNullOrWhiteSpace(first) ? null : first, string.IsNullOrWhiteSpace(last) ? null : last);
    }

    private static string? ExtractRawSubDictionary(string text, string key)
    {
        var keyIndex = text.IndexOf(key, StringComparison.Ordinal);
        if (keyIndex < 0)
            return null;

        var index = keyIndex + key.Length;
        while (index < text.Length && char.IsWhiteSpace(text[index]))
            index++;

        if (index + 1 >= text.Length || text[index] != '<' || text[index + 1] != '<')
            return null;

        var start = index;
        var depth = 0;

        while (index + 1 < text.Length)
        {
            if (text[index] == '<' && text[index + 1] == '<')
            {
                depth++;
                index += 2;
                continue;
            }

            if (text[index] == '>' && text[index + 1] == '>')
            {
                depth--;
                index += 2;
                if (depth == 0)
                    return text[start..index];

                continue;
            }

            index++;
        }

        return null;
    }

    private static (int b1, int b2, int b3, int b4) ExtractHeaderCommentBytes(byte[] bytes, int headerOffset)
    {
        if (headerOffset < 0 || bytes.Length == 0)
            return (0, 0, 0, 0);

        var index = headerOffset;

        while (index < bytes.Length && bytes[index] != (byte)'\n' && bytes[index] != (byte)'\r')
            index++;

        while (index < bytes.Length && (bytes[index] == (byte)'\n' || bytes[index] == (byte)'\r'))
            index++;

        if (index >= bytes.Length || bytes[index] != (byte)'%')
            return (0, 0, 0, 0);

        index++;

        var b1 = index < bytes.Length ? bytes[index++] : 0;
        var b2 = index < bytes.Length ? bytes[index++] : 0;
        var b3 = index < bytes.Length ? bytes[index++] : 0;
        var b4 = index < bytes.Length ? bytes[index] : 0;

        return (b1, b2, b3, b4);
    }

    private static bool TryGetStartXrefOffset(string text, out int offset)
    {
        var matches = StartXrefRegex().Matches(text);
        if (matches.Count == 0)
        {
            offset = -1;
            return false;
        }

        var lastMatch = matches[^1];
        if (!int.TryParse(lastMatch.Groups["offset"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out offset))
        {
            offset = -1;
            return false;
        }

        return true;
    }

    private static int ComputePostEofDataSize(string text, int eofIndex)
    {
        if (eofIndex < 0)
            return 0;

        var afterEofIndex = eofIndex + "%%EOF".Length;
        if (afterEofIndex >= text.Length)
            return 0;

        var trailing = text.Substring(afterEofIndex);
        var trimmedLength = trailing.Trim('\r', '\n', '\t', ' ', '\0').Length;

        return trimmedLength;
    }
}
