using VeraPdf.Library.Profiles;

namespace VeraPdf.Library.Results;

public enum AssertionStatus
{
    Passed,
    Failed,
    Unknown
}

public sealed record EvaluatedErrorArgument(string Expression, string? Name, string? Value);

/// <summary>A single rule check. Constructing this object does not validate a document.</summary>
public sealed class TestAssertion
{
    public TestAssertion(
        int ordinal, RuleId ruleId, AssertionStatus status, string? message, Location? location,
        string? locationContext, string? errorMessage, IEnumerable<EvaluatedErrorArgument>? errorArguments)
    {
        ArgumentNullException.ThrowIfNull(ruleId);
        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));
        Ordinal = ordinal;
        RuleId = ruleId;
        Status = status;
        Message = message;
        Location = location;
        LocationContext = locationContext;
        ErrorMessage = errorMessage;
        ErrorArguments = errorArguments is null ? null : Array.AsReadOnly(errorArguments.ToArray());
    }

    public int Ordinal { get; }
    public RuleId RuleId { get; }
    public AssertionStatus Status { get; }
    public string? Message { get; }
    public Location? Location { get; }
    public string? LocationContext { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<EvaluatedErrorArgument>? ErrorArguments { get; }
}
