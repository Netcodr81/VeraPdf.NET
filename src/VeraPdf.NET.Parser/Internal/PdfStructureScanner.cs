using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using VeraPdf.NET.Parser.ModelMapping;
using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Parser.Internal;

/// <summary>
/// Performs lightweight structural scanning over raw PDF bytes to extract parser signals
/// used by <see cref="PdfParser"/> and downstream model mapping.
/// </summary>
internal static partial class PdfStructureScanner
{
    /// <summary>
    /// Gets the regular expression used to locate the PDF header and version marker.
    /// </summary>
    [GeneratedRegex("%PDF-(?<version>\\d\\.\\d)", RegexOptions.CultureInvariant)]
    private static partial Regex HeaderRegex();

    /// <summary>
    /// Gets the regular expression used to count indirect object declarations.
    /// </summary>
    [GeneratedRegex("(?m)^\\s*\\d+\\s+\\d+\\s+obj\\b", RegexOptions.CultureInvariant)]
    private static partial Regex IndirectObjectRegex();

    /// <summary>
    /// Gets the regular expression used to locate the final startxref offset token.
    /// </summary>
    [GeneratedRegex("startxref\\s*(?<offset>\\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex StartXrefRegex();

    /// <summary>
    /// Scans PDF bytes and extracts structural markers, metadata flags, and offsets into a snapshot.
    /// </summary>
    /// <param name="bytes">Raw PDF bytes to inspect.</param>
    /// <param name="options">Parse strictness options controlling diagnostic severity.</param>
    /// <param name="diagnostics">The mutable diagnostics collection to populate while scanning.</param>
    /// <returns>A snapshot containing extracted structural information.</returns>
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

        return new ParsedPdfSnapshot
        {
            HasHeader = hasHeader,
            HasEofMarker = hasEof,
            HasStartXref = startXref,
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

    /// <summary>
    /// Extracts up to four binary comment bytes that typically follow the header line.
    /// </summary>
    /// <param name="bytes">Raw PDF bytes.</param>
    /// <param name="headerOffset">The detected header offset.</param>
    /// <returns>A tuple containing up to four bytes; zero values when unavailable.</returns>
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

    /// <summary>
    /// Attempts to parse the last declared startxref numeric offset.
    /// </summary>
    /// <param name="text">ASCII-decoded PDF content.</param>
    /// <param name="offset">The parsed offset when successful; otherwise -1.</param>
    /// <returns><see langword="true"/> when a numeric startxref offset is found; otherwise <see langword="false"/>.</returns>
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

    /// <summary>
    /// Computes the non-whitespace data length after the final EOF marker.
    /// </summary>
    /// <param name="text">ASCII-decoded PDF content.</param>
    /// <param name="eofIndex">The index of the final EOF marker.</param>
    /// <returns>The count of non-trimmed trailing characters after EOF.</returns>
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
