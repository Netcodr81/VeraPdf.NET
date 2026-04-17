using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Infrastructure;

namespace VeraPdf.NET.Validation.Context;

public sealed class CompiledValidationContext : IValidationContext
{
    public object Current { get; }

    public CompiledValidationContext(object current)
    {
        Current = current;
    }

    /// <summary>
    /// Resolves property path using compiled accessor.
    ///
    /// This is the key to zero-reflection validation.
    /// </summary>
    public object? GetProperty(string path)
    {
        if (Current == null) return null;

        var accessor = PropertyAccessorCache.GetAccessor(Current.GetType(), path);
        return accessor(Current);
    }
}