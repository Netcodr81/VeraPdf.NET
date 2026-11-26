using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VeraPdf.Model.BaseLayer;

namespace VeraPdf.Model.Compatibility;

public sealed class ContractParityTests
{
    // Deliberately independent of the generation assembly and its parser/type tables.
    private static readonly IReadOnlyDictionary<string, Type> Scalars = new Dictionary<string, Type>
    {
        ["Integer"] = typeof(long?),
        ["Decimal"] = typeof(double?),
        ["Boolean"] = typeof(bool?),
        ["String"] = typeof(string),
        ["List<String>"] = typeof(IReadOnlyList<string>),
        ["List<List<String>>"] = typeof(IReadOnlyList<IReadOnlyList<string>>),
        ["Set<String>"] = typeof(IReadOnlySet<string>)
    };

    [Fact]
    public void EveryDslDeclarationMatchesTheCompiledContractAndMetadata()
    {
        var assembly = typeof(IModelObject).Assembly;
        var nullability = new NullabilityInfoContext();
        var sources = Inventory();
        var reflected = assembly.GetTypes().Where(t => t.IsInterface && typeof(IModelObject).IsAssignableFrom(t)).ToArray();
        Assert.Equal(sources.Count, reflected.Length);
        Assert.Equal(sources.Select(e => e.Name).Order(), ModelHelper.GetTypes().Order());
        foreach (var entity in sources)
        {
            var definition = ModelHelper.GetDefinition(entity.Name);
            Assert.Equal(entity.Package + "." + entity.Name, definition.JavaName);
            Assert.Equal(entity.Source, definition.Source);
            Assert.Equal(entity.BaseName, definition.BaseName);
            var layer = entity.Source switch
            {
                "BaseLayer.mdl" => "BaseLayer",
                "CosLayer.mdl" => "CosLayer",
                "PDLayer.mdl" => "PDLayer",
                "Operator.mdl" => "Operator",
                "External.mdl" => "External",
                "XMPLayer.mdl" => "XMPLayer",
                "SELayer.mdl" => "SELayer",
                "SALayer.mdl" => "SALayer",
                _ => throw new InvalidOperationException("Unknown layer.")
            };
            var expectedName = $"VeraPdf.Model.{layer}." + (entity.Name == "Object" ? "IModelObject" : "I" + entity.Name);
            Assert.Equal(expectedName, definition.ClrName);
            var contract = assembly.GetType(expectedName, throwOnError: true)!;
            var expectedAncestors = new List<string>();
            var current = entity;
            while (current.BaseName is not null)
            {
                expectedAncestors.Add(current.BaseName);
                current = sources.Single(e => e.Name == current.BaseName);
            }
            Assert.Equal(expectedAncestors, ModelHelper.GetListOfSuperNames(entity.Name));
            Assert.Equal(expectedAncestors.Select(n => ModelHelper.GetDefinition(n).ClrName).Order(),
                contract.GetInterfaces().Select(i => i.FullName!).Order());

            var methods = contract.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            Assert.Empty(contract.GetProperties());
            if (entity.Name != "Object")
            {
                Assert.Equal(entity.Properties.Select(p => "Get" + p.Name).Order(), methods.Select(m => m.Name).Order());
            }
            Assert.Equal(entity.Properties.Select(p => p.Name), definition.Properties.Select(p => p.Name));
            foreach (var property in entity.Properties)
            {
                var method = methods.Single(m => m.Name == "Get" + property.Name);
                Assert.Empty(method.GetParameters());
                Assert.Equal(Scalars[property.Type], method.ReturnType);
                if (!method.ReturnType.IsValueType)
                {
                    Assert.Equal(NullabilityState.Nullable, nullability.Create(method.ReturnParameter).ReadState);
                }
                Assert.Equal(property.Type, definition.Properties.Single(p => p.Name == property.Name).DslType);
                Assert.Equal(property.Type switch
                {
                    "Integer" => "long?",
                    "Decimal" => "double?",
                    "Boolean" => "bool?",
                    "String" => "string?",
                    "List<String>" => "IReadOnlyList<string>?",
                    "List<List<String>>" => "IReadOnlyList<IReadOnlyList<string>>?",
                    "Set<String>" => "IReadOnlySet<string>?",
                    _ => throw new InvalidOperationException("Unexpected scalar.")
                }, definition.Properties.Single(p => p.Name == property.Name).ClrType);
            }
            Assert.Equal(entity.Links.Select(l => l.Name), definition.Links.Select(l => l.Name));
            foreach (var link in entity.Links)
            {
                var mapped = definition.Links.Single(l => l.Name == link.Name);
                Assert.Equal(link.Type.Split('.')[^1], mapped.Target);
                Assert.Equal(link.Suffix, mapped.Multiplicity);
                Assert.DoesNotContain(methods, m => m.Name == "Get" + link.Name);
            }
        }
        Assert.Equal(373, sources.Count);
        Assert.Equal(463, sources.Sum(e => e.Properties.Length));
        Assert.Equal(208, sources.Sum(e => e.Links.Length));
        Assert.DoesNotContain(assembly.GetReferencedAssemblies(), a => a.Name!.StartsWith("VeraPdf.Parser", StringComparison.Ordinal));
    }

