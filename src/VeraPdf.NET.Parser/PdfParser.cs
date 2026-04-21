using VeraPdf.NET.Parser.Abstractions;
using VeraPdf.NET.Parser.Internal;
using VeraPdf.NET.Parser.ModelMapping;
using VeraPdf.NET.Parser.Results;

namespace VeraPdf.NET.Parser;

public sealed class PdfParser : IPdfParser
{
    private readonly IPdfModelMapper _modelMapper;

    public PdfParser()
        : this(new DefaultPdfModelMapper())
    {
    }

    public PdfParser(IPdfModelMapper modelMapper)
    {
        _modelMapper = modelMapper ?? throw new ArgumentNullException(nameof(modelMapper));
    }

    public PdfParseResult Parse(Stream pdfStream, PdfParseOptions? options = null)
    {
        if (pdfStream == null)
            throw new ArgumentNullException(nameof(pdfStream));

        using var memory = new MemoryStream();
        pdfStream.CopyTo(memory);

        return Parse(memory.ToArray(), options);
    }

    public PdfParseResult Parse(byte[] pdfBytes, PdfParseOptions? options = null)
    {
        options ??= new PdfParseOptions();

        if (pdfBytes == null)
            throw new ArgumentNullException(nameof(pdfBytes));

        var diagnostics = new List<ParseDiagnostic>();
        var snapshot = PdfStructureScanner.Scan(pdfBytes, options, diagnostics);

        var hasErrors = diagnostics.Any(d => d.Severity == ParseDiagnosticSeverity.Error);

        if (hasErrors)
        {
            return new PdfParseResult
            {
                Success = false,
                Diagnostics = options.CaptureDiagnostics ? diagnostics : []
            };
        }

        var document = _modelMapper.MapToCosDocument(snapshot);

        return new PdfParseResult
        {
            Success = true,
            Document = document,
            Diagnostics = options.CaptureDiagnostics ? diagnostics : []
        };
    }

    public PdfParseResult ParseFile(string absolutePath, PdfParseOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
            throw new ArgumentException("Absolute path is required.", nameof(absolutePath));

        if (!Path.IsPathRooted(absolutePath))
            throw new ArgumentException("File path must be absolute.", nameof(absolutePath));

        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("PDF file was not found.", absolutePath);

        var bytes = File.ReadAllBytes(absolutePath);

        return Parse(bytes, options);
    }

    public async Task<PdfParseResult> ParseAsync(
        Stream pdfStream,
        PdfParseOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (pdfStream == null)
            throw new ArgumentNullException(nameof(pdfStream));

        using var memory = new MemoryStream();
        await pdfStream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);

        return Parse(memory.ToArray(), options);
    }
}
