using VeraPdf.Model.Generation;

namespace VeraPdf.Model.Tests;

public sealed class GenerationTests
{
    private const string Root = "package org.verapdf.model.baselayer; type Object {}";

    [Fact]
    public void ReadsCommentsCollectionsQualifiedReferencesAndSplitBraces()
    {
        var text = """
            package org.verapdf.model.coslayer;
            import org.verapdf.model.baselayer.*;
            % A boolean validation view.
            type Sample extends org.verapdf.model.baselayer.Object
            {
                % Diagnostic argument groups.
                property errors: List<List<String>>;
                property names: Set<String>;
                property value: Boolean;
                link parent: Object?;
                link children: Object*;
                link required: Object+;
                link single: Object;
            }
            """;
        var entities = DslReader.Read(Root, "BaseLayer.mdl")
            .Concat(DslReader.Read(text, "CosLayer.mdl")).ToArray();
        var output = ModelEmitter.Emit(entities);
        var sample = entities[1];
        Assert.Equal(["?", "*", "+", ""], sample.Links.Select(l => l.Multiplicity));
        Assert.Equal(["Diagnostic argument groups."], sample.Properties[0].Documentation);
        Assert.Contains("IReadOnlyList<IReadOnlyList<string>>? Geterrors()", output["CosLayer.g.cs"]);
        Assert.Contains("IReadOnlySet<string>? Getnames()", output["CosLayer.g.cs"]);
        Assert.DoesNotContain("Getchildren()", output["CosLayer.g.cs"]);
    }

    [Fact]
    public void OutputIsIndependentOfInputFileOrderAndLineEndings()
    {
        var sample = "package org.verapdf.model.coslayer;\nimport org.verapdf.model.baselayer.Object;\n% Text <tag> & value\n type Sample extends Object { property x: String; }";
        var root = DslReader.Read(Root, "BaseLayer.mdl");
        var first = ModelEmitter.Emit(root.Concat(DslReader.Read(sample, "CosLayer.mdl")).ToArray());
        var second = ModelEmitter.Emit(DslReader.Read(sample.Replace("\n", "\r\n"), "CosLayer.mdl").Concat(root).ToArray());
        Assert.Equal(first, second);
        Assert.Contains("&lt;tag&gt; &amp; value", first["CosLayer.g.cs"]);
        Assert.DoesNotContain('\r', string.Concat(first.Values));
    }

    [Fact]
    public void CSharpKeywordsAreSafeWithPrefixedInterfaceAndGetterNames()
    {
        var entities = DslReader.Read(Root, "BaseLayer.mdl").Concat(DslReader.Read(
            "package org.verapdf.model.coslayer; import org.verapdf.model.baselayer.Object; " +
            "type class extends Object { property event: String; link namespace: Object?; }", "CosLayer.mdl")).ToArray();
        var code = ModelEmitter.Emit(entities)["CosLayer.g.cs"];
        Assert.Contains("public interface Iclass", code);
        Assert.Contains("string? Getevent()", code);
        Assert.DoesNotContain("Getnamespace()", code);
    }

    [Theory]
    [InlineData("type Sample { unknown a: String; }")]
    [InlineData("type Sample { property a: String }")]
    [InlineData("type Sample { property a: String?; }")]
    [InlineData("type Sample { property a: String;")]
    [InlineData("type Sample { property a: String; } @")]
    public void InvalidSyntaxReportsSourceLine(string body)
    {
        var error = Assert.Throws<FormatException>(() =>
            DslReader.Read("package org.verapdf.model.coslayer;\n" + body, "Invalid.mdl"));
        Assert.Contains("Invalid.mdl:2:", error.Message);
    }

