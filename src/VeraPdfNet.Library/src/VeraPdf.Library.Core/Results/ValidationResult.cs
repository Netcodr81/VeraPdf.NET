using System.Collections.ObjectModel;
using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Results;

public enum JobEndStatus
{
    Normal,
    Timeout,
    Cancelled
}

/// <summary>
/// A result supplied by a validator. Reported assertions may be only a subset
/// of the checks performed, so compliance cannot be inferred from this list.
/// </summary>
public sealed class ValidationResult
{
    public ValidationResult(
        ValidationProfile profile, IEnumerable<TestAssertion> assertions, bool isCompliant,
        int totalAssertions, JobEndStatus endStatus, IReadOnlyDictionary<RuleId, int>? failedChecks = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(assertions);
        if (totalAssertions < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAssertions));
        if (!Enum.IsDefined(endStatus))
            throw new ArgumentOutOfRangeException(nameof(endStatus));
        Profile = profile;
        Assertions = Array.AsReadOnly(assertions.ToArray());
        IsCompliant = isCompliant;
        TotalAssertions = totalAssertions;
        EndStatus = endStatus;
        FailedChecks = failedChecks is null
            ? null
            : new ReadOnlyDictionary<RuleId, int>(new Dictionary<RuleId, int>(failedChecks));
    }

    public ValidationProfile Profile { get; }
    public string? Flavour => Profile.Flavour;
    public ProfileDetails? ProfileDetails => Profile.Details;
    public IReadOnlyList<TestAssertion> Assertions { get; }
    public bool IsCompliant { get; }
    public int TotalAssertions { get; }
    public JobEndStatus EndStatus { get; }
    public IReadOnlyDictionary<RuleId, int>? FailedChecks { get; }

    /// <summary>Removes passed assertions while preserving aggregate validation data.</summary>
    public ValidationResult WithoutPassedAssertions() =>
        new(Profile, Assertions.Where(assertion => assertion.Status != AssertionStatus.Passed),
            IsCompliant, TotalAssertions, EndStatus, FailedChecks);
}
