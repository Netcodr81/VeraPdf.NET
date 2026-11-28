// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
namespace VeraPdf.Library.Xmp;

[Flags]
public enum XmpIterationOptions
{
    None = 0,
    JustChildren = 1,
    JustLeafNodes = 2,
    JustLeafName = 4,
    OmitQualifiers = 8
}

/// <summary>A raw node and its path within a property-scoped traversal.</summary>
public sealed record XmpIterationEntry(string Path, XmpNode Node);

/// <summary>An iteration result; schema entries have a namespace but no path or node.</summary>
public sealed record XmpMetadataIterationEntry(string? NamespaceUri, string? Path, XmpNode? Node)
{
    public bool IsSchema => Node is null;
}
