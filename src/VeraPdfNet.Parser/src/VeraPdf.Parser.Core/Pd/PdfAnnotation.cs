using VeraPdf.Parser.Cos;

namespace VeraPdf.Parser.Pd;

public sealed class PdfAnnotation : PdfDictionaryObject
{
    public PdfAnnotation(CosDictionary dictionary)
        : base(dictionary)
    {
    }
}