    [Fact]
    public void RootApiMatchesPinnedUpstreamGenerator()
    {
        var expected = new Dictionary<string, Type>
        {
            ["GetLinks"] = typeof(IReadOnlyList<string>),
            ["GetLinkedObjects"] = typeof(IReadOnlyList<IModelObject>),
            ["GetSuperTypes"] = typeof(IReadOnlyList<string>),
            ["GetProperties"] = typeof(IReadOnlyList<string>),
            ["GetObjectType"] = typeof(string),
            ["GetID"] = typeof(string),
            ["IsContextDependent"] = typeof(bool?),
            ["GetExtraContext"] = typeof(string),
            ["GetContext"] = typeof(string)
        };
        Assert.Equal(expected.Keys.Order(), typeof(IModelObject).GetMethods().Select(m => m.Name).Order());
        foreach (var pair in expected)
        {
            var method = typeof(IModelObject).GetMethod(pair.Key)!;
            Assert.Equal(pair.Value, method.ReturnType);
            if (pair.Key == "GetLinkedObjects")
            {
                Assert.Equal(typeof(string), Assert.Single(method.GetParameters()).ParameterType);
            }
            else Assert.Empty(method.GetParameters());
            var nullability = new NullabilityInfoContext().Create(method.ReturnParameter);
            if (!pair.Value.IsValueType)
            {
                Assert.Equal(pair.Key is "GetID" or "GetExtraContext" or "GetContext"
                    ? NullabilityState.Nullable : NullabilityState.NotNull, nullability.ReadState);
            }
        }
        Assert.Equal("f8e9f87d98a96784cd78a15229736722bb77eff8", ModelHelper.GeneratorCommit);
    }

    [Fact]
    public void InitialDeclarationBaselineRequiresDeliberateApiReview()
    {
        using var baseline = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Baseline", "declarations.json")));
        foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Schema"), "*.mdl"))
        {
            var canonical = Regex.Replace(WithoutComments(File.ReadAllText(file)), @"\s+", "");
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
            Assert.Equal(baseline.RootElement.GetProperty(Path.GetFileName(file)).GetString(), hash);
        }
        Assert.Equal(8, baseline.RootElement.EnumerateObject().Count());
    }

    private sealed record Member(string Name, string Type, string Suffix);
    private sealed record Entity(string Name, string Package, string Source, string? BaseName, Member[] Properties, Member[] Links);

    private static string WithoutComments(string text) => Regex.Replace(text, @"%[^\r\n]*", "");

    private static List<Entity> Inventory()
    {
        var entities = new List<Entity>();
        foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Schema"), "*.mdl").Order(StringComparer.Ordinal))
        {
            var text = WithoutComments(File.ReadAllText(file));
            var package = Regex.Match(text, @"package\s+([\w.]+)\s*;").Groups[1].Value;
            foreach (Match type in Regex.Matches(text, @"type\s+(\w+)(?:\s+extends\s+([\w.]+))?\s*\{([^}]*)\}"))
            {
                var properties = new List<Member>();
                var links = new List<Member>();
                foreach (Match attribute in Regex.Matches(type.Groups[3].Value, @"(property|link)\s+(\w+)\s*:\s*([\w.<>]+)\s*([?*+]?)\s*;"))
                {
                    var member = new Member(attribute.Groups[2].Value, attribute.Groups[3].Value, attribute.Groups[4].Value);
                    (attribute.Groups[1].Value == "property" ? properties : links).Add(member);
                }
                entities.Add(new Entity(type.Groups[1].Value, package, Path.GetFileName(file),
                    type.Groups[2].Success ? type.Groups[2].Value.Split('.')[^1] : null, properties.ToArray(), links.ToArray()));
            }
        }
        return entities;
    }
}
