namespace VeraPdf.NET.Model.Contracts.Semantic;

public interface ISETable
{
    bool UseHeadersAndIdOrScope { get; }

    int ColumnSpan { get; }

    int RowSpan { get; }

    int NumberOfRowWithWrongColumnSpan { get; }

    int NumberOfColumnWithWrongRowSpan { get; }
}
