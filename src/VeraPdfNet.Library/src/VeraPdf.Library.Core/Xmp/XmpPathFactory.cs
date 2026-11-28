// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Text;

namespace VeraPdf.Library.Xmp;

public static class XmpPathFactory
{
    public static string ComposeArrayItemPath(string? arrayName, int itemIndex) => itemIndex switch
    {
        > 0 => (arrayName ?? "null") + "[" + itemIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + "]",
        -1 => (arrayName ?? "null") + "[last()]",
        _ => throw new XmpException("Array index must be larger than zero", XmpError.BadIndex)
    };

    public static string ComposeStructFieldPath(XmpSchemaRegistry registry, string fieldNamespace, string fieldName)
    {
        Require(fieldNamespace, "Empty field namespace URI", XmpError.BadSchema);
        Require(fieldName, "Empty f name", XmpError.BadPath);
        return "/" + SimpleRoot(registry, fieldNamespace, fieldName, "The field name must be simple");
    }

    public static string ComposeQualifierPath(XmpSchemaRegistry registry, string qualifierNamespace, string qualifierName)
    {
        Require(qualifierNamespace, "Empty qualifier namespace URI", XmpError.BadSchema);
        Require(qualifierName, "Empty qualifier name", XmpError.BadPath);
        return "/?" + SimpleRoot(registry, qualifierNamespace, qualifierName, "The qualifier name must be simple");
    }

    public static string ComposeLanguageSelector(string? arrayName, string language)
    {
        ArgumentNullException.ThrowIfNull(language);
        return (arrayName ?? "null") + "[?xml:lang=\"" + NormalizeLanguage(language) + "\"]";
    }

    internal static string NormalizeLanguage(string language)
    {
        if (language == "x-default") return language;
        var value = new StringBuilder();
        var subtag = 1;
        foreach (var c in language)
        {
            if (c is '-' or '_') { value.Append('-'); subtag++; }
            else if (c != ' ') value.Append(subtag == 2 ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
        }
        return value.ToString();
    }

    public static string ComposeFieldSelector(XmpSchemaRegistry registry, string? arrayName,
        string fieldNamespace, string fieldName, string? fieldValue) =>
        (arrayName ?? "null") + "[" + SimpleRoot(registry, fieldNamespace, fieldName, "The fieldName name must be simple")
        + "=\"" + (fieldValue ?? "null") + "\"]";

    private static string SimpleRoot(XmpSchemaRegistry registry, string schema, string name, string message)
    {
        var path = XmpPath.Parse(registry, schema, name);
        if (path.Segments.Count != 2) throw new XmpException(message, XmpError.BadPath);
        return path.Segments[1].Name;
    }

    private static void Require(string value, string message, XmpError error)
    {
        if (string.IsNullOrEmpty(value)) throw new XmpException(message, error);
    }
}
