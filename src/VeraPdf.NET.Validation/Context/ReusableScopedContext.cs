using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Infrastructure;

namespace VeraPdf.NET.Validation.Context;

/// <summary>
/// A reusable, allocation-free scoped context used inside ANY/ALL.
/// 
/// Instead of allocating a new context per item, this instance is:
/// - created once per predicate
/// - reused by updating Current
/// 
/// This removes a major GC hotspot.
/// </summary>
public sealed class ReusableScopedContext : IValidationContext
{
    private readonly IValidationContext _parent;

    /// <summary>
    /// Current object being evaluated (changes per iteration)
    /// </summary>
    public object? Current { get; private set; }

    public ReusableScopedContext(IValidationContext parent)
    {
        _parent = parent;
    }

    /// <summary>
    /// Updates the current object for the next iteration.
    /// This replaces object allocation.
    /// </summary>
    public void SetCurrent(object? current)
    {
        Current = current;
    }

    /// <summary>
    /// Resolves property with veraPDF semantics:
    /// 1. Check current object
    /// 2. Fallback to parent context
    /// </summary>
    public object? GetProperty(string path)
    {
        if (Current != null)
        {
            var accessor = PropertyAccessorCache.GetAccessor(Current.GetType(), path);
            var value = accessor(Current);

            if (value != null)
                return value;
        }

        return _parent.GetProperty(path);
    }
}