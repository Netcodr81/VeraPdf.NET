using VeraPdf.Parser.Cos;
using VeraPdf.Parser.Filters;

namespace VeraPdf.Parser.Pd;

public sealed class PdfContentStream
{
    private readonly CosStream _stream;

    public PdfContentStream(CosStream stream)
    {
        _stream = stream;
    }

    public byte[] GetDecodedData(PdfStreamDecodeOptions? options = null) =>
        new PdfStreamFilterService(options).Decode(_stream);
}
