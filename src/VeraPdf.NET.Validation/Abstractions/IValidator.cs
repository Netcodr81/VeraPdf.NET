
using VeraPdf.NET.Validation.Results;

namespace VeraPdf.NET.Validation.Abstractions;

public interface IValidator
{
    /// <summary>
    /// Executes validation against the provided root object (PDF model root).
    /// Equivalent to veraPDF Validator.validate().
    /// </summary>
    ValidationResult Validate(object root);
}
