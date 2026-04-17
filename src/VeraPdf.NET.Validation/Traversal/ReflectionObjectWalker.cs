using System.Collections;
using VeraPdf.NET.Validation.Abstractions;

namespace VeraPdf.NET.Validation.Traversal;

public sealed class ReflectionObjectWalker : IObjectWalker
{
    /// <summary>
    /// Traverses object graph depth-first.
    /// Equivalent to veraPDF model traversal.
    /// </summary>
    public IEnumerable<object> Traverse(object root)
    {
        var visited = new HashSet<object>();
        var stack = new Stack<object>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (!visited.Add(current))
                continue;

            yield return current;

            if (IsLeaf(current.GetType()))
                continue;

            foreach (var prop in current.GetType().GetProperties())
            {
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                    continue;

                object? value;
                try
                {
                    value = prop.GetValue(current);
                }
                catch
                {
                    continue;
                }

                if (value == null)
                    continue;

                if (value is string)
                    continue;

                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                            stack.Push(item);
                    }

                    continue;
                }

                stack.Push(value);
            }
        }
    }

    private static bool IsLeaf(Type type)
    {
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(TimeSpan)
            || type == typeof(Guid);
    }
}