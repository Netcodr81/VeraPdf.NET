using VeraPdf.NET.Validation.Abstractions;

namespace VeraPdf.NET.Validation.Execution;

internal sealed class RuleDispatchTable
{
    /// <summary>
    /// Maps concrete runtime type → applicable rules.
    /// Eliminates repeated IsAssignableFrom checks.
    /// </summary>
    private readonly Dictionary<Type, List<IRule>> _dispatch = new();

    private readonly IReadOnlyList<IRule> _rules;

    public RuleDispatchTable(IEnumerable<IRule> rules)
    {
        _rules = rules.ToList();
    }

    /// <summary>
    /// Gets rules for a specific runtime type.
    /// Cached after first resolution.
    /// </summary>
    public IReadOnlyList<IRule> GetRules(Type runtimeType)
    {
        if (_dispatch.TryGetValue(runtimeType, out var cached))
            return cached;

        // Resolve once
        var matched = _rules
            .Where(r => r.TargetType.IsAssignableFrom(runtimeType))
            .ToList();

        _dispatch[runtimeType] = matched;

        return matched;
    }
}