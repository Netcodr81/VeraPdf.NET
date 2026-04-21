namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISAStructElem : ISAObject
{
    string? StandardType { get; }

    bool IsLeafElem { get; }

    bool HasLowestDepthError { get; }

    int Page { get; }

    int LastPage { get; }

    int ChildrenCount { get; }
}
