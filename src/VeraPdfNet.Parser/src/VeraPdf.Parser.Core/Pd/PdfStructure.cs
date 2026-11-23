using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public sealed class PdfStructure : PdfDictionaryObject
{
    public PdfStructure(CosDictionary dictionary)
        : base(dictionary)
    {
    }
}
