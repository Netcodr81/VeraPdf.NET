using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VeraPdf.Library.Core;

public static class XmlSerialiser
{
    public static T TypeFromXml<T>(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var textReader = new StringReader(source);
        return TypeFromXml<T>(textReader);
    }

    public static T TypeFromXml<T>(TextReader source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return Deserialize<T>(reader);
    }

    public static T TypeFromXml<T>(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return Deserialize<T>(reader);
    }

    public static T TypeFromXml<T>(FileInfo source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var stream = source.OpenRead();
        return TypeFromXml<T>(stream);
    }

    public static string ToXml<T>(T value, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var textWriter = new StringWriter();
        using (var writer = XmlWriter.Create(textWriter, CreateWriterSettings(format, fragment)))
        {
            new XmlSerializer(typeof(T)).Serialize(writer, value);
        }

        return textWriter.ToString();
    }

    public static void ToXml<T>(T value, Stream destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(destination);
        using var writer = XmlWriter.Create(destination, CreateWriterSettings(format, fragment));
        new XmlSerializer(typeof(T)).Serialize(writer, value);
    }

    public static void ToXml<T>(T value, TextWriter destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(destination);
        using var writer = XmlWriter.Create(destination, CreateWriterSettings(format, fragment));
        new XmlSerializer(typeof(T)).Serialize(writer, value);
    }

    public static void ToXml<T>(T value, FileInfo destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(destination);
        using var stream = destination.Create();
        ToXml(value, stream, format, fragment);
    }

