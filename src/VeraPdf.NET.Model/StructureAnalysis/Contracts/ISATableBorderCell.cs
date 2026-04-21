namespace VeraPdf.NET.Model.Contracts.StructureAnalysis;

public interface ISATableBorderCell : ISAObject
{
    int ColSpan { get; }

    int RowSpan { get; }
}
