namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISATextChunk
{
    decimal TextSize { get; }

    decimal ContrastRatio { get; }

    decimal TextWeight { get; }

    bool HasSpecialStyle { get; }

    bool HasSpecialBackground { get; }

    bool IsUnderlined { get; }

    bool IsWhiteSpaceChunk { get; }
}
