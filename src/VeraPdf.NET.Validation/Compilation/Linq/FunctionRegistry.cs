using System.Reflection;

namespace VeraPdf.NET.Validation.Compilation.Linq;

internal static class FunctionRegistry
{
    /// <summary>
    /// Maps DSL function name → implementation method
    /// </summary>
    public static readonly Dictionary<string, MethodInfo> Functions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["exists"] = Get(nameof(FunctionImplementations.Exists)),
            ["isDefined"] = Get(nameof(FunctionImplementations.IsDefined)),
            ["hasType"] = Get(nameof(FunctionImplementations.HasType)),
            ["hasRole"] = Get(nameof(FunctionImplementations.HasRole)),
            ["isStandardFont"] = Get(nameof(FunctionImplementations.IsStandardFont))
        };

    private static MethodInfo Get(string name) =>
        typeof(FunctionImplementations).GetMethod(name)!;
}