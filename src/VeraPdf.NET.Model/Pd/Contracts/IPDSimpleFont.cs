namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDSimpleFont : IPDFont
{
    bool IsStandard { get; }

    int? FirstChar { get; }

    int? LastChar { get; }

    int? WidthsSize { get; }

    bool ContainsDifferences { get; }
}
