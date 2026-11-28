// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Collections;

namespace VeraPdf.Library.Xmp;

/// <summary>A forward-only cursor over a packet or schema.</summary>
public sealed class XmpMetadataIterator : IEnumerator<XmpMetadataIterationEntry>
{
    private readonly XmpSchemaRegistry registry;
    private readonly string? schemaUri;
    private readonly XmpIterationOptions options;
    private readonly IEnumerator<IGrouping<string, XmpNode>> schemas;
    private XmpNode[] properties = [];
    private int propertyIndex;
    private XmpNode? propertyRoot;
    private XmpPropertyIterator? propertyIterator;
    private XmpMetadataIterationEntry? current;
    private bool skipRemainingSchemas;
    private bool disposed;

    internal XmpMetadataIterator(IReadOnlyList<XmpNode> properties, XmpSchemaRegistry registry,
        string? schemaUri, XmpIterationOptions options)
    {
        this.registry = registry;
        this.schemaUri = schemaUri;
        this.options = options;
        schemas = properties.GroupBy(property => property.NamespaceUri, StringComparer.Ordinal)
            .Where(schema => schemaUri is null || schema.Key == schemaUri).GetEnumerator();
    }

    public XmpMetadataIterationEntry Current => current
        ?? throw new InvalidOperationException("The iterator is not positioned on a node.");

    object IEnumerator.Current => Current;

    /// <summary>Skips children and qualifiers of the current node or all properties of the current schema.</summary>
    public void SkipSubtree()
    {
        EnsurePositioned();
        if (current!.IsSchema) propertyIndex = properties.Length;
        else propertyIterator?.SkipSubtree();
    }

    /// <summary>Skips the current subtree and remaining siblings at the same level.</summary>
    public void SkipSiblings()
    {
        EnsurePositioned();
        if (current!.IsSchema)
        {
            propertyIndex = properties.Length;
            skipRemainingSchemas = true;
        }
        else if (ReferenceEquals(current.Node, propertyRoot))
        {
            propertyIndex = properties.Length;
            propertyIterator?.SkipSubtree();
        }
        else
        {
            propertyIterator!.SkipSiblings();
        }
    }

    public bool MoveNext()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        current = null;
        var childrenOnly = (options & XmpIterationOptions.JustChildren) != 0;
        var leafOnly = (options & XmpIterationOptions.JustLeafNodes) != 0;
        while (true)
        {
            if (propertyIterator is not null)
            {
                if (propertyIterator.MoveNext())
                {
                    var entry = propertyIterator.Current;
                    current = new XmpMetadataIterationEntry(
                        entry.Node.NamespaceUri.Length == 0 ? null : entry.Node.NamespaceUri, entry.Path, entry.Node);
                    return true;
                }
                propertyIterator.Dispose();
                propertyIterator = null;
                propertyRoot = null;
            }

            if (propertyIndex < properties.Length)
            {
                propertyRoot = properties[propertyIndex++];
                var name = XmpPacket.IterationName(registry, propertyRoot);
                if (childrenOnly)
                {
                    if (leafOnly && propertyRoot.Children.Count != 0) continue;
                    current = new XmpMetadataIterationEntry(propertyRoot.NamespaceUri, name, propertyRoot);
                    return true;
                }
                propertyIterator = new XmpPropertyIterator(registry, propertyRoot, name, options);
                continue;
            }

            if (skipRemainingSchemas || !schemas.MoveNext()) return false;
            var schema = schemas.Current;
            properties = childrenOnly && schemaUri is null ? [] : schema.ToArray();
            propertyIndex = 0;
            if (!leafOnly && (!childrenOnly || schemaUri is null))
            {
                current = new XmpMetadataIterationEntry(schema.Key, null, null);
                return true;
            }
        }
    }

    private void EnsurePositioned()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (current is null) throw new InvalidOperationException("The iterator is not positioned on a node.");
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        propertyIterator?.Dispose();
        schemas.Dispose();
        current = null;
    }
}
