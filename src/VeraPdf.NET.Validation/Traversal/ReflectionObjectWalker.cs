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

            foreach (var prop in current.GetType().GetProperties())
            {
                var value = prop.GetValue(current);

                if (value is IEnumerable<object> list)
                {
                    foreach (var item in list)
                        stack.Push(item);
                }
                else if (value != null)
                {
                    stack.Push(value);
                }
            }
        }
    }
}