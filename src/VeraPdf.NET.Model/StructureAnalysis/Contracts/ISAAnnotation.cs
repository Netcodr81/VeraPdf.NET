namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISAAnnotation
{
    int F { get; }

    string? Contents { get; }

    string? Alt { get; }

    bool IsOutsideCropBox { get; }
}
