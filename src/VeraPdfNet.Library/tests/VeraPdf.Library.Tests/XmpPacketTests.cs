using System.Text;
using VeraPdf.Library.Xmp;

namespace VeraPdf.Library.Tests;

public class XmpPacketTests
{
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xmp = "http://ns.adobe.com/xap/1.0/";

    [Fact]
    public void ReadsUnnormalizedValuesFromJavaReferencePacket()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-packet-reference.tsv"));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Decode(lines[0].Split('\t')[1])));
        var packet = XmpPacket.Parse(stream);
        var registry = new XmpSchemaRegistry();
        Assert.True(stream.CanRead);
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            var ns = Decode(fields[1]);
            var path = Decode(fields[2]);
            Assert.Equal(Decode(fields[3]), Assert.IsType<XmpNode>(packet.GetPropertyByPath(registry, ns, path)).Value);
        }
        Assert.Equal(XmpNodeKind.OrderedArray, packet.GetProperty("http://purl.org/dc/elements/1.1/", "creator")!.Kind);
        Assert.Equal(XmpNodeKind.AlternativeArray, packet.GetProperty("http://purl.org/dc/elements/1.1/", "title")!.Kind);
        Assert.Equal(XmpNodeKind.Uri, packet.GetProperty(Xmp, "Identifier")!.Kind);
        Assert.Equal(XmpNodeKind.Structure, packet.GetProperty(Xmp, "Struct")!.Kind);
    }

    [Fact]
    public void PathLookupMatchesJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-lookup-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            var schema = Decode(fields[1]);
            var path = Decode(fields[2]);
            if (fields[3] == "error")
            {
                var exception = Assert.Throws<XmpException>(() => packet.GetPropertyByPath(registry, schema, path));
                Assert.Equal(int.Parse(fields[4]), (int)exception.Error);
            }
            else if (fields[3] == "missing")
                Assert.Null(packet.GetPropertyByPath(registry, schema, path));
            else
                Assert.Equal(Decode(fields[4]), Assert.IsType<XmpNode>(packet.GetPropertyByPath(registry, schema, path)).Value);
        }
    }

    [Fact]
    public void ReadOnlyArrayOperationsMatchJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-array-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            var schema = fields[1] == "-" ? null : Decode(fields[1]);
            var path = fields[2] == "-" ? null : Decode(fields[2]);
            var index = int.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture);
            object? Read() => fields[0] == "count"
                ? packet.CountArrayItems(registry, schema!, path!)
                : packet.GetArrayItem(registry, schema!, path!, index);
            if (fields[4] == "error")
                Assert.Equal(int.Parse(fields[5], System.Globalization.CultureInfo.InvariantCulture),
                    (int)Assert.Throws<XmpException>(Read).Error);
            else if (fields[4] == "missing")
                Assert.Null(Read());
            else if (fields[4] == "count")
                Assert.Equal(int.Parse(fields[5], System.Globalization.CultureInfo.InvariantCulture), Assert.IsType<int>(Read()));
            else
                Assert.Equal(Decode(fields[5]), Assert.IsType<XmpNode>(Read()).Value);
        }
    }

    [Fact]
    public void ReadOnlyArrayOperationsRejectMissingRegistryAndTruncatedPaths()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}'/>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.CountArrayItems(null!, Xmp, "Items"));
        Assert.Throws<ArgumentNullException>(() => packet.GetArrayItem(null!, Xmp, "Items", 1));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.CountArrayItems(registry, Xmp, "Items[")).Error);
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.GetArrayItem(registry, Xmp, "Items[", 1)).Error);
    }

    [Fact]
    public void TypedPropertyReadsMatchJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-typed-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            var path = Decode(fields[1]);
            string? Read() => fields[0] switch
            {
                "string" => packet.GetPropertyString(registry, Xmp, path),
                "boolean" => packet.GetPropertyBoolean(registry, Xmp, path)?.ToString().ToLowerInvariant(),
                "integer" => packet.GetPropertyInteger(registry, Xmp, path)?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "long" => packet.GetPropertyLong(registry, Xmp, path)?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "double" => packet.GetPropertyDouble(registry, Xmp, path) is double value
                    ? unchecked((ulong)BitConverter.DoubleToInt64Bits(value)).ToString("x16", System.Globalization.CultureInfo.InvariantCulture) : null,
                "date" => packet.GetPropertyDate(registry, Xmp, path)?.ToString(),
                "base64" => packet.GetPropertyBase64(registry, Xmp, path) is byte[] bytes ? Convert.ToBase64String(bytes) : null,
                _ => throw new InvalidDataException("Unknown reference operation")
            };
            if (fields[2] == "error")
                Assert.Equal(int.Parse(fields[3], System.Globalization.CultureInfo.InvariantCulture),
                    (int)Assert.Throws<XmpException>(Read).Error);
            else if (fields[2] == "missing")
                Assert.Null(Read());
            else
                Assert.Equal(Decode(fields[3]), Read());
        }
    }

    [Fact]
    public void TypedPropertyReadsValidateArgumentsAndResolveAliases()
    {
        const string dc = "http://purl.org/dc/elements/1.1/";
        const string pdf = "http://ns.adobe.com/pdf/1.3/";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:dc='{dc}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><dc:creator><rdf:Seq><rdf:li>42</rdf:li></rdf:Seq></dc:creator>" +
            "<p:Qualified rdf:value='1' p:Source='12'/></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        Assert.Equal(42, packet.GetPropertyInteger(registry, pdf, "Author"));
        Assert.Equal(12, packet.GetPropertyInteger(registry, Xmp, "Qualified/?xmp:Source"));
        Assert.Null(packet.GetPropertyInteger(registry, Xmp, "Missing"));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.GetPropertyString(registry, "", "Name")).Error);
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.GetPropertyBoolean(registry, Xmp, null!)).Error);
        Assert.Throws<ArgumentNullException>(() => packet.GetPropertyDate(null!, Xmp, "Name"));
        Assert.Equal(XmpError.BadPath, Assert.Throws<XmpException>(() => packet.GetPropertyBase64(registry, dc, "creator")).Error);
    }

    [Fact]
    public void FieldAndQualifierReadsMatchJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-field-qualifier-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        registry.RegisterNamespace("urn:field-test", "test");
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            string? DecodeArgument(int i) => fields[i] == "-" ? null : Decode(fields[i]);
            XmpNode? Read() => fields[0] == "field"
                ? packet.GetStructField(registry, DecodeArgument(1)!, DecodeArgument(2)!, DecodeArgument(3)!, DecodeArgument(4)!)
                : packet.GetQualifier(registry, DecodeArgument(1)!, DecodeArgument(2)!, DecodeArgument(3)!, DecodeArgument(4)!);
            if (fields[5] == "error")
                Assert.Equal(int.Parse(fields[6], System.Globalization.CultureInfo.InvariantCulture),
                    (int)Assert.Throws<XmpException>(Read).Error);
            else if (fields[5] == "missing")
                Assert.Null(Read());
            else
                Assert.Equal(Decode(fields[6]), Assert.IsType<XmpNode>(Read()).Value);
        }
        Assert.Throws<ArgumentNullException>(() => packet.GetStructField(null!, Xmp, "Struct", Xmp, "Field"));
        Assert.Throws<ArgumentNullException>(() => packet.GetQualifier(null!, Xmp, "Qualified", Xmp, "Source"));
    }

    [Fact]
    public void QualifierLookupUsesNamespaceUriRatherThanSourcePrefix()
    {
        const string custom = "urn:field-test";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:original='{custom}'>" +
            "<rdf:Description><p:Qualified rdf:value='content' original:Source='source'/></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        registry.RegisterNamespace(custom, "registered");
        Assert.Equal("source", packet.GetQualifier(registry, Xmp, "Qualified", custom, "Source")!.Value);
    }

    [Fact]
    public void ScopedIterationMatchesJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-iteration-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        var expected = new List<(string Path, string? Value)>();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            if (fields[0] == "node")
                expected.Add((Decode(fields[3]), fields[4] == "-" ? null : Decode(fields[4])));
            else
            {
                var path = Decode(fields[1]);
                var options = (XmpIterationOptions)int.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                var actual = packet.IterateProperty(registry, Xmp, path, options)
                    .Select(entry => (entry.Path, Value: (string?)(entry.Node.Value ?? ""))).ToList();
                Assert.Equal(expected, actual);
                expected.Clear();
            }
        }
    }

    [Fact]
    public void PropertyIteratorSkipsSubtreesAndSiblingLists()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource' xml:lang='en'>" +
            "<p:First rdf:parseType='Resource'><p:Nested>value</p:Nested></p:First>" +
            "<p:Second>two</p:Second></p:Root></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();

        using (var iterator = packet.CreatePropertyIterator(registry, Xmp, "Root"))
        {
            Assert.True(iterator.MoveNext());
            Assert.Equal("xmp:Root", iterator.Current.Path);
            Assert.True(iterator.MoveNext());
            Assert.Equal("xmp:Root/xmp:First", iterator.Current.Path);
            iterator.SkipSubtree();
            Assert.True(iterator.MoveNext());
            Assert.Equal("xmp:Root/xmp:Second", iterator.Current.Path);
            Assert.True(iterator.MoveNext());
            Assert.Equal("xmp:Root/xml:lang", iterator.Current.Path);
            Assert.False(iterator.MoveNext());
        }

        using (var iterator = packet.CreatePropertyIterator(registry, Xmp, "Root"))
        {
            Assert.True(iterator.MoveNext());
            Assert.True(iterator.MoveNext());
            iterator.SkipSiblings();
            Assert.True(iterator.MoveNext());
            Assert.Equal("xmp:Root/xml:lang", iterator.Current.Path);
            Assert.False(iterator.MoveNext());
        }

        using (var iterator = packet.CreatePropertyIterator(registry, Xmp, "Root"))
        {
            Assert.True(iterator.MoveNext());
            iterator.SkipSiblings();
            Assert.False(iterator.MoveNext());
        }
    }

    [Fact]
    public void PropertyIteratorHonorsOptionsAndValidatesCursorState()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource'>" +
            "<p:First rdf:parseType='Resource'><p:Nested>value</p:Nested></p:First>" +
            "<p:Second xml:lang='en'>two</p:Second></p:Root></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        foreach (var options in new[] { XmpIterationOptions.None, XmpIterationOptions.JustChildren,
                     XmpIterationOptions.JustLeafNodes, XmpIterationOptions.JustLeafName,
                     XmpIterationOptions.OmitQualifiers, XmpIterationOptions.JustChildren | XmpIterationOptions.JustLeafNodes })
        {
            using var iterator = packet.CreatePropertyIterator(registry, Xmp, "Root", options);
            var actual = new List<XmpIterationEntry>();
            while (iterator.MoveNext()) actual.Add(iterator.Current);
            Assert.Equal(packet.IterateProperty(registry, Xmp, "Root", options)
                    .Select(entry => (entry.Path, entry.Node.Value)),
                actual.Select(entry => (entry.Path, entry.Node.Value)));
        }
        using var cursor = packet.CreatePropertyIterator(registry, Xmp, "Root", XmpIterationOptions.JustLeafNodes);
        Assert.Throws<InvalidOperationException>(() => cursor.SkipSubtree());
        Assert.Throws<InvalidOperationException>(() => cursor.Current);
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:First/xmp:Nested", cursor.Current.Path);
        cursor.SkipSiblings();
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Second", cursor.Current.Path);
        cursor.SkipSubtree();
        Assert.False(cursor.MoveNext());
        Assert.Throws<InvalidOperationException>(() => cursor.SkipSiblings());
        Assert.Throws<NotSupportedException>(() => cursor.Reset());
        cursor.Dispose();
        Assert.Throws<ObjectDisposedException>(() => cursor.MoveNext());
        using var missing = packet.CreatePropertyIterator(registry, Xmp, "Missing");
        Assert.False(missing.MoveNext());
        Assert.Throws<ArgumentNullException>(() => packet.CreatePropertyIterator(null!, Xmp, "Root"));
        Assert.Throws<ArgumentOutOfRangeException>(() => packet.CreatePropertyIterator(registry, Xmp, "Root", (XmpIterationOptions)16));
    }

    [Fact]
    public void PropertyIteratorSkipsArraySiblingsAndQualifiers()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource'>" +
            "<p:Array><rdf:Seq><rdf:li xml:lang='en'>one</rdf:li><rdf:li>two</rdf:li></rdf:Seq></p:Array>" +
            "<p:Qualified rdf:value='value' xml:lang='fr' p:source='book'/></p:Root></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        using var cursor = packet.CreatePropertyIterator(registry, Xmp, "Root");
        Assert.True(cursor.MoveNext());
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Array", cursor.Current.Path);
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Array[1]", cursor.Current.Path);
        cursor.SkipSiblings();
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Qualified", cursor.Current.Path);
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Qualified/xml:lang", cursor.Current.Path);
        cursor.SkipSiblings();
        Assert.False(cursor.MoveNext());

        using var children = packet.CreatePropertyIterator(registry, Xmp, "Root", XmpIterationOptions.JustChildren);
        Assert.True(children.MoveNext());
        children.SkipSiblings();
        Assert.False(children.MoveNext());
    }

    [Fact]
    public void SkippedSubtreeDoesNotResolveItsNamespaces()
    {
        const string custom = "urn:skipped-iteration";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='{custom}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource'>" +
            "<p:Skipped rdf:parseType='Resource'><c:Field>v</c:Field></p:Skipped>" +
            "<p:Next>next</p:Next></p:Root></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        using var cursor = packet.CreatePropertyIterator(registry, Xmp, "Root");
        Assert.True(cursor.MoveNext());
        Assert.True(cursor.MoveNext());
        cursor.SkipSubtree();
        Assert.True(cursor.MoveNext());
        Assert.Equal("xmp:Root/xmp:Next", cursor.Current.Path);
        Assert.False(cursor.MoveNext());
        Assert.Equal(XmpError.BadSchema,
            Assert.Throws<XmpException>(() => packet.IterateProperty(registry, Xmp, "Root").ToList()).Error);
    }

    [Fact]
    public void SchemaIterationMatchesJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-schema-iteration-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        var expected = new List<(string? Namespace, string? Path, string? Value, bool IsSchema)>();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            string? Get(int index) => fields[index] == "-" ? null : Decode(fields[index]);
            if (fields[0] == "node")
                expected.Add((Get(3), Get(4), Get(5), bool.Parse(fields[6])));
            else
            {
                var options = (XmpIterationOptions)int.Parse(fields[2], System.Globalization.CultureInfo.InvariantCulture);
                using var cursor = packet.CreateIterator(registry, Get(1), options);
                var cursorEntries = new List<XmpMetadataIterationEntry>();
                while (cursor.MoveNext()) cursorEntries.Add(cursor.Current);
                var actual = cursorEntries
                    .Select(entry => (entry.NamespaceUri, entry.Path,
                        entry.IsSchema ? null : entry.Node!.Value ?? "", entry.IsSchema)).ToList();
                Assert.Equal(expected, actual);
                Assert.Equal(actual, packet.Iterate(registry, Get(1), options)
                    .Select(entry => (entry.NamespaceUri, entry.Path,
                        entry.IsSchema ? null : entry.Node!.Value ?? "", entry.IsSchema)));
                expected.Clear();
            }
        }
    }

    [Fact]
    public void MetadataIteratorSkipsSchemasPropertiesAndNestedSiblings()
    {
        const string dc = "http://purl.org/dc/elements/1.1/";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:dc='{dc}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource' xml:lang='en'>" +
            "<p:First>one</p:First><p:Second>two</p:Second></p:Root>" +
            "<p:Other>other</p:Other><dc:format>pdf</dc:format></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();

        using (var cursor = packet.CreateIterator(registry))
        {
            Assert.True(cursor.MoveNext());
            Assert.True(cursor.Current.IsSchema);
            cursor.SkipSubtree();
            Assert.True(cursor.MoveNext());
            Assert.Equal(dc, cursor.Current.NamespaceUri);
            Assert.True(cursor.Current.IsSchema);
            Assert.True(cursor.MoveNext());
            Assert.Equal("dc:format", cursor.Current.Path);
            Assert.False(cursor.MoveNext());
        }
        using (var cursor = packet.CreateIterator(registry))
        {
            Assert.True(cursor.MoveNext());
            cursor.SkipSiblings();
            Assert.False(cursor.MoveNext());
        }
        using (var cursor = packet.CreateIterator(registry))
        {
            Assert.True(cursor.MoveNext());
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Root", cursor.Current.Path);
            cursor.SkipSiblings();
            Assert.True(cursor.MoveNext());
            Assert.Equal(dc, cursor.Current.NamespaceUri);
            Assert.True(cursor.Current.IsSchema);
        }
        using (var cursor = packet.CreateIterator(registry, Xmp))
        {
            Assert.True(cursor.MoveNext());
            Assert.True(cursor.MoveNext());
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Root/xmp:First", cursor.Current.Path);
            cursor.SkipSiblings();
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Root/xml:lang", cursor.Current.Path);
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Other", cursor.Current.Path);
            Assert.False(cursor.MoveNext());
        }
    }

    [Fact]
    public void MetadataIteratorHandlesOptionsCursorStateAndLazySkips()
    {
        const string custom = "urn:metadata-iteration-skipped";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='{custom}'>" +
            "<rdf:Description><p:Root rdf:parseType='Resource'><p:First>one</p:First>" +
            "<p:Second>two</p:Second></p:Root><p:Other>other</p:Other>" +
            "<c:Unregistered>value</c:Unregistered></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        using (var cursor = packet.CreateIterator(registry, options: XmpIterationOptions.JustChildren))
        {
            Assert.Throws<InvalidOperationException>(() => cursor.SkipSubtree());
            Assert.Throws<InvalidOperationException>(() => cursor.Current);
            Assert.True(cursor.MoveNext());
            Assert.True(cursor.Current.IsSchema);
            cursor.SkipSiblings();
            Assert.False(cursor.MoveNext());
            Assert.False(cursor.MoveNext());
            Assert.Throws<InvalidOperationException>(() => cursor.SkipSiblings());
            Assert.Throws<NotSupportedException>(() => cursor.Reset());
            cursor.Dispose();
            Assert.Throws<ObjectDisposedException>(() => cursor.MoveNext());
        }
        using (var cursor = packet.CreateIterator(registry, Xmp, XmpIterationOptions.JustChildren))
        {
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Root", cursor.Current.Path);
            cursor.SkipSiblings();
            Assert.False(cursor.MoveNext());
        }
        using (var cursor = packet.CreateIterator(registry, Xmp, XmpIterationOptions.JustLeafNodes))
        {
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Root/xmp:First", cursor.Current.Path);
            cursor.SkipSiblings();
            Assert.True(cursor.MoveNext());
            Assert.Equal("xmp:Other", cursor.Current.Path);
            Assert.False(cursor.MoveNext());
        }
        using (var cursor = packet.CreateIterator(registry))
        {
            Assert.True(cursor.MoveNext());
            cursor.SkipSubtree();
            Assert.True(cursor.MoveNext());
            Assert.Equal(custom, cursor.Current.NamespaceUri);
            cursor.SkipSubtree();
            Assert.False(cursor.MoveNext());
        }
        using var missing = packet.CreateIterator(registry, "urn:missing");
        Assert.False(missing.MoveNext());
        Assert.Throws<ArgumentNullException>(() => packet.CreateIterator(null!));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.CreateIterator(registry, "")).Error);
        Assert.Throws<ArgumentOutOfRangeException>(() => packet.CreateIterator(registry, options: (XmpIterationOptions)16));
    }

    [Fact]
    public void SchemaIterationValidatesInputsAndHandlesEmptyPackets()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}'/>");
        var registry = new XmpSchemaRegistry();
        Assert.Empty(packet.Iterate(registry));
        Assert.Empty(packet.Iterate(registry, Xmp));
        Assert.Throws<ArgumentNullException>(() => packet.Iterate(null!));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.Iterate(registry, "")).Error);
        Assert.Throws<ArgumentOutOfRangeException>(() => packet.Iterate(registry, options: (XmpIterationOptions)16));
    }

    [Fact]
    public void ScopedIterationValidatesInputsAndRequiresRegisteredNamespaces()
    {
        const string custom = "urn:iteration";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='{custom}'>" +
            "<rdf:Description><p:Struct rdf:parseType='Resource'><c:Field>v</c:Field></p:Struct></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        Assert.Throws<ArgumentNullException>(() => packet.IterateProperty(null!, Xmp, "Struct"));
        Assert.Throws<ArgumentOutOfRangeException>(() => packet.IterateProperty(registry, Xmp, "Struct", (XmpIterationOptions)16));
        Assert.Equal(XmpError.BadParameter, Assert.Throws<XmpException>(() => packet.IterateProperty(registry, Xmp, "")).Error);
        Assert.Equal(XmpError.BadSchema, Assert.Throws<XmpException>(() => packet.IterateProperty(registry, Xmp, "Struct").ToList()).Error);
        registry.RegisterNamespace(custom, "custom");
        var items = packet.IterateProperty(registry, Xmp, "Struct").ToList();
        Assert.Equal(new[] { "xmp:Struct", "xmp:Struct/custom:Field" }, items.Select(item => item.Path));
        Assert.Equal("v", items[1].Node.Value);
    }

    [Fact]
    public void PathLookupResolvesCustomNamespacesAndDoesNotMutateAliases()
    {
        const string custom = "urn:custom";
        const string dc = "http://purl.org/dc/elements/1.1/";
        const string pdf = "http://ns.adobe.com/pdf/1.3/";
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:c='{custom}' xmlns:dc='{dc}'>" +
            "<rdf:Description><p:Struct rdf:parseType='Resource'><c:Field>v</c:Field></p:Struct>" +
            "<dc:title><rdf:Alt><rdf:li xml:lang='fr'>Titre</rdf:li></rdf:Alt></dc:title></rdf:Description></rdf:RDF>");
        var registry = new XmpSchemaRegistry();
        registry.RegisterNamespace(custom, "test");
        Assert.Equal("v", packet.GetPropertyByPath(registry, Xmp, "Struct/test:Field")!.Value);
        Assert.Null(packet.GetPropertyByPath(registry, pdf, "Title"));
        Assert.Single(packet.GetProperty(dc, "title")!.Children);
        Assert.Null(packet.GetPropertyByPath(registry, Xmp, "Struct/test:Missing"));
    }

    [Fact]
    public void LocalizedTextMatchesJavaReferenceWithoutMutatingTree()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-localized-reference.tsv"));
        var packet = Parse(Decode(lines[0].Split('\t')[1]));
        var registry = new XmpSchemaRegistry();
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            var path = Decode(fields[0]);
            var generic = fields[1] == "-" ? null : Decode(fields[1]);
            var specific = Decode(fields[2]);
            if (fields[3] == "error")
            {
                var exception = Assert.Throws<XmpException>(() => packet.GetLocalizedText(registry, Xmp, path, generic, specific));
                Assert.Equal(int.Parse(fields[4]), (int)exception.Error);
            }
            else if (fields[3] == "missing")
                Assert.Null(packet.GetLocalizedText(registry, Xmp, path, generic, specific));
            else
            {
                var item = Assert.IsType<XmpNode>(packet.GetLocalizedText(registry, Xmp, path, generic, specific));
                Assert.Equal(Decode(fields[4]), item.Value);
                Assert.Equal(Decode(fields[5]), item.Language);
            }
        }
        Assert.Equal("Original", packet.GetProperty(Xmp, "Moved")!.Children[0].Value);
    }

    [Fact]
    public void ReadsStructuresAndMultipleDescriptions()
    {
        var packet = Parse($"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            "<rdf:Description rdf:about='id'><p:S><rdf:Description><p:Field>v</p:Field></rdf:Description></p:S></rdf:Description>" +
            "<rdf:Description rdf:about='id'><p:Bag><rdf:Bag><rdf:li>one</rdf:li></rdf:Bag></p:Bag></rdf:Description></rdf:RDF>");
        Assert.Equal("id", packet.About);
        Assert.Equal("v", Assert.Single(packet.GetProperty(Xmp, "S")!.Children).Value);
        Assert.Equal(XmpNodeKind.UnorderedArray, packet.GetProperty(Xmp, "Bag")!.Kind);
        Assert.Null(packet.GetProperty(Xmp, "missing"));
    }

    [Theory]
    [InlineData("rdf:ID='top'")]
    [InlineData("rdf:nodeID='top'")]
    public void ReadsNodeIdentityWithoutTreatingItAsMetadata(string identity)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>" +
            $"<rdf:Description {identity} p:Label='label'>" +
            "<p:Struct><rdf:Description rdf:nodeID='inner' p:Field='field'/></p:Struct>" +
            "<p:Typed><p:Type rdf:ID='inner'><p:Field>typed</p:Field></p:Type></p:Typed>" +
            "<p:Bag><rdf:Bag rdf:about='ignored'><rdf:li>item</rdf:li></rdf:Bag></p:Bag>" +
            "</rdf:Description><rdf:Description rdf:about='subject' p:Other='other'/></rdf:RDF>";
        var packet = Parse(xml);
        Assert.Equal("subject", packet.About);
        Assert.Equal("label", packet.GetProperty(Xmp, "Label")!.Value);
        Assert.Equal("field", Assert.Single(packet.GetProperty(Xmp, "Struct")!.Children).Value);
        var typed = packet.GetProperty(Xmp, "Typed")!;
        Assert.Equal("typed", Assert.Single(typed.Children).Value);
        Assert.Equal(Xmp + ":Type", Assert.Single(typed.Qualifiers).Value);
        Assert.Equal("item", Assert.Single(packet.GetProperty(Xmp, "Bag")!.Children).Value);
        using var output = new MemoryStream();
        XmpPacketWriter.Write(packet, output);
        output.Position = 0;
        var copy = XmpPacket.Parse(output);
        Assert.Equal("subject", copy.About);
        Assert.Equal("field", Assert.Single(copy.GetProperty(Xmp, "Struct")!.Children).Value);
        Assert.Equal("item", Assert.Single(copy.GetProperty(Xmp, "Bag")!.Children).Value);
    }

    [Theory]
    [InlineData("rdf:about='' rdf:ID='id'")]
    [InlineData("rdf:about='subject' rdf:nodeID='id'")]
    [InlineData("rdf:ID='id' rdf:nodeID='node'")]
    public void RejectsConflictingNodeIdentitiesAtEveryLevel(string identities)
    {
        var prefix = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'>";
        var documents = new[]
        {
            $"{prefix}<rdf:Description {identities}/></rdf:RDF>",
            $"{prefix}<rdf:Description><p:S><rdf:Description {identities}/></p:S></rdf:Description></rdf:RDF>",
            $"{prefix}<rdf:Description><p:S><p:Type {identities}/></p:S></rdf:Description></rdf:RDF>",
            $"{prefix}<rdf:Description><p:A><rdf:Seq {identities}/></p:A></rdf:Description></rdf:RDF>"
        };
        foreach (var document in documents)
            Assert.Equal(XmpError.BadRdf, Assert.Throws<XmpException>(() => Parse(document)).Error);
    }

    [Theory]
    [InlineData("<p:one unsupported='x'/>")]
    [InlineData("<p:one>mixed<rdf:Bag/></p:one>")]
    [InlineData("<p:one><![CDATA[value]]></p:one>")]
    public void UnsupportedConstructsFailExplicitly(string property)
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>{property}</rdf:Description></rdf:RDF>";
        Assert.Throws<NotSupportedException>(() => Parse(xml));
    }

    [Fact]
    public void RejectsDuplicatePropertiesAndMismatchedSubjects()
    {
        var duplicate = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='1'><p:A>2</p:A></rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() => Parse(duplicate)).Error);
        var mismatch = $"<rdf:RDF xmlns:rdf='{Rdf}'><rdf:Description rdf:about='a'/><rdf:Description rdf:about='b'/></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() => Parse(mismatch)).Error);
    }

    [Fact]
    public void RejectsDtdAndMalformedXmlWithoutClosingStream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE a [<!ENTITY x SYSTEM 'file:///secret'>]><a>&x;</a>"));
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => XmpPacket.Parse(stream)).Error);
        Assert.True(stream.CanRead);
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => Parse("<broken>")).Error);
    }

    [Fact]
    public void RejectsInvalidArrayItems()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description><p:A><rdf:Bag><p:item>v</p:item></rdf:Bag></p:A></rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadRdf, Assert.Throws<XmpException>(() => Parse(xml)).Error);
    }

    [Fact]
    public void ReadsAttributeStructuresAndQualifiedValues()
    {
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>" +
            "<p:S p:A='one' p:B='two'/><p:Q rdf:value='hello' p:source='test' xml:lang='EN_us'/>" +
            "<p:U rdf:resource='urn:test' p:source='s'/>" +
            "<p:N><rdf:Description p:extra='a'><rdf:value xml:lang='fr'>nested</rdf:value><p:source>book</p:source></rdf:Description></p:N>" +
            "<p:T rdf:parseType='Resource' xml:lang='en'><p:F>v</p:F></p:T>" +
            "</rdf:Description></rdf:RDF>";
        var packet = Parse(xml);
        var structure = packet.GetProperty(Xmp, "S")!;
        Assert.Equal(XmpNodeKind.Structure, structure.Kind);
        Assert.Equal(new[] { "one", "two" }, structure.Children.Select(n => n.Value));
        var q = packet.GetProperty(Xmp, "Q")!;
        Assert.Equal("hello", q.Value);
        Assert.Equal("en-US", q.Language);
        Assert.Equal("test", Assert.Single(q.Qualifiers).Value);
        var uri = packet.GetProperty(Xmp, "U")!;
        Assert.Equal(XmpNodeKind.Uri, uri.Kind);
        Assert.Equal("s", Assert.Single(uri.Qualifiers).Value);
        var nested = packet.GetProperty(Xmp, "N")!;
        Assert.Equal("nested", nested.Value);
        Assert.Equal("fr", nested.Language);
        Assert.Equal(new[] { "a", "book" }, nested.Qualifiers.Select(n => n.Value));
        Assert.Equal("en", packet.GetProperty(Xmp, "T")!.Language);
    }

    [Fact]
    public void RejectsInvalidQualifiedValues()
    {
        var prefix = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description>";
        const string suffix = "</rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() =>
            Parse(prefix + "<p:A rdf:value='a' rdf:resource='b'/>" + suffix)).Error);
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() =>
            Parse(prefix + "<p:A xml:lang='en' rdf:parseType='Resource'><rdf:value xml:lang='fr'>v</rdf:value></p:A>" + suffix)).Error);
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() =>
            Parse(prefix + "<p:A rdf:parseType='Collection'/>" + suffix)).Error);
    }

    [Fact]
    public void NormalizesLegacyDublinCoreNamesBeforeDuplicateChecks()
    {
        const string dc = "http://purl.org/dc/elements/1.1/";
        var xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:old='http://purl.org/dc/1.1/' xmlns:dc='{dc}'>" +
            "<rdf:Description><old:title>a</old:title><dc:title>b</dc:title></rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() => Parse(xml)).Error);
        xml = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}' xmlns:old='http://purl.org/dc/1.1/' xmlns:dc='{dc}'>" +
            "<rdf:Description><p:Q rdf:value='v' old:title='a' dc:title='b'/></rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() => Parse(xml)).Error);
    }

    [Fact]
    public void ReadsXmlEncodingFromStream()
    {
        var text = $"<?xml version='1.0' encoding='utf-16'?><rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='hello'/></rdf:RDF>";
        using var stream = new MemoryStream(Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(text)).ToArray());
        Assert.Equal("hello", XmpPacket.Parse(stream).GetProperty(Xmp, "A")!.Value);
    }

    [Fact]
    public void DiscoveryMatchesJavaReference()
    {
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-discovery-reference.tsv")))
        {
            var fields = line.Split('\t');
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Decode(fields[1])));
            var packet = XmpPacket.ParseDiscovered(stream, bool.Parse(fields[0]));
            Assert.Equal(fields[2] == "-" ? null : Decode(fields[2]), packet.GetProperty(Xmp, "Label")?.Value);
            Assert.Equal(fields[3] == "-" ? null : Decode(fields[3]), packet.PacketHeader);
            Assert.True(stream.CanRead);
        }
    }

    [Fact]
    public void DiscoveryRejectsMalformedXmlAndDtdWithoutClosingStream()
    {
        using var malformed = new MemoryStream(Encoding.UTF8.GetBytes("<broken>"));
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(malformed)).Error);
        Assert.True(malformed.CanRead);
        using var dtd = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE a [<!ENTITY x SYSTEM 'file:///secret'>]><a>&x;</a>"));
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(dtd)).Error);
        Assert.True(dtd.CanRead);
    }

    [Fact]
    public void StrictParsingStillRejectsMissingOrNestedRoots()
    {
        Assert.Throws<NotSupportedException>(() => Parse("<document/>"));
        Assert.Throws<NotSupportedException>(() => Parse($"<document><rdf:RDF xmlns:rdf='{Rdf}'/></document>"));
    }

    [Fact]
    public void DiscoveryHandlesDeeplyNestedDocuments()
    {
        const int depth = 5000;
        var xml = string.Concat(Enumerable.Repeat("<a>", depth)) +
            $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:Label='deep'/></rdf:RDF>" +
            string.Concat(Enumerable.Repeat("</a>", depth));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        Assert.Equal("deep", XmpPacket.ParseDiscovered(stream).GetProperty(Xmp, "Label")!.Value);
    }

    [Fact]
    public void StringControlRecoveryMatchesJavaReference()
    {
        foreach (var line in File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-control-reference.tsv")))
        {
            var fields = line.Split('\t');
            var input = Decode(fields[1]);
            var fix = bool.Parse(fields[0]);
            if (fields[2] == "error")
            {
                var exception = Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(input, fixControlCharacters: fix));
                Assert.Equal(int.Parse(fields[3]), (int)exception.Error);
            }
            else
                Assert.Equal(Decode(fields[3]), XmpPacket.ParseDiscovered(input, fixControlCharacters: fix).GetProperty(Xmp, "Label")!.Value);
        }
    }

    [Fact]
    public void StringRecoveryDoesNotPermitDtdsOrMaskRdfErrors()
    {
        const string dtd = "<!DOCTYPE a [<!ENTITY x SYSTEM 'file:///secret'>]><a>&x;</a>";
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(dtd, fixControlCharacters: true)).Error);
        var duplicate = $"<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='1'><p:A>2</p:A></rdf:Description></rdf:RDF>";
        Assert.Equal(XmpError.BadXmp, Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(duplicate, fixControlCharacters: true)).Error);
    }

    [Fact]
    public void StreamRecoveryMatchesJavaReference()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "xmp-stream-recovery-reference.tsv"));
        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var fields = lines[lineIndex].Split('\t');
            using var stream = new MemoryStream(Convert.FromBase64String(fields[2]));
            var options = new XmpRecoveryOptions { FixControlCharacters = bool.Parse(fields[0]), AcceptLatin1 = bool.Parse(fields[1]) };
            if (fields[3] == "error")
            {
                var result = Record.Exception(() => XmpPacket.ParseDiscovered(stream, options));
                Assert.True(result is XmpException, $"Expected XMP error for fixture {lineIndex}; got {result}");
                var exception = Assert.IsType<XmpException>(result);
                Assert.Equal(int.Parse(fields[4]), (int)exception.Error);
            }
            else
                Assert.Equal(Decode(fields[4]), XmpPacket.ParseDiscovered(stream, options).GetProperty(Xmp, "Label")!.Value);
            Assert.True(stream.CanRead);
        }
    }

    [Fact]
    public void StreamRecoveryRejectsDtdFromCurrentPosition()
    {
        var text = "prefix<!DOCTYPE a [<!ENTITY x SYSTEM 'file:///secret'>]><a>&x;</a>";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        stream.Position = 6;
        var options = new XmpRecoveryOptions { FixControlCharacters = true, AcceptLatin1 = true };
        Assert.Equal(XmpError.BadXml, Assert.Throws<XmpException>(() => XmpPacket.ParseDiscovered(stream, options)).Error);
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void StreamRecoveryHandlesNonSeekableStreamsFromCurrentPosition()
    {
        var xml = $"prefix<rdf:RDF xmlns:rdf='{Rdf}' xmlns:p='{Xmp}'><rdf:Description p:A='raw\u0001text'/></rdf:RDF>";
        using var source = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        source.Position = 6;
        using var stream = new NonSeekableStream(source);
        var options = new XmpRecoveryOptions { FixControlCharacters = true };
        Assert.Equal("raw text", XmpPacket.ParseDiscovered(stream, options).GetProperty(Xmp, "A")!.Value);
        Assert.True(stream.CanRead);
    }

    private sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private static XmpPacket Parse(string value)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
        return XmpPacket.Parse(stream);
    }

    private static string Decode(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
}
