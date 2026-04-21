namespace VeraPdf.NET.Model.Contracts.Semantic;

public interface ISEContentItem
{
    bool IsTaggedContent { get; }

    bool IsSignature { get; }

    bool IsArtifact { get; }

    string? ParentStructureTag { get; }

    string? ParentStandardTag { get; }

    int ParentsTagsCount { get; }
}
