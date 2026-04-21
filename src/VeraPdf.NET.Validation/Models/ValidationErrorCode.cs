namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Enumerates known error conditions that can occur during validation.
/// </summary>
public enum ValidationErrorCode
{
    /// <summary>
    /// No error occurred.
    /// </summary>
    None = 0,
    /// <summary>
    /// Input data or options were invalid.
    /// </summary>
    InvalidInput = 1,
    /// <summary>
    /// The PDF failed structural parsing prechecks.
    /// </summary>
    InvalidPdf = 2,
    /// <summary>
    /// Runtime dependencies could not be provisioned.
    /// </summary>
    RuntimeUnavailable = 3,
    /// <summary>
    /// The external validation process timed out.
    /// </summary>
    ProcessTimeout = 4,
    /// <summary>
    /// The external validation process failed with a non-zero exit code.
    /// </summary>
    ProcessFailed = 5,
    /// <summary>
    /// An unexpected internal error occurred.
    /// </summary>
    InternalError = 6
}
