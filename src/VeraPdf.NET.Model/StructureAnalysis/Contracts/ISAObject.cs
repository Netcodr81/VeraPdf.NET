namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISAObject
{
    string? StructureID { get; }

    int ErrorCodesCount { get; }

    int ErrorArgumentsCount { get; }
}
