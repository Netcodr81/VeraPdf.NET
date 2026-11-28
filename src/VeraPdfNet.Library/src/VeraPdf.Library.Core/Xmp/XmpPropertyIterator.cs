// Derived from veraPDF xmp-core, Copyright 2006 Adobe Systems Incorporated.
// Distributed under the BSD 3-Clause License; see LICENSE.XMP.txt.
using System.Collections;
using System.Xml.Linq;

namespace VeraPdf.Library.Xmp;

/// <summary>A forward-only cursor over a property subtree.</summary>
public sealed class XmpPropertyIterator : IEnumerator<XmpIterationEntry>
{
    private readonly record struct PendingNode(XmpNode Node, string Path, bool IsRoot, object Siblings);

    private readonly XmpSchemaRegistry registry;
    private readonly XmpIterationOptions options;
    private readonly Stack<PendingNode> pending = new();
    private PendingNode? current;
    private bool skipSubtree;
    private bool disposed;

    internal XmpPropertyIterator(XmpSchemaRegistry registry, XmpNode? root, string path, XmpIterationOptions options)
    {
        this.registry = registry;
        this.options = options;
        if (root is not null) pending.Push(new PendingNode(root, path, true, new object()));
    }

    public XmpIterationEntry Current => current is { } node
        ? new XmpIterationEntry(node.Path, node.Node)
        : throw new InvalidOperationException("The iterator is not positioned on a node.");

    object IEnumerator.Current => Current;

    /// <summary>Skips children and qualifiers of the current node.</summary>
    public void SkipSubtree()
    {
        EnsurePositioned();
        skipSubtree = true;
    }

    /// <summary>Skips the current subtree and remaining siblings in the same child or qualifier list.</summary>
    public void SkipSiblings()
    {
        EnsurePositioned();
        skipSubtree = true;
        var siblings = current!.Value.Siblings;
        while (pending.TryPeek(out var node) && ReferenceEquals(node.Siblings, siblings))
            pending.Pop();
    }

    public bool MoveNext()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (current is { } previous && !skipSubtree) PushChildren(previous);
        current = null;
        skipSubtree = false;
        while (pending.TryPop(out var node))
        {
            var childrenOnly = (options & XmpIterationOptions.JustChildren) != 0;
            if ((!node.IsRoot || !childrenOnly) &&
                ((options & XmpIterationOptions.JustLeafNodes) == 0 || node.Node.Children.Count == 0))
            {
                current = node;
                return true;
            }
            PushChildren(node);
        }
        return false;
    }

    private void PushChildren(PendingNode parent)
    {
        var childrenOnly = (options & XmpIterationOptions.JustChildren) != 0;
        if (childrenOnly && !parent.IsRoot) return;

        var node = parent.Node;
        var childSiblings = new object();
        var children = new List<PendingNode>();
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var segment = XmpPacket.IsArray(node) ? $"[{i + 1}]" : XmpPacket.IterationName(registry, child);
            children.Add(new PendingNode(child, XmpPacket.AccumulateIterationPath(parent.Path, segment, options), false, childSiblings));
        }
        var qualifiers = new List<PendingNode>();
        if (!childrenOnly && (options & XmpIterationOptions.OmitQualifiers) == 0)
        {
            var siblings = new object();
            var index = node.Children.Count;
            if (node.Language is not null)
            {
                var language = new XmpNode(XNamespace.Xml.NamespaceName, "lang", XmpNodeKind.Literal, node.Language, null, []);
                var segment = XmpPacket.IsArray(node) ? $"[{++index}]" : "xml:lang";
                qualifiers.Add(new PendingNode(language, XmpPacket.AccumulateIterationPath(parent.Path, segment, options), false, siblings));
            }
            foreach (var qualifier in node.Qualifiers)
            {
                var segment = XmpPacket.IsArray(node) ? $"[{++index}]" : XmpPacket.IterationName(registry, qualifier);
                qualifiers.Add(new PendingNode(qualifier, XmpPacket.AccumulateIterationPath(parent.Path, segment, options), false, siblings));
            }
        }

        for (var i = qualifiers.Count - 1; i >= 0; i--) pending.Push(qualifiers[i]);
        for (var i = children.Count - 1; i >= 0; i--) pending.Push(children[i]);
    }

    private void EnsurePositioned()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (current is null) throw new InvalidOperationException("The iterator is not positioned on a node.");
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
        disposed = true;
        current = null;
        pending.Clear();
    }
}
