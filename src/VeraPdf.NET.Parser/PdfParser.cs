using VeraPdf.NET.Parser.Abstractions;
using VeraPdf.NET.Parser.Internal;
using VeraPdf.NET.Parser.ModelMapping;
using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Parser;

/// <summary>
/// Parses PDF bytes or streams into a COS document model and parse diagnostics.
/// </summary>
public sealed class PdfParser
{
    private readonly IPdfModelMapper _modelMapper;

    /// <summary>
    /// Initializes a parser with the default PDF model mapper.
    /// </summary>
    public PdfParser()
        : this(new DefaultPdfModelMapper())
    {
    }

    /// <summary>
    /// Initializes a parser with a custom model mapper.
    /// </summary>
    /// <param name="modelMapper">The mapper used to convert parser snapshots into COS models.</param>
    public PdfParser(IPdfModelMapper modelMapper)
    {
        _modelMapper = modelMapper ?? throw new ArgumentNullException(nameof(modelMapper));
    }

    /// <summary>
    /// Parses PDF content from a byte array.
    /// </summary>
    /// <param name="pdfBytes">The PDF content bytes.</param>
    /// <param name="options">Optional parser behavior settings.</param>
    /// <returns>The parse result containing diagnostics and a mapped document when successful.</returns>
    public PdfParseResult Parse(byte[] pdfBytes, PdfParseOptions? options = null)
    {
        if (pdfBytes is null)
            throw new ArgumentNullException(nameof(pdfBytes));

        options ??= new PdfParseOptions();

        var diagnostics = new List<ParseDiagnostic>();
        var snapshot = PdfStructureScanner.Scan(pdfBytes, options, diagnostics);
        var hasErrors = diagnostics.Any(d => d.Severity == ParseDiagnosticSeverity.Error);

        return new PdfParseResult
        {
            Success = !hasErrors,
            Document = hasErrors ? null : _modelMapper.MapToCosDocument(snapshot),
            Diagnostics = diagnostics
        };
    }

    /// <summary>
    /// Parses PDF content from a stream.
    /// </summary>
    /// <param name="stream">The stream containing PDF content.</param>
    /// <param name="options">Optional parser behavior settings.</param>
    /// <param name="cancellationToken">A token used to cancel asynchronous reading.</param>
    /// <returns>The parse result containing diagnostics and a mapped document when successful.</returns>
    public async Task<PdfParseResult> ParseAsync(Stream stream, PdfParseOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (stream is null)
            throw new ArgumentNullException(nameof(stream));

        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        return Parse(memoryStream.ToArray(), options);
    }

    /// <summary>
    /// Parses PDF content from a file path.
    /// </summary>
    /// <param name="absoluteFilePath">The absolute path to the PDF file.</param>
    /// <param name="options">Optional parser behavior settings.</param>
    /// <returns>The parse result containing diagnostics and a mapped document when successful.</returns>
    public PdfParseResult ParseFile(string absoluteFilePath, PdfParseOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(absoluteFilePath))
            throw new ArgumentException("File path is required.", nameof(absoluteFilePath));

        var bytes = File.ReadAllBytes(absoluteFilePath);
        return Parse(bytes, options);
    }
}
