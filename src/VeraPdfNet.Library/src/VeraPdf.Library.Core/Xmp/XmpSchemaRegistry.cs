// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Collections.ObjectModel;

namespace VeraPdf.Library.Xmp;

public enum XmpAliasForm
{
    Direct,
    OrderedArrayFirstItem,
    AltTextDefaultItem
}

public sealed record XmpAlias(string NamespaceUri, string Prefix, string PropertyName, XmpAliasForm Form);

/// <summary>Registers XMP namespace prefixes and exposes the standard property aliases.</summary>
public sealed class XmpSchemaRegistry
{
    private const string Dc = "http://purl.org/dc/elements/1.1/";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";
    private const string Rights = "http://ns.adobe.com/xap/1.0/rights/";

    [ThreadStatic] private static Dictionary<string, string>? threadNamespaces;
    [ThreadStatic] private static Dictionary<string, string>? threadPrefixes;

    private readonly object sync = new();
    private readonly Dictionary<string, string> namespaces = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> prefixes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, XmpAlias> aliases = new(StringComparer.Ordinal);

    /// <summary>
    /// Creates a registry and clears custom registrations on the current thread, as in xmp-core.
    /// Standard registrations are scoped to this instance.
    /// </summary>
    public XmpSchemaRegistry()
    {
        threadNamespaces = new(StringComparer.Ordinal);
        threadPrefixes = new(StringComparer.Ordinal);
        foreach (var (uri, prefix) in StandardNamespaces)
            RegisterNamespace(uri, prefix, true);
        foreach (var (alias, uri, property, form) in StandardAliases)
            aliases.Add(alias, new XmpAlias(uri, namespaces[uri], property, form));
    }

    /// <summary>Registers a namespace, resolving prefix collisions using the upstream suffix convention.</summary>
    public string RegisterNamespace(string namespaceUri, string suggestedPrefix, bool isStandard = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(namespaceUri);
        ArgumentException.ThrowIfNullOrEmpty(suggestedPrefix);
        var prefix = suggestedPrefix.EndsWith(':') ? suggestedPrefix : suggestedPrefix + ":";
        if (!IsNamespaceName(prefix[..^1]))
            throw new ArgumentException("The prefix is a bad XML name.", nameof(suggestedPrefix));

        lock (sync)
        {
            var existing = GetNamespacePrefix(namespaceUri);
            if (existing is not null)
                return existing;
            var candidate = prefix;
            for (var index = 1; GetNamespaceUri(candidate) is not null; index++)
                candidate = prefix[..^1] + "_" + index + "_:";
            if (isStandard)
            {
                namespaces.Add(namespaceUri, candidate);
                prefixes.Add(candidate, namespaceUri);
            }
            else
            {
                ThreadNamespaces.Add(namespaceUri, candidate);
                ThreadPrefixes.Add(candidate, namespaceUri);
            }
            return candidate;
        }
    }

    public string? GetNamespacePrefix(string? namespaceUri)
    {
        if (namespaceUri is null) return null;
        lock (sync)
            return ThreadNamespaces.GetValueOrDefault(namespaceUri) ?? namespaces.GetValueOrDefault(namespaceUri);
    }

    public string? GetNamespaceUri(string? prefix)
    {
        if (prefix is null) return null;
        if (!prefix.EndsWith(':')) prefix += ":";
        lock (sync)
            return ThreadPrefixes.GetValueOrDefault(prefix) ?? prefixes.GetValueOrDefault(prefix);
    }

    /// <summary>Removes a namespace from the current thread and this registry's standard mappings.</summary>
    public void DeleteNamespace(string? namespaceUri)
    {
        if (namespaceUri is null) return;
        lock (sync)
        {
            var prefix = GetNamespacePrefix(namespaceUri);
            if (prefix is null) return;
            ThreadNamespaces.Remove(namespaceUri);
            ThreadPrefixes.Remove(prefix);
            namespaces.Remove(namespaceUri);
            prefixes.Remove(prefix);
        }
    }

    public IReadOnlyDictionary<string, string> GetNamespaces()
    {
        lock (sync) return Snapshot(ThreadNamespaces, namespaces);
    }

    public IReadOnlyDictionary<string, string> GetPrefixes()
    {
        lock (sync) return Snapshot(ThreadPrefixes, prefixes);
    }

    public XmpAlias? FindAlias(string? qualifiedName)
    {
        if (qualifiedName is null) return null;
        lock (sync) return aliases.GetValueOrDefault(qualifiedName);
    }

    public XmpAlias? ResolveAlias(string? namespaceUri, string? propertyName)
    {
        lock (sync)
        {
            var prefix = GetNamespacePrefix(namespaceUri);
            return prefix is null ? null : aliases.GetValueOrDefault(prefix + (propertyName ?? "null"));
        }
    }

    public IReadOnlyList<XmpAlias> FindAliases(string? namespaceUri)
    {
        lock (sync)
        {
            var prefix = GetNamespacePrefix(namespaceUri);
            return prefix is null ? Array.Empty<XmpAlias>() : Array.AsReadOnly(
                aliases.Where(item => item.Key.StartsWith(prefix, StringComparison.Ordinal))
                    .OrderBy(item => item.Key, StringComparer.Ordinal).Select(item => item.Value).ToArray());
        }
    }

