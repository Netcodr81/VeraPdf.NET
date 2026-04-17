namespace VeraPdf.NET.Validation.Abstractions;

public interface IValidationContext
{
    /// <summary>
    /// Current object under validation.
    /// </summary>
    object Current { get; }

    /// <summary>
    /// Resolves a property path (e.g. "resources.fonts.size").
    /// Equivalent to veraPDF property resolution.
    /// </summary>
    object? GetProperty(string path);
}