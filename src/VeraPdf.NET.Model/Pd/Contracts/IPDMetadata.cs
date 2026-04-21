namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDMetadata
{
    string? Filter { get; }

    bool IsCatalogMetadata { get; }
}
