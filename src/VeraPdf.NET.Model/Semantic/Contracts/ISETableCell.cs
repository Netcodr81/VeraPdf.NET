namespace VeraPdf.NET.Model.Contracts.Semantic;

public interface ISETableCell
{
    int ColSpan { get; }

    int RowSpan { get; }

    bool HasIntersection { get; }
}
