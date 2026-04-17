namespace VeraPdf.NET.Validation.Infrastructure;

public static class TypeMapper
{
    /// <summary>
    /// Explicit overrides for known mismatches between veraPDF and .NET model.
    /// Only include cases that cannot be resolved by convention.
    /// </summary>
    private static readonly Dictionary<string, string> ExplicitMap = new()
    {
        // Common mismatches / aliases
        ["PDAnnotation"] = "IPDAnnot",
        ["PDAnnot"] = "IPDAnnot",

        // Sometimes used interchangeably in profiles
        ["COSObjectKey"] = "ICosObject",

        // Add more here ONLY when convention fails
    };

    /// <summary>
    /// Cache resolved types for performance.
    /// </summary>
    private static readonly Dictionary<string, Type> Cache = new();

    /// <summary>
    /// Entry point: maps veraPDF type name → CLR Type from your Model project.
    /// </summary>
    public static Type MapType(string veraType)
    {
        if (string.IsNullOrWhiteSpace(veraType))
            throw new ArgumentException("Type name cannot be null or empty.");

        // Normalize (strip namespace if present)
        veraType = Normalize(veraType);

        // Check cache first
        if (Cache.TryGetValue(veraType, out var cached))
            return cached;

        // Step 1: explicit mapping (edge cases)
        if (ExplicitMap.TryGetValue(veraType, out var mappedName))
        {
            var type = ResolveClrType(mappedName);
            Cache[veraType] = type;
            return type;
        }

        // Step 2: convention mapping
        var candidateNames = GetCandidateNames(veraType);

        foreach (var name in candidateNames)
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

    // ------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------

    /// <summary>
    /// Removes namespace if present (veraPDF sometimes uses FQNs)
    /// </summary>
    private static string Normalize(string typeName)
    {
        return typeName.Contains('.')
            ? typeName.Split('.').Last()
            : typeName;
    }

    /// <summary>
    /// Generates possible interface names based on veraPDF naming conventions.
    /// </summary>
    private static IEnumerable<string> GetCandidateNames(string veraType)
    {
        // Already an interface
        if (veraType.StartsWith("I"))
            yield return veraType;

        // PD → IPD
        if (veraType.StartsWith("PD"))
            yield return "I" + veraType;

        // COS → ICos (note casing!)
        if (veraType.StartsWith("COS"))
            yield return "I" + veraType.Substring(0, 1) + veraType.Substring(1).ToLower();

        // XMP → IXMP
        if (veraType.StartsWith("XMP"))
            yield return "I" + veraType;

        // Generic fallback
        yield return "I" + veraType;
    }

    /// <summary>
    /// Attempts to resolve type from loaded assemblies.
    /// </summary>
    private static Type ResolveClrType(string name)
    {
        var type = TryResolveClrType(name);

        if (type == null)
            throw new InvalidOperationException($"CLR type '{name}' not found.");

        return type;
    }

    /// <summary>
    /// Searches all loaded assemblies for a matching type name.
    /// Restricts to your Model assembly for performance.
    /// </summary>
    private static Type? TryResolveClrType(string name)
    {
        // Prefer your model assembly first
        var modelAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name?.StartsWith("VeraPdf.NET.Model") == true);

        if (modelAssembly != null)
        {
            var type = modelAssembly.GetTypes()
                .FirstOrDefault(t => t.Name == name);

            if (type != null)
                return type;
        }

        // Fallback: scan all assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == name);

            if (type != null)
                return type;
        }

        return null;
    }
}