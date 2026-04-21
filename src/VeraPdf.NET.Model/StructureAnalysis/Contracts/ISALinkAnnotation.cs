namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISALinkAnnotation : ISAAnnotation
{
    bool TextValueIsLink { get; }

    bool ContentsIsLink { get; }

    bool AltIsLink { get; }
}
