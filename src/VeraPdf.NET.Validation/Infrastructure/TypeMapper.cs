namespace VeraPdf.NET.Validation.Infrastructure;

public static class TypeMapper
{
    /// <summary>
    /// Explicit overrides for known mismatches between veraPDF and .NET model.
    /// Only include cases that cannot be resolved by convention.
    /// </summary>
    private static readonly Dictionary<string, string> ExplicitMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PDAnnotation"] = "PDAnnot",
        ["PDAnnot"] = "PDAnnot",
        ["COSObjectKey"] = "CosObject"
    };

    /// <summary>
    /// Cache resolved types for performance.
    /// </summary>
    private static readonly Dictionary<string, Type> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Entry point: maps veraPDF type name → CLR Type from your Model project.
    /// </summary>
    public static Type MapType(string veraType)
    {
        if (string.IsNullOrWhiteSpace(veraType))
            throw new ArgumentException("Type name cannot be null or empty.");

        veraType = Normalize(veraType);

        if (Cache.TryGetValue(veraType, out var cached))
            return cached;

        if (ExplicitMap.TryGetValue(veraType, out var mappedName))
        {
            var mapped = ResolveClrType(mappedName);
            Cache[veraType] = mapped;
            return mapped;
        }

        foreach (var name in GetCandidateNames(veraType))
        {
            var type = TryResolveClrType(name);
            if (type != null)
            {
                Cache[veraType] = type;
                return type;
            }
        }

        throw new InvalidOperationException($"Unable to map veraPDF type '{veraType}' to a CLR type.");
    }

    private static string Normalize(string typeName)
    {
        var shortName = typeName.Contains('.')
            ? typeName.Split('.').Last()
            : typeName;

        return shortName.Trim();
    }

    private static IEnumerable<string> GetCandidateNames(string veraType)
    {
        yield return veraType;

        if (veraType.StartsWith("I", StringComparison.Ordinal) && veraType.Length > 1)
            yield return veraType[1..];

        if (!veraType.StartsWith("I", StringComparison.Ordinal))
            yield return "I" + veraType;

        if (veraType.StartsWith("COS", StringComparison.Ordinal))
            yield return "Cos" + veraType[3..];

        if (veraType.StartsWith("PD", StringComparison.Ordinal))
            yield return "PD" + veraType[2..];
    }

    private static Type ResolveClrType(string name)
    {
        var type = TryResolveClrType(name);

        if (type == null)
            throw new InvalidOperationException($"CLR type '{name}' not found.");

        return type;
    }

    private static Type? TryResolveClrType(string name)
    {
        var modelAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("VeraPdf.NET.Model", StringComparison.Ordinal) == true);

        if (modelAssembly != null)
        {
            var type = modelAssembly.GetTypes().FirstOrDefault(t => t.Name.Equals(name, StringComparison.Ordinal));
            if (type != null)
                return type;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name.Equals(name, StringComparison.Ordinal));
            if (type != null)
                return type;
        }

        return null;
    }
}