using System.Text;
using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosStreamFilterReferenceTests
{
    [Fact]
    public void ResolvesIndirectFilterAndDecodeParameters()
    {
        var prefix = "%PDF-1.7\n";
        var streamObject =
            "1 0 obj\n<< /Length 1 /Filter 2 0 R /DecodeParms 3 0 R /F 4 0 R /FFilter 2 0 R /FDecodeParms 3 0 R >>\nstream\nx\nendstream\nendobj\n";
        var filterObject = "2 0 obj\n/Crypt\nendobj\n";
        var parametersObject = "3 0 obj\n<< /Name /Identity >>\nendobj\n";
        var fileObject = "4 0 obj\n(external)\nendobj\n";
        var streamOffset = Encoding.ASCII.GetByteCount(prefix);
        var filterOffset = streamOffset + Encoding.ASCII.GetByteCount(streamObject);
        var parametersOffset = filterOffset + Encoding.ASCII.GetByteCount(filterObject);
        var fileOffset = parametersOffset + Encoding.ASCII.GetByteCount(parametersObject);
        var xrefOffset = fileOffset + Encoding.ASCII.GetByteCount(fileObject);
        var pdf = prefix +
            streamObject +
            filterObject +
            parametersObject +
            fileObject +
            "xref\n0 5\n" +
            "0000000000 65535 f \n" +
            $"{streamOffset:0000000000} 00000 n \n" +
            $"{filterOffset:0000000000} 00000 n \n" +
            $"{parametersOffset:0000000000} 00000 n \n" +
            $"{fileOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 5 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        using var reader = PdfDocumentReader.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)));
        var indirectStream = Assert.IsType<CosIndirectObject>(reader.Resolve(new CosKey(1, 0)));
        var stream = Assert.IsType<CosStream>(indirectStream.Value);
        var adapted = CosStreamModelAdapter.FromParserValue(stream, reader);

        var filter = Assert.IsAssignableFrom<ICosFilter>(
            Assert.Single(adapted.GetLinkedObjects("filters")));
        Assert.Equal("Crypt", filter.GetinternalRepresentation());
        Assert.Equal("Identity", filter.GetdecodeParms());
        Assert.Equal("external", adapted.GetF());
        Assert.Equal("Crypt", adapted.GetFFilter());
        Assert.Equal("dictionary(size = 1)", adapted.GetFDecodeParms());
    }
}
