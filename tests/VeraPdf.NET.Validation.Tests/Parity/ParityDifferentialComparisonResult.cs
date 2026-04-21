using VeraPdf.NET.Validation.Reporting;

namespace VeraPdf.NET.Validation.Tests.Parity;

public sealed class ParityDifferentialComparisonResult
{
    public bool IsMatch { get; init; }

    public IReadOnlyList<string> Differences { get; init; } = [];

    public NormalizedValidationOutput? Expected { get; init; }

    public NormalizedValidationOutput? Actual { get; init; }
}

public sealed class ParityDifferentialManifestResult
{
    public int TotalCases { get; init; }

    public int MatchedCases { get; init; }

    public IReadOnlyDictionary<string, ParityDifferentialComparisonResult> CaseResults { get; init; } =
        new Dictionary<string, ParityDifferentialComparisonResult>();
}
