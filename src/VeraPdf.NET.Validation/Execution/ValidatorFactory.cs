using VeraPdf.NET.Validation.Abstractions;

namespace VeraPdf.NET.Validation.Execution;

/// <summary>
/// Factory responsible for constructing a fully configured Validator.
/// This isolates internal implementation details from consumers.
/// </summary>
public static class ValidatorFactory
{
    public static IValidator Create(
        IEnumerable<IRule> rules,
        IObjectWalker walker,
        ValidationOptions? options = null)
    {
        options ??= new ValidationOptions();

        // Future: apply options (precompile, logging, etc.)
        if (options.PrecompileAccessors)
        {
            // Hook for warming PropertyAccessorCache
        }

        return new Validator(rules, walker);
    }
}