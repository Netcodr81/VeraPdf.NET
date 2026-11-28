using VeraPdf.Model;
using VeraPdf.Model.BaseLayer;
using VeraPdf.Model.CosLayer;
using VeraPdf.Parser;
using VeraPdf.Parser.Cos;

namespace VeraPdf.Library.Validation.Gf.Model.Cos;

public sealed class CosArrayModelAdapter : GenericModelObject, ICosArray
{
    private readonly long _size;
    private readonly IReadOnlyList<IModelObject> _elements;

    private CosArrayModelAdapter(CosArray value, PdfDocumentReader? reader)
        : base("CosArray")
    {
        _size = value.Values.Count;
        var elements = new List<IModelObject>(value.Values.Count);
        foreach (var item in value.Values)
        {
            if (item is not null)
            {
                elements.Add(CosObjectModelAdapter.FromParserValue(item, reader));
            }
        }

        _elements = elements.AsReadOnly();
    }

    public long? Getsize() => _size;

    public override IReadOnlyList<IModelObject> GetLinkedObjects(string linkName) =>
        linkName == "elements"
            ? _elements
            : base.GetLinkedObjects(linkName);

    public static ICosArray FromParserValue(CosArray value, PdfDocumentReader? reader = null)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new CosArrayModelAdapter(value, reader);
    }
}
