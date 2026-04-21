namespace VeraPdf.NET.Parser.ModelMapping;

public sealed class ParsedPdfSnapshot
{
    public bool HasHeader { get; init; }

    public bool HasEofMarker { get; init; }

    public bool HasStartXref { get; init; }

    public bool HasXrefKeyword { get; init; }

    public bool XrefSubsectionHeaderSpaceSeparated { get; init; }

    public bool XrefEolMarkersComplyPdfa { get; init; }

    public bool HasCatalogObject { get; init; }

    public bool HasInfoEntry { get; init; }

    public bool HasPieceInfoEntry { get; init; }

    public bool HasEmbeddedFilesEntry { get; init; }

    public bool HasMarkInfoEntry { get; init; }

    public bool MarkedTrue { get; init; }

    public bool SuspectsTrue { get; init; }

    public bool HasViewerPreferencesEntry { get; init; }

    public bool DisplayDocTitleTrue { get; init; }

    public bool HasMetadataEntry { get; init; }

    public bool HasXRefStream { get; init; }

    public bool IsLinearized { get; init; }

    public bool HasTrailer { get; init; }

    public bool TrailerContainsEncrypt { get; init; }

    public bool TrailerContainsInfo { get; init; }

    public int TrailerSize { get; init; }

    public string? TrailerKeysString { get; init; }

    public string? TrailerRawMarkInfo { get; init; }

    public string? TrailerRawViewerPreferences { get; init; }

    public string? FirstId { get; init; }

    public string? LastId { get; init; }

    public string? Header { get; init; }

    public decimal HeaderVersion { get; init; }

    public int HeaderOffset { get; init; }

    public int HeaderByte1 { get; init; }

    public int HeaderByte2 { get; init; }

    public int HeaderByte3 { get; init; }

    public int HeaderByte4 { get; init; }

    public int PostEofDataSize { get; init; }

    public int IndirectObjectCount { get; init; }

    public int StartXrefOffset { get; init; }
}
