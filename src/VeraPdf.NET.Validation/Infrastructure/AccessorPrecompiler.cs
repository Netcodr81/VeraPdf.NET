namespace VeraPdf.NET.Validation.Infrastructure;

public static class AccessorPrecompiler
{
    /// <summary>
    /// Pre-compiles accessors for known rules.
    /// Avoids first-run latency.
    /// </summary>
    public static void Precompile(IEnumerable<string> propertyPaths, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            foreach (var path in propertyPaths)
            {
                PropertyAccessorCache.GetAccessor(type, path);
            }
        }
    }
}