    public static IReadOnlyList<T> CollectionFromXml<T>(string source, params Type[] knownTypes)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var textReader = new StringReader(source);
        return CollectionFromXml<T>(textReader, knownTypes);
    }

    public static IReadOnlyList<T> CollectionFromXml<T>(TextReader source, params Type[] knownTypes)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return DeserializeCollection<T>(reader, CreateItemSerializers<T>(knownTypes));
    }

    public static IReadOnlyList<T> CollectionFromXml<T>(Stream source, params Type[] knownTypes)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var reader = XmlReader.Create(source, CreateReaderSettings());
        return DeserializeCollection<T>(reader, CreateItemSerializers<T>(knownTypes));
    }

    public static IReadOnlyList<T> CollectionFromXml<T>(FileInfo source, params Type[] knownTypes)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var stream = source.OpenRead();
        return CollectionFromXml<T>(stream, knownTypes);
    }

    public static string ToXml<T>(string rootName, IEnumerable<T> items, bool format, bool fragment)
    {
        using var textWriter = new StringWriter();
        using (var writer = XmlWriter.Create(textWriter, CreateWriterSettings(format, fragment)))
        {
            SerializeCollection(rootName, items, writer, format);
        }

        return textWriter.ToString();
    }

    public static void ToXml<T>(string rootName, IEnumerable<T> items, Stream destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(destination);
        using var writer = XmlWriter.Create(destination, CreateWriterSettings(format, fragment));
        SerializeCollection(rootName, items, writer, format);
    }

    public static void ToXml<T>(string rootName, IEnumerable<T> items, TextWriter destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(destination);
        using var writer = XmlWriter.Create(destination, CreateWriterSettings(format, fragment));
        SerializeCollection(rootName, items, writer, format);
    }

    public static void ToXml<T>(string rootName, IEnumerable<T> items, FileInfo destination, bool format, bool fragment)
    {
        ArgumentNullException.ThrowIfNull(destination);
        using var stream = destination.Create();
        ToXml(rootName, items, stream, format, fragment);
    }

    public static string Schema(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        using var writer = new StringWriter();
        Schema(value, writer);
        return writer.ToString();
    }

    public static void Schema(object value, TextWriter destination)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(destination);
        CreateSchema(value.GetType()).Write(destination);
    }

    public static void Schema(object value, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(destination);
        CreateSchema(value.GetType()).Write(destination);
    }

    public static IReadOnlyDictionary<string, string> SchemaDocuments(object value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var documents = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var schema in CreateSchemas(value.GetType()))
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            schema.Write(writer);
            documents.Add(schema.TargetNamespace ?? string.Empty, writer.ToString());
        }

        return new ReadOnlyDictionary<string, string>(documents);
    }

    private static XmlReaderSettings CreateReaderSettings() => new()
    {
        CloseInput = false,
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null
    };

    private static XmlWriterSettings CreateWriterSettings(bool format, bool fragment) => new()
    {
        CloseOutput = false,
        Indent = format,
        OmitXmlDeclaration = fragment
    };

    private static T Deserialize<T>(XmlReader reader) =>
        (T)(new XmlSerializer(typeof(T)).Deserialize(reader)
            ?? throw new InvalidOperationException($"XML did not contain a value of type '{typeof(T)}'."));

    private static IReadOnlyList<T> DeserializeCollection<T>(XmlReader reader, IReadOnlyList<XmlSerializer> serializers)
    {
        reader.MoveToContent();
        if (reader.NodeType != XmlNodeType.Element)
        {
            throw new XmlException("Expected a collection root element.");
        }

        var items = new List<T>();
        if (reader.IsEmptyElement)
        {
            reader.Read();
            return items;
        }

        reader.ReadStartElement();
        while (reader.MoveToContent() == XmlNodeType.Element)
        {
            object? item;
            using (var itemReader = reader.ReadSubtree())
            {
                itemReader.MoveToContent();
                var serializer = serializers.FirstOrDefault(candidate => candidate.CanDeserialize(itemReader))
                    ?? throw new InvalidOperationException(
                        $"No registered XML type can deserialize collection item '{itemReader.Name}'.");
                item = serializer.Deserialize(itemReader);
                if (item is not T)
                {
                    throw new InvalidOperationException($"Collection item is not of type '{typeof(T)}'.");
                }
            }

            items.Add((T)item);
            reader.Read();
        }

        reader.ReadEndElement();
        return items;
    }

    private static void SerializeCollection<T>(string rootName, IEnumerable<T> items, XmlWriter writer, bool format)
    {
        ArgumentException.ThrowIfNullOrEmpty(rootName);
        ArgumentNullException.ThrowIfNull(items);
        XmlConvert.VerifyNCName(rootName);
        writer.WriteStartElement(rootName);
        foreach (var item in items)
        {
            if (item is null)
            {
                continue;
            }

            var serializer = new XmlSerializer(item.GetType());
            using var itemStream = new MemoryStream();
            using (var itemWriter = XmlWriter.Create(itemStream, CreateWriterSettings(format: false, fragment: true)))
            {
                serializer.Serialize(itemWriter, item);
            }

            itemStream.Position = 0;
            using var itemReader = XmlReader.Create(itemStream, CreateReaderSettings());
            itemReader.MoveToContent();
            writer.WriteNode(itemReader, defattr: true);
        }

        writer.WriteEndElement();
    }

    private static IReadOnlyList<XmlSerializer> CreateItemSerializers<T>(IEnumerable<Type> knownTypes)
    {
        ArgumentNullException.ThrowIfNull(knownTypes);
        var itemTypes = new HashSet<Type> { typeof(T) };
        foreach (var knownType in knownTypes)
        {
            ArgumentNullException.ThrowIfNull(knownType);
            if (!typeof(T).IsAssignableFrom(knownType))
            {
                throw new ArgumentException($"Known type '{knownType}' is not assignable to '{typeof(T)}'.", nameof(knownTypes));
            }

            itemTypes.Add(knownType);
        }

        return itemTypes.Select(type => new XmlSerializer(type)).ToArray();
    }

    private static XmlSchema CreateSchema(Type type)
    {
        var schemas = CreateSchemas(type);
        if (schemas.Count != 1)
        {
            throw new NotSupportedException("Use SchemaDocuments for types using multiple XML namespaces.");
        }

        return schemas[0];
    }

    private static IReadOnlyList<XmlSchema> CreateSchemas(Type type)
    {
        var schemas = new XmlSchemas();
        var importer = new XmlReflectionImporter();
        var mapping = importer.ImportTypeMapping(type);
        new XmlSchemaExporter(schemas).ExportTypeMapping(mapping);
        schemas.Compile(null, true);
        return schemas.Cast<XmlSchema>().OrderBy(schema => schema.TargetNamespace, StringComparer.Ordinal).ToArray();
    }
}
