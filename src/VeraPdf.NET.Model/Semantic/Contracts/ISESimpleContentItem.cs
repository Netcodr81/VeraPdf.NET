namespace VeraPdf.NET.Model.Contracts.Semantic;

public interface ISESimpleContentItem : ISEContentItem
{
    string? ItemType { get; }

    bool IsFormula { get; }
}