    public IReadOnlyDictionary<string, XmpAlias> GetAliases()
    {
        lock (sync)
            return new ReadOnlyDictionary<string, XmpAlias>(new SortedDictionary<string, XmpAlias>(aliases, StringComparer.Ordinal));
    }

    private static Dictionary<string, string> ThreadNamespaces => threadNamespaces ??= new(StringComparer.Ordinal);
    private static Dictionary<string, string> ThreadPrefixes => threadPrefixes ??= new(StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> Snapshot(
        Dictionary<string, string> local, Dictionary<string, string> standard)
    {
        var result = new SortedDictionary<string, string>(local, StringComparer.Ordinal);
        foreach (var item in standard) result[item.Key] = item.Value;
        return new ReadOnlyDictionary<string, string>(result);
    }

    internal static bool IsNamespaceName(string name)
    {
        if (name.Length == 0) return true;
        if (!IsNameStart(name[0])) return false;
        return name.Skip(1).All(c => IsNameStart(c) || c is '-' or '.' or >= '0' and <= '9' or '\u00b7'
            or >= '\u0300' and <= '\u036f' or >= '\u203f' and <= '\u2040');
    }

    internal static bool IsXmlName(string name)
    {
        if (name.Length == 0) return true;
        if (name[0] != ':' && !IsNameStart(name[0])) return false;
        return name.Skip(1).All(c => c == ':' || IsNameStart(c) || c is '-' or '.' or >= '0' and <= '9' or '\u00b7'
            or >= '\u0300' and <= '\u036f' or >= '\u203f' and <= '\u2040');
    }

    private static bool IsNameStart(char c) =>
        c is >= 'A' and <= 'Z' or '_' or >= 'a' and <= 'z'
            or >= '\u00c0' and <= '\u00d6' or >= '\u00d8' and <= '\u00f6'
            or >= '\u00f8' and <= '\u02ff' or >= '\u0370' and <= '\u037d'
            or >= '\u037f' and <= '\u1fff' or >= '\u200c' and <= '\u200d'
            or >= '\u2070' and <= '\u218f' or >= '\u2c00' and <= '\u2fef'
            or >= '\u3001' and <= '\ud7ff' or >= '\uf900' and <= '\ufdcf'
            or >= '\ufdf0' and <= '\ufffd';

    private static readonly (string Uri, string Prefix)[] StandardNamespaces =
    [
        ("http://www.w3.org/XML/1998/namespace", "xml"),
        ("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf"),
        (Dc, "dc"), ("http://iptc.org/std/Iptc4xmpCore/1.0/xmlns/", "Iptc4xmpCore"),
        ("http://iptc.org/std/Iptc4xmpExt/2008-02-29/", "Iptc4xmpExt"),
        ("http://ns.adobe.com/DICOM/", "DICOM"), ("http://ns.useplus.org/ldf/xmp/1.0/", "plus"),
        ("adobe:ns:meta/", "x"), ("http://ns.adobe.com/iX/1.0/", "iX"),
        (Xmp, "xmp"), (Rights, "xmpRights"),
        ("http://ns.adobe.com/xap/1.0/mm/", "xmpMM"), ("http://ns.adobe.com/xap/1.0/bj/", "xmpBJ"),
        ("http://ns.adobe.com/xmp/note/", "xmpNote"), ("http://ns.adobe.com/pdf/1.3/", "pdf"),
        ("http://ns.adobe.com/pdfx/1.3/", "pdfx"), ("http://www.npes.org/pdfx/ns/id/", "pdfxid"),
        ("http://www.aiim.org/pdfa/ns/schema#", "pdfaSchema"), ("http://www.aiim.org/pdfa/ns/property#", "pdfaProperty"),
        ("http://www.aiim.org/pdfa/ns/type#", "pdfaType"), ("http://www.aiim.org/pdfa/ns/field#", "pdfaField"),
        ("http://www.aiim.org/pdfa/ns/id/", "pdfaid"), ("http://www.aiim.org/pdfa/ns/extension/", "pdfaExtension"),
        ("http://ns.adobe.com/photoshop/1.0/", "photoshop"), ("http://ns.adobe.com/album/1.0/", "album"),
        ("http://ns.adobe.com/exif/1.0/", "exif"), ("http://cipa.jp/exif/1.0/", "exifEX"),
        ("http://ns.adobe.com/exif/1.0/aux/", "aux"), ("http://ns.adobe.com/tiff/1.0/", "tiff"),
        ("http://ns.adobe.com/png/1.0/", "png"), ("http://ns.adobe.com/jpeg/1.0/", "jpeg"),
        ("http://ns.adobe.com/jp2k/1.0/", "jp2k"), ("http://ns.adobe.com/camera-raw-settings/1.0/", "crs"),
        ("http://ns.adobe.com/StockPhoto/1.0/", "bmsp"), ("http://ns.adobe.com/creatorAtom/1.0/", "creatorAtom"),
        ("http://ns.adobe.com/asf/1.0/", "asf"), ("http://ns.adobe.com/xmp/wav/1.0/", "wav"),
        ("http://ns.adobe.com/bwf/bext/1.0/", "bext"), ("http://ns.adobe.com/riff/info/", "riffinfo"),
        ("http://ns.adobe.com/xmp/1.0/Script/", "xmpScript"), ("http://ns.adobe.com/TransformXMP/", "txmp"),
        ("http://ns.adobe.com/swf/1.0/", "swf"), ("http://ns.adobe.com/xmp/1.0/DynamicMedia/", "xmpDM"),
        ("http://ns.adobe.com/xmp/transient/1.0/", "xmpx"), ("http://ns.adobe.com/xap/1.0/t/", "xmpT"),
        ("http://ns.adobe.com/xap/1.0/t/pg/", "xmpTPg"), ("http://ns.adobe.com/xap/1.0/g/", "xmpG"),
        ("http://ns.adobe.com/xap/1.0/g/img/", "xmpGImg"), ("http://ns.adobe.com/xap/1.0/sType/Font#", "stFnt"),
        ("http://ns.adobe.com/xap/1.0/sType/Dimensions#", "stDim"), ("http://ns.adobe.com/xap/1.0/sType/ResourceEvent#", "stEvt"),
        ("http://ns.adobe.com/xap/1.0/sType/ResourceRef#", "stRef"), ("http://ns.adobe.com/xap/1.0/sType/Version#", "stVer"),
        ("http://ns.adobe.com/xap/1.0/sType/Job#", "stJob"), ("http://ns.adobe.com/xap/1.0/sType/ManifestItem#", "stMfs"),
        ("http://ns.adobe.com/xmp/Identifier/qual/1.0/", "xmpidq")
    ];

    private static readonly (string Alias, string Uri, string Property, XmpAliasForm Form)[] StandardAliases =
    [
        ("xmp:Author", Dc, "creator", XmpAliasForm.OrderedArrayFirstItem),
        ("xmp:Authors", Dc, "creator", XmpAliasForm.Direct), ("xmp:Description", Dc, "description", XmpAliasForm.Direct),
        ("xmp:Format", Dc, "format", XmpAliasForm.Direct), ("xmp:Keywords", Dc, "subject", XmpAliasForm.Direct),
        ("xmp:Locale", Dc, "language", XmpAliasForm.Direct), ("xmp:Title", Dc, "title", XmpAliasForm.Direct),
        ("xmpRights:Copyright", Dc, "rights", XmpAliasForm.Direct),
        ("pdf:Author", Dc, "creator", XmpAliasForm.OrderedArrayFirstItem), ("pdf:BaseURL", Xmp, "BaseURL", XmpAliasForm.Direct),
        ("pdf:CreationDate", Xmp, "CreateDate", XmpAliasForm.Direct), ("pdf:Creator", Xmp, "CreatorTool", XmpAliasForm.Direct),
        ("pdf:ModDate", Xmp, "ModifyDate", XmpAliasForm.Direct), ("pdf:Subject", Dc, "description", XmpAliasForm.AltTextDefaultItem),
        ("pdf:Title", Dc, "title", XmpAliasForm.AltTextDefaultItem),
        ("photoshop:Author", Dc, "creator", XmpAliasForm.OrderedArrayFirstItem),
        ("photoshop:Caption", Dc, "description", XmpAliasForm.AltTextDefaultItem), ("photoshop:Copyright", Dc, "rights", XmpAliasForm.AltTextDefaultItem),
        ("photoshop:Keywords", Dc, "subject", XmpAliasForm.Direct), ("photoshop:Marked", Rights, "Marked", XmpAliasForm.Direct),
        ("photoshop:Title", Dc, "title", XmpAliasForm.AltTextDefaultItem), ("photoshop:WebStatement", Rights, "WebStatement", XmpAliasForm.Direct),
        ("tiff:Artist", Dc, "creator", XmpAliasForm.OrderedArrayFirstItem), ("tiff:Copyright", Dc, "rights", XmpAliasForm.Direct),
        ("tiff:DateTime", Xmp, "ModifyDate", XmpAliasForm.Direct), ("tiff:ImageDescription", Dc, "description", XmpAliasForm.Direct),
        ("tiff:Software", Xmp, "CreatorTool", XmpAliasForm.Direct),
        ("png:Author", Dc, "creator", XmpAliasForm.OrderedArrayFirstItem), ("png:Copyright", Dc, "rights", XmpAliasForm.AltTextDefaultItem),
        ("png:CreationTime", Xmp, "CreateDate", XmpAliasForm.Direct), ("png:Description", Dc, "description", XmpAliasForm.AltTextDefaultItem),
        ("png:ModificationTime", Xmp, "ModifyDate", XmpAliasForm.Direct), ("png:Software", Xmp, "CreatorTool", XmpAliasForm.Direct),
        ("png:Title", Dc, "title", XmpAliasForm.AltTextDefaultItem)
    ];
}
