using VeraPdf.NET.Model;
using VeraPdf.NET.Parser.ModelMapping;

namespace VeraPdf.NET.Parser.Abstractions;

/// <summary>
/// Maps parsed PDF snapshots into COS document models.
/// </summary>
public interface IPdfModelMapper
{
    /// <summary>
    /// Converts a parsed snapshot into a COS document representation.
    /// </summary>
    /// <param name="snapshot">The parsed snapshot extracted from PDF content.</param>
    /// <returns>The mapped COS document.</returns>
    CosDocument MapToCosDocument(ParsedPdfSnapshot snapshot);
}