    [Theory]
    [InlineData("type Sample extends Missing {}", "reference")]
    [InlineData("type Sample extends Object { property a: Date; }", "scalar")]
    [InlineData("type Sample extends Object { property a: String; property a: String; }", "duplicate")]
    [InlineData("type Sample extends Object { property Links: String; }", "collision")]
    [InlineData("type Sample extends Object { link unknown: Missing; }", "reference")]
    [InlineData("type Sample extends Other {} type Other extends Sample {}", "cycle")]
    [InlineData("type Sample extends Object {} type Sample extends Object {}", "duplicate")]
    [InlineData("type Sample extends Object {} type sample extends Object {}", "collision")]
    [InlineData("type Sample extends Object { property a: String; } type Other extends Sample { property a: String; }", "collision")]
    public void SemanticErrorsAreNotSilentlySkipped(string body, string expected)
    {
        var entities = DslReader.Read(Root, "BaseLayer.mdl").Concat(DslReader.Read(
            "package org.verapdf.model.coslayer; import org.verapdf.model.baselayer.Object; " + body, "Invalid.mdl")).ToArray();
        Assert.Contains(expected, Assert.Throws<FormatException>(() => ModelEmitter.Emit(entities)).Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckIsReadOnlyAndDetectsChangedMissingAndObsoleteFiles()
    {
        var parent = Path.Combine(Path.GetTempPath(), "verapdf-model-" + Guid.NewGuid().ToString("N"));
        var directory = Path.Combine(parent, "Generated");
        var expected = ModelEmitter.Emit(DslReader.Read(Root, "BaseLayer.mdl"));
        try
        {
            Assert.NotEmpty(GeneratedFiles.Apply(directory, expected, check: true));
            Assert.False(Directory.Exists(directory));
            GeneratedFiles.Apply(directory, expected, check: false);
            Assert.Empty(GeneratedFiles.Apply(directory, expected, check: true));
            File.AppendAllText(Path.Combine(directory, "BaseLayer.g.cs"), "// edited");
            File.Delete(Path.Combine(directory, "schema.json"));
            File.WriteAllText(Path.Combine(directory, "Obsolete.g.cs"), ModelEmitter.Banner + "\n");
            File.WriteAllText(Path.Combine(directory, "Unrelated.txt"), "keep");
            var changes = GeneratedFiles.Apply(directory, expected, check: true);
            Assert.Equal(3, changes.Count);
            Assert.Contains("Changed: BaseLayer.g.cs", changes);
            Assert.Contains("Missing: schema.json", changes);
            Assert.Contains("Obsolete: Obsolete.g.cs", changes);
            Assert.False(File.Exists(Path.Combine(directory, "schema.json")));
            GeneratedFiles.Apply(directory, expected, check: false);
            Assert.Equal("keep", File.ReadAllText(Path.Combine(directory, "Unrelated.txt")));
            Assert.False(File.Exists(Path.Combine(directory, "Obsolete.g.cs")));
            Assert.Empty(GeneratedFiles.Apply(directory, expected, check: true));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
            if (Directory.Exists(parent)) Directory.Delete(parent);
        }
    }

    [Fact]
    public void RefusesUnsafeOrUnownedOutput()
    {
        var expected = ModelEmitter.Emit(DslReader.Read(Root, "BaseLayer.mdl"));
        Assert.Throws<ArgumentException>(() => GeneratedFiles.Apply(Path.GetTempPath(), expected, check: false));
        var parent = Path.Combine(Path.GetTempPath(), "verapdf-model-" + Guid.NewGuid().ToString("N"));
        var directory = Path.Combine(parent, "Generated");
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "Mine.g.cs"), "// not generator-owned");
            Assert.Throws<IOException>(() => GeneratedFiles.Apply(directory, expected, check: false));
            Assert.Throws<ArgumentException>(() => GeneratedFiles.Apply(directory,
                new Dictionary<string, string> { ["../escape.cs"] = "" }, check: false));
        }
        finally
        {
            File.Delete(Path.Combine(directory, "Mine.g.cs"));
            Directory.Delete(directory);
            Directory.Delete(parent);
        }
    }
}
