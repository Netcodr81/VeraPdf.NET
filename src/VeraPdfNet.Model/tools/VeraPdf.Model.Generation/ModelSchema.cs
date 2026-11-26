using System.Text.Json.Serialization;

namespace VeraPdf.Model.Generation;

public sealed record ModelAttribute(string Name, string Type, string Multiplicity, IReadOnlyList<string> Documentation, int Line = 1);

public sealed record ModelEntity(
    string Package, string Name, string? BaseName, string Source,
    IReadOnlyList<string> Imports, IReadOnlyList<ModelAttribute> Properties,
    IReadOnlyList<ModelAttribute> Links, IReadOnlyList<string> Documentation, int Line = 1)
{
    [JsonIgnore]
    public string JavaName => Package + "." + Name;

    [JsonIgnore]
    public string ClrName => ModelSchema.Namespace(Package) + "." + (Name == "Object" ? "IModelObject" : "I" + Name);
}

public static class ModelSchema
{
    public static string ScalarType(string name) => name switch
    {
        "Integer" => "long?",
        "Decimal" => "double?",
        "Boolean" => "bool?",
        "String" => "string?",
        "List<String>" => "IReadOnlyList<string>?",
        "List<List<String>>" => "IReadOnlyList<IReadOnlyList<string>>?",
        "Set<String>" => "IReadOnlySet<string>?",
        _ => throw new FormatException($"Unsupported scalar type '{name}'.")
    };

    public static string Namespace(string package) => package switch
    {
        "org.verapdf.model.baselayer" => "VeraPdf.Model.BaseLayer",
        "org.verapdf.model.coslayer" => "VeraPdf.Model.CosLayer",
        "org.verapdf.model.pdlayer" => "VeraPdf.Model.PDLayer",
        "org.verapdf.model.operator" => "VeraPdf.Model.Operator",
        "org.verapdf.model.external" => "VeraPdf.Model.External",
        "org.verapdf.model.xmplayer" => "VeraPdf.Model.XMPLayer",
        "org.verapdf.model.selayer" => "VeraPdf.Model.SELayer",
        "org.verapdf.model.salayer" => "VeraPdf.Model.SALayer",
        _ => throw new FormatException($"Unsupported model package '{package}'.")
    };

    public static ModelEntity Resolve(ModelEntity owner, string name, IReadOnlyList<ModelEntity> entities)
    {
        var candidates = entities.Where(e =>
            e.JavaName == name ||
            (e.Name == name && (e.Package == owner.Package ||
             owner.Imports.Contains(e.JavaName, StringComparer.Ordinal) ||
             owner.Imports.Contains(e.Package + ".*", StringComparer.Ordinal)))).ToArray();
        if (candidates.Length != 1)
        {
            throw new FormatException($"{owner.Source}:{owner.Line}: {owner.Name}: unresolved or ambiguous model reference '{name}'.");
        }
        return candidates[0];
    }

    public static void Validate(IReadOnlyList<ModelEntity> entities)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        var clrNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entity in entities)
        {
            // Upstream ModelHelper indexes by simple name, so it must be unique across packages.
            if (!names.Add(entity.Name) || !clrNames.Add(entity.ClrName))
            {
                throw new FormatException($"{entity.Source}:{entity.Line}: duplicate type or C# output collision '{entity.Name}'.");
            }
            var members = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in entity.Properties.Concat(entity.Links))
            {
                if (!members.Add(member.Name))
                {
                    throw new FormatException($"{entity.Source}:{member.Line}: {entity.Name}: duplicate member '{member.Name}'.");
                }
            }
            foreach (var property in entity.Properties)
            {
                try
                {
                    _ = ScalarType(property.Type);
                }
                catch (FormatException error)
                {
                    throw new FormatException($"{entity.Source}:{property.Line}: {error.Message}", error);
                }
                if (property.Name is "Links" or "LinkedObjects" or "SuperTypes" or "Properties" or
                    "ObjectType" or "ID" or "ExtraContext" or "Context")
                {
                    throw new FormatException($"{entity.Source}:{property.Line}: {entity.Name}: root API collision '{property.Name}'.");
                }
            }
            foreach (var link in entity.Links)
            {
                _ = Resolve(entity, link.Type, entities);
            }
            var visited = new HashSet<string>(StringComparer.Ordinal) { entity.JavaName };
            var ancestorMembers = new HashSet<string>(entity.Properties.Select(p => p.Name), StringComparer.Ordinal);
            var current = entity;
            while (current.BaseName is not null)
            {
                current = Resolve(current, current.BaseName, entities);
                if (!visited.Add(current.JavaName))
                {
                    throw new FormatException($"{entity.Source}:{entity.Line}: inheritance cycle at '{current.Name}'.");
                }
                foreach (var property in current.Properties)
                {
                    if (!ancestorMembers.Add(property.Name))
                    {
                        throw new FormatException($"{entity.Source}:{entity.Line}: inherited getter collision '{property.Name}'.");
                    }
                }
            }
        }
        if (entities.Count(e => e.BaseName is null) != 1 ||
            !entities.Any(e => e.Name == "Object" && e.Package == "org.verapdf.model.baselayer" && e.BaseName is null))
        {
            throw new FormatException("The model must have exactly one root: org.verapdf.model.baselayer.Object.");
        }
    }
}
