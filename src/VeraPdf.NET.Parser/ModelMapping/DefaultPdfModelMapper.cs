using VeraPdf.NET.Model;
using VeraPdf.NET.Parser.Abstractions;

namespace VeraPdf.NET.Parser.ModelMapping;

/// <summary>
/// Provides the default mapping from parser snapshots to COS document models.
/// </summary>
public sealed class DefaultPdfModelMapper : IPdfModelMapper
{
    /// <summary>
    /// Converts a parsed snapshot into a populated COS document.
    /// </summary>
    /// <param name="snapshot">The parsed PDF snapshot.</param>
    /// <returns>A COS document mapped from snapshot values.</returns>
    public CosDocument MapToCosDocument(ParsedPdfSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        return new CosDocument
        {
            Header = snapshot.Header,
            HeaderOffset = snapshot.HeaderOffset,
            HeaderVersion = snapshot.HeaderVersion,
            HeaderByte1 = snapshot.HeaderByte1,
            HeaderByte2 = snapshot.HeaderByte2,
            HeaderByte3 = snapshot.HeaderByte3,
            HeaderByte4 = snapshot.HeaderByte4,
            PostEOFDataSize = snapshot.PostEofDataSize,
            NrIndirects = snapshot.IndirectObjectCount,
            IsLinearized = snapshot.IsLinearized,
            ContainsInfo = snapshot.HasInfoEntry,
            ContainsPieceInfo = snapshot.HasPieceInfoEntry,
            ContainsEmbeddedFiles = snapshot.HasEmbeddedFilesEntry,
            Marked = snapshot.MarkedTrue,
            Suspects = snapshot.SuspectsTrue,
            DisplayDocTitle = snapshot.DisplayDocTitleTrue,
            Document = new PDDocument
            {
                ContainsMetadata = snapshot.HasMetadataEntry,
                ContainsXRefStream = snapshot.HasXRefStream
            }
        };
    }
}
