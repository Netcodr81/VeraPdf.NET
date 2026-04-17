using System.Text;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Compilation.Linq;
using VeraPdf.NET.Validation.Execution;
using VeraPdf.NET.Validation.Profiles;
using VeraPdf.NET.Validation.Rules;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Traversal;

namespace VeraPdf.NET.Validation.Tests.Utils;

internal static class TestHelper
{
    public static IRule BuildRule(string expression, Type targetType, string id = "test-rule")
    {
        var compiler = new RuleCompiler(new LinqRuleEngine());

        var definition = new RuleDefinition
        {
            Id = id,
            TargetTypeName = targetType.FullName ?? targetType.Name,
            TargetType = targetType,
            Test = expression
        };

        return compiler.Compile(definition);
    }

    public static Validator CreateValidator(string expression, Type targetType, ValidationOptions? options = null)
    {
        var rule = BuildRule(expression, targetType);

        return new Validator(
            new[] { rule },
            new ReflectionObjectWalker(),
            options);
    }

    public static PdfFileSnapshot LoadPdfSnapshot(string absolutePath)
    {
        var bytes = File.ReadAllBytes(absolutePath);
        var text = Encoding.ASCII.GetString(bytes);

        return new PdfFileSnapshot
        {
            HasPdfHeader = text.StartsWith("%PDF-", StringComparison.Ordinal),
            HasEofMarker = text.Contains("%%EOF", StringComparison.Ordinal),
            HasCatalogObject = text.Contains("/Type /Catalog", StringComparison.Ordinal),
            PageMarkerCount = CountOccurrences(text, "/Type /Page"),
            FileSize = bytes.Length
        };
    }

    public static string GetAssetPath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
