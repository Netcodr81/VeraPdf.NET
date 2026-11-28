// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Text;

namespace VeraPdf.Library.Xmp;

public enum XmpError
{
    BadParameter = 4,
    BadValue = 5,
    BadSchema = 101,
    BadPath = 102,
    BadOptions = 103,
    BadIndex = 104,
    BadSerialize = 107,
    BadXml = 201,
    BadRdf = 202,
    BadXmp = 203
}

public sealed class XmpException(string message, XmpError error, Exception? innerException = null) : Exception(message, innerException)
{
    public XmpError Error { get; } = error;
}

public enum XmpPathSegmentKind
{
    Schema,
    Field,
    Qualifier,
    ArrayIndex,
    ArrayLast,
    QualifierSelector,
    FieldSelector
}

public sealed record XmpPathSegment(string Name, XmpPathSegmentKind Kind, bool IsAlias = false,
    XmpAliasForm AliasForm = XmpAliasForm.Direct);

/// <summary>An expanded XMP path, including its schema and any resolved alias steps.</summary>
public sealed class XmpPath
{
    private XmpPath(List<XmpPathSegment> segments) => Segments = Array.AsReadOnly(segments.ToArray());

    public IReadOnlyList<XmpPathSegment> Segments { get; }

    /// <summary>Parses and expands a path using the supplied schema registry.</summary>
    public static XmpPath Parse(XmpSchemaRegistry registry, string schemaUri, string path)
    {
        ArgumentNullException.ThrowIfNull(registry);
        if (schemaUri is null || path is null) throw Error("Parameter must not be null", XmpError.BadParameter);
        var rootEnd = 0;
        while (rootEnd < path.Length && "/[*".IndexOf(path[rootEnd]) < 0) rootEnd++;
        if (rootEnd == 0) throw Error("Empty initial XMPPath step", XmpError.BadPath);
        var root = VerifyRoot(registry, schemaUri, path[..rootEnd]);
        var alias = registry.FindAlias(root);
        var segments = new List<XmpPathSegment>();
        if (alias is null)
        {
            segments.Add(new(schemaUri, XmpPathSegmentKind.Schema));
            segments.Add(new(root, XmpPathSegmentKind.Field));
        }
        else
        {
            segments.Add(new(alias.NamespaceUri, XmpPathSegmentKind.Schema));
            segments.Add(new(VerifyRoot(registry, alias.NamespaceUri, alias.PropertyName), XmpPathSegmentKind.Field, true, alias.Form));
            if (alias.Form == XmpAliasForm.AltTextDefaultItem)
                segments.Add(new("[?xml:lang='x-default']", XmpPathSegmentKind.QualifierSelector, true, alias.Form));
            else if (alias.Form == XmpAliasForm.OrderedArrayFirstItem)
                segments.Add(new("[1]", XmpPathSegmentKind.ArrayIndex, true, alias.Form));
        }

        var pos = rootEnd;
        while (pos < path.Length)
        {
            if (path[pos] == '/')
            {
                pos++;
                if (pos == path.Length) throw Error("Empty XMPPath segment", XmpError.BadPath);
            }
            if (path[pos] == '*')
            {
                pos++;
                if (pos == path.Length || path[pos] != '[') throw Error("Missing '[' after '*'", XmpError.BadPath);
            }
            if (path[pos] == '[')
                segments.Add(ParseIndex(registry, path, ref pos));
            else
            {
                var start = pos;
                while (pos < path.Length && "/[*".IndexOf(path[pos]) < 0) pos++;
                if (start == pos) throw Error("Empty XMPPath segment", XmpError.BadPath);
                var name = path[start..pos];
                if (name[0] == '@')
                {
                    name = "?" + name[1..];
                    if (name != "?xml:lang") throw Error("Only xml:lang allowed with '@'", XmpError.BadPath);
                }
                var isQualifier = name[0] == '?';
                VerifyQualifiedName(registry, isQualifier ? name[1..] : name);
                segments.Add(new(name, isQualifier ? XmpPathSegmentKind.Qualifier : XmpPathSegmentKind.Field));
            }
        }
        return new XmpPath(segments);
    }

