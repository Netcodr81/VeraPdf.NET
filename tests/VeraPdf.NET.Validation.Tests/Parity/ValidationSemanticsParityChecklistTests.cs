using Shouldly;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Parity;

public class ValidationSemanticsParityChecklistTests
{
    [Fact]
    public void Checklist_Exists_And_IsDefined_Should_Work_For_Supported_Cases()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage
                {
                    Name = "Cover",
                    Number = 1
                }
            }
        };

        var validator = TestHelper.CreateValidator(
            "exists(Name) && isDefined(Name)",
            typeof(TestPage));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Checklist_HasType_Currently_Has_Argument_Typing_Gap()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage
                {
                    Name = "Cover",
                    Number = 1
                }
            }
        };

        Should.Throw<ArgumentException>(() =>
        {
            var validator = TestHelper.CreateValidator(
                "hasType(Name, \"String\")",
                typeof(TestPage));

            _ = validator.Validate(doc);
        });
    }

    [Fact]
    public void Checklist_Count_Any_All_Should_Work_For_Supported_Cases()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage { Number = 1 },
                new TestPage { Number = 4 },
                new TestPage { Number = 7 }
            }
        };

        var validator = TestHelper.CreateValidator(
            "COUNT(Pages) == 3 && ANY(Pages, Number > 5) && ALL(Pages, Number > 0)",
            typeof(TestDocument));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Checklist_All_Should_Return_True_For_Null_Collections_In_Current_Engine()
    {
        var doc = new TestDocument
        {
            Pages = null!
        };

        var validator = TestHelper.CreateValidator(
            "ANY(Pages, Number > 0) == false && ALL(Pages, Number > 0) == true",
            typeof(TestDocument));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Checklist_Function_Name_Casing_Should_Be_Case_Insensitive()
    {
        var doc = new TestDocument
        {
            Pages =
            {
                new TestPage
                {
                    Name = "Cover",
                    Number = 1
                }
            }
        };

        var validator = TestHelper.CreateValidator(
            "ExIsTs(Name) && IsDeFiNeD(Name)",
            typeof(TestPage));

        var result = validator.Validate(doc);

        result.IsCompliant.ShouldBeTrue();
    }
}
