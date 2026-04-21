namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISAPage
{
    int AnnotsCount { get; }

    int TableBordersCount { get; }
}
