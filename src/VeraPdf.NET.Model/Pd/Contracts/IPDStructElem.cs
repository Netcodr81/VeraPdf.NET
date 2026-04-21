namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDStructElem
{
    string? StandardType { get; }

    bool ContainsLang { get; }

    bool ContainsParent { get; }

    bool IsGrouping { get; }
}
