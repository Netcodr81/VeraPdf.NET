using VeraPdf.NET.Validation.Abstractions;

namespace VeraPdf.NET.Validation.Rules;

public sealed class CompiledRule : IRule
{
    private readonly Func<IValidationContext, bool> _func;

    public string Id { get; }

    /// <summary>
    /// Strongly-typed target used for dispatch.
    /// </summary>
    public Type TargetType { get; }

    public CompiledRule(string id, Type targetType, Func<IValidationContext, bool> func)
    {
        Id = id;
        TargetType = targetType;
        _func = func;
    }

    /// <summary>
    /// Executes compiled rule delegate.
    /// </summary>
    public bool Evaluate(IValidationContext context) => _func(context);
}