using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace VeraPdf.NET.Validation.Infrastructure;

internal static class PropertyAccessorCache
{
    /// <summary>
    /// Cache of compiled property accessors.
    /// Key = (Type, Path)
    ///
    /// Eliminates reflection from validation execution path.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type, string), Func<object, object?>> _cache = new();

    /// <summary>
    /// Returns cached accessor or builds it once.
    /// Thread-safe and lock-free for reads.
    /// </summary>
    public static Func<object, object?> GetAccessor(Type type, string path)
    {
        return _cache.GetOrAdd((type, path), key => BuildAccessor(key.Item1, key.Item2));
    }

    /// <summary>
    /// Builds a compiled delegate:
    /// object → object?
    ///
    /// Supports:
    /// - nested paths (a.b.c)
    /// - interface properties
    /// - null-safe navigation
    /// </summary>
    private static Func<object, object?> BuildAccessor(Type type, string path)
    {
        var param = Expression.Parameter(typeof(object), "obj");
        Expression current = Expression.Convert(param, type);

        foreach (var part in path.Split('.'))
        {
            // Support interface + concrete properties
            var prop = current.Type.GetProperty(part)
                ?? current.Type.GetInterfaces()
                    .SelectMany(i => i.GetProperties())
                    .FirstOrDefault(p => p.Name == part);

            if (prop == null)
                return _ => null;

            var next = Expression.Property(current, prop);

            // Null-safe navigation
            current = Expression.Condition(
                Expression.Equal(current, Expression.Constant(null)),
                Expression.Constant(null, typeof(object)),
                Expression.Convert(next, typeof(object))
            );
        }

        return Expression.Lambda<Func<object, object?>>(current, param).Compile();
    }
}