using VeraPdf.NET.Model;
using VeraPdf.NET.Parser.ModelMapping;

namespace VeraPdf.NET.Parser.Abstractions;

public interface IPdfModelMapper
{
    CosDocument MapToCosDocument(ParsedPdfSnapshot snapshot);
}
