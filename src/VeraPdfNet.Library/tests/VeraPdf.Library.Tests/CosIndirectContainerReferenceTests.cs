using System.Text;
using VeraPdf.Library.Validation.Gf.Model.Cos;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Model.PDLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Tests;

public class CosIndirectContainerReferenceTests
{
    [Fact]
    public void ResolvesReferencesInsideArraysAndDictionaries()
    {
        var prefix = "%PDF-1.7\n";
        var rootObject =
            "1 0 obj\n<< /Type /Page /Items [2 0 R] /Nested << /Ref 2 0 R >> /Metadata 3 0 R >>\nendobj\n";
        var valueObject = "2 0 obj\n42\nendobj\n";
        var metadataObject =
            "3 0 obj\n<< /Type /Metadata /Subtype /XML /Length 1 >>\nstream\nx\nendstream\nendobj\n";
        var rootOffset = Encoding.ASCII.GetByteCount(prefix);
        var valueOffset = rootOffset + Encoding.ASCII.GetByteCount(rootObject);
        var metadataOffset = valueOffset + Encoding.ASCII.GetByteCount(valueObject);
        var xrefOffset = metadataOffset + Encoding.ASCII.GetByteCount(metadataObject);
        var pdf = prefix +
            rootObject +
            valueObject +
            metadataObject +
            "xref\n0 4\n" +
            "0000000000 65535 f \n" +
            $"{rootOffset:0000000000} 00000 n \n" +
            $"{valueOffset:0000000000} 00000 n \n" +
            $"{metadataOffset:0000000000} 00000 n \n" +
            "trailer\n<< /Size 4 /Root 1 0 R >>\n" +
            $"startxref\n{xrefOffset}\n%%EOF";

        using var reader = PdfDocumentReader.Open(
            new MemoryStream(Encoding.ASCII.GetBytes(pdf)));
        var root = Assert.IsType<CosIndirectObject>(reader.Resolve(new CosKey(1, 0)));
        var rootDictionary = Assert.IsAssignableFrom<ICosDict>(
            Assert.Single(CosIndirectModelAdapter.FromParserValue(root, reader)
                .GetLinkedObjects("directObject")));
        var values = rootDictionary.GetLinkedObjects("values");

        var array = Assert.IsAssignableFrom<ICosArray>(values[1]);
        var arrayReference = Assert.IsAssignableFrom<ICosIndirect>(
            Assert.Single(array.GetLinkedObjects("elements")));
        Assert.Equal("2 0", ((IModelObject)arrayReference).GetID());
        Assert.Equal(
            42,
            Assert.IsAssignableFrom<ICosNumber>(
                Assert.Single(arrayReference.GetLinkedObjects("directObject"))).GetintValue());

        var nestedDictionary = Assert.IsAssignableFrom<ICosDict>(values[2]);
        var dictionaryReference = Assert.IsAssignableFrom<ICosIndirect>(
            Assert.Single(nestedDictionary.GetLinkedObjects("values")));
        Assert.Equal(
            42,
            Assert.IsAssignableFrom<ICosNumber>(
                Assert.Single(dictionaryReference.GetLinkedObjects("directObject"))).GetintValue());

        var metadata = Assert.IsAssignableFrom<IPDMetadata>(
            Assert.Single(rootDictionary.GetLinkedObjects("metadata")));
        Assert.Equal("3 0 obj", metadata.GetobjectKey());
        Assert.False(metadata.GetisCatalogMetadata());
        Assert.Equal(1, Assert.IsAssignableFrom<ICosStream>(
            Assert.Single(metadata.GetLinkedObjects("stream"))).GetrealLength());
    }
}
