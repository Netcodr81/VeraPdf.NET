using System.Collections;

namespace VeraPdf.NET.Validation.Compilation.Linq;

internal static class CollectionHelpers
{
    /// <summary>
    /// Converts any IEnumerable into IEnumerable<object>.
    ///
    /// Why this exists:
    /// - Model uses strongly typed collections (e.g. IReadOnlyList<IPDPage>)
    /// - LINQ Any/All requires consistent typing
    /// - Avoids invalid casts at runtime
    /// </summary>
    public static IEnumerable<object> ToObjectEnumerable(object? input)
    {
        if (input is IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                yield return item!;
        }
    }
}