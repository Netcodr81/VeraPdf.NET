using Shouldly;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Execution;

public class FunctionTests
{
    [Fact]
    public void Exists_Should_Return_True_For_NonNull()
    {
        var doc = new TestDocument
        {
            Pages = { new TestPage { Name = "Test" } }
        };

        var validator = TestHelper.CreateValidator(
            "exists(Name)",
            typeof(TestPage));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Count_Should_Work()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage(),
                new TestPage()
            }
        };

        var validator = TestHelper.CreateValidator(
            "COUNT(Pages) == 2",
            typeof(TestDocument));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }
}