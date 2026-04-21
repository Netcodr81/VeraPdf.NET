namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDAcroForm
{
    bool NeedAppearances { get; }

    bool ContainsXFA { get; }

    string? DynamicRender { get; }

    int FormFieldsCount { get; }
}
