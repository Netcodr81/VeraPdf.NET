using System.Collections;

namespace VeraPdf.NET.Validation.Compilation.Linq;

internal static class FunctionImplementations
{
    /// <summary>
    /// exists(x)
    /// True if value is not null and not empty (for collections)
    /// </summary>
    public static bool Exists(object? value)
    {
        if (value == null)
            return false;

        if (value is IEnumerable e)
            return e.Cast<object>().Any();

        return true;
    }

    /// <summary>
    /// isDefined(x)
    /// veraPDF semantics: property exists AND is not null
    /// </summary>
    public static bool IsDefined(object? value)
    {
        return value != null;
    }

    /// <summary>
    /// hasType(obj, "TypeName")
    /// Checks runtime type name (used heavily in veraPDF)
    /// </summary>
    public static bool HasType(object? obj, string typeName)
    {
        if (obj == null) return false;

        return obj.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// hasRole(structElem, "H1")
    /// Used for tagged PDF validation
    /// </summary>
    public static bool HasRole(object? obj, string role)
    {
        if (obj == null) return false;

        var prop = obj.GetType().GetProperty("Role");
        var value = prop?.GetValue(obj)?.ToString();

        return string.Equals(value, role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// isStandardFont(font)
    /// Simplified check — extend with full font list later
    /// </summary>
    public static bool IsStandardFont(object? font)
    {
        if (font == null) return false;

        var nameProp = font.GetType().GetProperty("BaseFont");
        var name = nameProp?.GetValue(font)?.ToString();

        if (name == null) return false;

        return StandardFonts.Contains(name);
    }

    private static readonly HashSet<string> StandardFonts = new()
    {
        "Helvetica",
        "Times-Roman",
        "Courier",
        "Symbol",
        "ZapfDingbats"
    };
}