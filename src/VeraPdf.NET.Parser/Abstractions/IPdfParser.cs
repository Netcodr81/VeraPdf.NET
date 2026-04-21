using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Parser.Abstractions;

public interface IPdfParser
{
    PdfParseResult Parse(Stream pdfStream, PdfParseOptions? options = null);

    PdfParseResult Parse(byte[] pdfBytes, PdfParseOptions? options = null);

    PdfParseResult ParseFile(string absolutePath, PdfParseOptions? options = null);

    Task<PdfParseResult> ParseAsync(Stream pdfStream, PdfParseOptions? options = null, CancellationToken cancellationToken = default);
}
