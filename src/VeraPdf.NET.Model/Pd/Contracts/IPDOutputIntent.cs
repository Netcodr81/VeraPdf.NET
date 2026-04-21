namespace VeraPdf.NET.Model.Pd.Contracts;

public interface IPDOutputIntent
{
    string? DestOutputProfileIndirect { get; }

    bool ContainsDestOutputProfileRef { get; }

    string? OutputConditionIdentifier { get; }

    string? S { get; }

    string? ICCProfileMD5 { get; }
}
