namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDFormField
{
    string? FT { get; }

    bool ContainsAA { get; }

    bool ContainsLang { get; }

    int KidsCount { get; }
}
