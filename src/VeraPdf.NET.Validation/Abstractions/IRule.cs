namespace VeraPdf.NET.Validation.Abstractions;

public interface IRule
{
    string Id { get; }

    /// <summary>
    /// The type this rule targets.
    /// Used for rule dispatch.
    /// </summary>
    Type TargetType { get; }

    bool Evaluate(IValidationContext context);
}