namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Represents a validation error captured during precheck, runtime provisioning, or standard execution.
/// </summary>
public sealed class ValidationError
{
    /// <summary>
    /// Gets the categorized error code.
    /// </summary>
    public required ValidationErrorCode Code { get; init; }

    /// <summary>
    /// Gets the human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the optional target associated with the error (for example, a field or subsystem).
    /// </summary>
    public string? Target { get; init; }

    /// <summary>
    /// Gets a human-readable description of the error code.
    /// </summary>
    public string CodeDescription => Code switch
    {
        ValidationErrorCode.None => "No validation error occurred.",
        ValidationErrorCode.InvalidInput => "Input data or options were invalid.",
        ValidationErrorCode.InvalidPdf => "The document failed parser precheck and is not a structurally valid PDF for external validation.",
        ValidationErrorCode.RuntimeUnavailable => "veraPDF runtime dependencies are unavailable or could not be provisioned.",
        ValidationErrorCode.ProcessTimeout => "The external veraPDF process exceeded the configured timeout.",
        ValidationErrorCode.ProcessFailed => "The external veraPDF process exited with a failure code.",
        ValidationErrorCode.InternalError => "An unexpected internal validation error occurred.",
        _ => "Unknown validation error."
    };
}
