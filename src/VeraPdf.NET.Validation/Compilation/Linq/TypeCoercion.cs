namespace VeraPdf.NET.Validation.Compilation.Linq;

public static class TypeCoercion
{
    /// <summary>
    /// Coerces two values into comparable types.
    /// Matches veraPDF's loose typing behavior.
    /// </summary>
    public static (object? left, object? right) Coerce(object? left, object? right)
    {
        if (left == null || right == null)
            return (left, right);

        if (IsNumber(left) || IsNumber(right))
            return (ToDouble(left), ToDouble(right));

        if (left is bool || right is bool)
            return (ToBool(left), ToBool(right));

        return (left.ToString(), right.ToString());
    }

    /// <summary>
    /// Converts value to boolean using veraPDF semantics.
    /// </summary>
    public static bool ToBool(object? o)
    {
        return o switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var v) => v,
            _ => false
        };
    }

    private static bool IsNumber(object o) =>
        o is int or long or float or double or decimal;

    private static double ToDouble(object o)
    {
        return o switch
        {
            double d => d,
            int i => i,
            long l => l,
            string s when double.TryParse(s, out var v) => v,
            _ => 0
        };
    }
}