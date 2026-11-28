using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

internal static class CosObjectModelAdapter
{
    public static ICosObject FromParserValue(CosValue value, PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            CosBoolean boolean => CosBoolModelAdapter.ValueOf(boolean),
            CosInteger or CosReal => CosNumberModelAdapter.FromParserValue(value)
                ?? throw new InvalidOperationException("The numeric COS adapter rejected a numeric value."),
            CosName name => CosNameModelAdapter.FromParserValue(name)
                ?? throw new InvalidOperationException("The COS name adapter rejected a name value."),
            CosString text => CosStringModelAdapter.FromParserValue(text)
                ?? throw new InvalidOperationException("The COS string adapter rejected a string value."),
            CosNull => CosNullModelAdapter.Instance,
            CosIndirectReference reference => AdaptReference(reference, reader),
            CosArray array => CosArrayModelAdapter.FromParserValue(array, reader),
            CosDictionary dictionary => CosDictionaryModelAdapter.FromParserValue(dictionary, reader),
            CosStream stream => CosStreamModelAdapter.FromParserValue(stream, reader),
            _ => throw new NotSupportedException(
                $"COS object adaptation is not implemented for '{value.GetType().Name}'.")
        };
    }

    private static ICosObject AdaptReference(CosIndirectReference reference, PdfDocumentReader? reader)
    {
        var indirect = new CosIndirectObject(
            new CosKey(reference.ObjectNumber, reference.Generation),
            reference);
        return CosIndirectModelAdapter.FromParserValue(indirect, reader);
    }
}