    public override string ToString()
    {
        var result = new StringBuilder();
        for (var i = 1; i < Segments.Count; i++)
        {
            if (i > 1 && Segments[i].Kind is XmpPathSegmentKind.Field or XmpPathSegmentKind.Qualifier)
                result.Append('/');
            result.Append(Segments[i].Name);
        }
        return result.ToString();
    }

    private static XmpPathSegment ParseIndex(XmpSchemaRegistry registry, string path, ref int pos)
    {
        var start = pos++;
        if (pos >= path.Length) throw Error("Missing ']' for array index", XmpError.BadPath);
        XmpPathSegmentKind kind;
        if (path[pos] is >= '0' and <= '9')
        {
            while (pos < path.Length && path[pos] is >= '0' and <= '9') pos++;
            kind = XmpPathSegmentKind.ArrayIndex;
        }
        else
        {
            while (pos < path.Length && path[pos] != ']' && path[pos] != '=') pos++;
            if (pos >= path.Length) throw Error("Missing ']' or '=' for array index", XmpError.BadPath);
            if (path[pos] == ']')
            {
                if (path[start..pos] != "[last()") throw Error("Invalid non-numeric array index", XmpError.BadPath);
                kind = XmpPathSegmentKind.ArrayLast;
            }
            else
            {
                var name = path[(start + 1)..pos];
                pos++;
                if (pos >= path.Length || path[pos] is not ('\'' or '"'))
                    throw Error("Invalid quote in array selector", XmpError.BadPath);
                var quote = path[pos++];
                while (pos < path.Length)
                {
                    if (path[pos] == quote)
                    {
                        if (pos + 1 >= path.Length || path[pos + 1] != quote) break;
                        pos++;
                    }
                    pos++;
                }
                if (pos >= path.Length) throw Error("No terminating quote for array selector", XmpError.BadPath);
                pos++;
                kind = XmpPathSegmentKind.FieldSelector;
                if (name.StartsWith('@'))
                {
                    if (name != "@xml:lang") throw Error("Only xml:lang allowed with '@'", XmpError.BadPath);
                    kind = XmpPathSegmentKind.QualifierSelector;
                    VerifyQualifiedName(registry, name[1..]);
                }
                else if (name.StartsWith('?'))
                {
                    kind = XmpPathSegmentKind.QualifierSelector;
                    VerifyQualifiedName(registry, name[1..]);
                }
            }
        }
        if (pos >= path.Length || path[pos] != ']') throw Error("Missing ']' for array index", XmpError.BadPath);
        pos++;
        var segment = path[start..pos];
        if (kind == XmpPathSegmentKind.QualifierSelector && segment.StartsWith("[@", StringComparison.Ordinal))
            segment = "[?" + segment[2..];
        return new(segment, kind);
    }

    private static void VerifyQualifiedName(XmpSchemaRegistry registry, string name)
    {
        var colon = name.IndexOf(':');
        if (colon > 0 && XmpSchemaRegistry.IsNamespaceName(name[..colon]))
        {
            if (registry.GetNamespaceUri(name[..colon]) is not null) return;
            throw Error("Unknown namespace prefix for qualified name", XmpError.BadPath);
        }
        throw Error("Ill-formed qualified name", XmpError.BadPath);
    }

    private static string VerifyRoot(XmpSchemaRegistry registry, string schemaUri, string root)
    {
        if (schemaUri.Length == 0) throw Error("Schema namespace URI is required", XmpError.BadSchema);
        if (root[0] is '?' or '@') throw Error("Top level name must not be a qualifier", XmpError.BadPath);
        var prefix = registry.GetNamespacePrefix(schemaUri)
            ?? throw Error("Unregistered schema namespace URI", XmpError.BadSchema);
        var colon = root.IndexOf(':');
        if (colon < 0)
        {
            if (!XmpSchemaRegistry.IsXmlName(root)) throw Error("Bad XML name", XmpError.BadPath);
            return prefix + root;
        }
        if (!XmpSchemaRegistry.IsXmlName(root[..colon]) || !XmpSchemaRegistry.IsXmlName(root[colon..]))
            throw Error("Bad XML name", XmpError.BadPath);
        if (root[..(colon + 1)] != prefix) throw Error("Schema namespace URI and prefix mismatch", XmpError.BadSchema);
        return root;
    }

    private static XmpException Error(string message, XmpError error) => new(message, error);
}
