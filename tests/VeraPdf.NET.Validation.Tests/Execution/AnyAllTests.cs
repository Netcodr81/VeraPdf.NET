using Shouldly;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Execution;

public class AnyAllTests
{
    [Fact]
    public void Any_Should_Return_True_When_Any_Match()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage { Number = 1 },
                new TestPage { Number = 5 }
            }
        };

        var validator = TestHelper.CreateValidator(
            "ANY(Pages, Number > 3)",
            typeof(TestDocument));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void All_Should_Return_False_When_One_Fails()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage { Number = 1 },
                new TestPage { Number = 5 }
            }
        };

        var validator = TestHelper.CreateValidator(
            "ALL(Pages, Number > 3)",
            typeof(TestDocument));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeFalse();
    }
}