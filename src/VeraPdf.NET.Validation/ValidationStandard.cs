namespace VeraPdf.NET.Validation;

/// <summary>
/// Defines supported validation standards that can be combined for a single validation run.
/// </summary>
[Flags]
public enum ValidationStandard
{
    /// <summary>
    /// No validation standard selected.
    /// </summary>
    None = 0,
    /// <summary>
    /// PDF/A conformance validation.
    /// </summary>
    PdfA = 1,
    /// <summary>
    /// PDF/UA accessibility conformance validation.
    /// </summary>
    PdfUa = 2,
    /// <summary>
    /// WCAG 2.2 validation profile.
    /// </summary>
    Wcag22 = 4,
    /// <summary>
    /// All supported validation standards.
    /// </summary>
    All = PdfA | PdfUa | Wcag22
}
