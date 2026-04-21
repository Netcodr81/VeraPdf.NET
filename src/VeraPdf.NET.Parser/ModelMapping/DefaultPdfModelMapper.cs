using VeraPdf.NET.Model;
using VeraPdf.NET.Parser.Abstractions;

namespace VeraPdf.NET.Parser.ModelMapping;

public sealed class DefaultPdfModelMapper : IPdfModelMapper
{
    public CosDocument MapToCosDocument(ParsedPdfSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        var trailer = snapshot.HasTrailer
            ? new CosTrailer
            {
                IsEncrypted = snapshot.TrailerContainsEncrypt,
                Info = snapshot.TrailerContainsInfo ? new CosInfo() : null,
                Size = snapshot.TrailerSize,
                KeysString = snapshot.TrailerKeysString
            }
            : null;

        var xref = snapshot.HasXrefKeyword || snapshot.HasXRefStream
            ? new CosXRef
            {
                SubsectionHeaderSpaceSeparated = snapshot.XrefSubsectionHeaderSpaceSeparated,
                XrefEOLMarkersComplyPDFA = snapshot.XrefEolMarkersComplyPdfa
            }
            : null;

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
            FirstPageID = snapshot.FirstId,
            LastID = snapshot.LastId,
            FirstPageIDValue = snapshot.FirstId,
            LastIDValue = snapshot.LastId,
            MarkInfo = snapshot.TrailerRawMarkInfo,
            ViewerPreferences = snapshot.TrailerRawViewerPreferences,
            Trailer = trailer,
            XRef = xref,
            Document = new PDDocument
            {
                ContainsMetadata = snapshot.HasMetadataEntry,
                ContainsXRefStream = snapshot.HasXRefStream
            }
        };
    }
}
