namespace VeraPdf.NET.Validation.Abstractions;

public interface IObjectWalker
{
    /// <summary>
    /// Traverses the full object graph.
    /// Equivalent to veraPDF model traversal.
    /// </summary>
    IEnumerable<object> Traverse(object root);
}
