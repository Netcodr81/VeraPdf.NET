using Shouldly;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;

namespace VeraPdf.NET.Validation.Tests.Execution;

public class ValidatorTests
{
    [Fact]
    public void Should_Validate_Simple_Property()
    {
        var doc = new TestDocument
        {
            Pages = { new TestPage { Name = "Page1" } }
        };

        var validator = TestHelper.CreateValidator("Name != null", typeof(TestPage));

        var result = validator.Validate(doc);

        result.Results.Count.ShouldBe(1);
        result.Results[0].Passed.ShouldBeTrue();
        result.IsCompliant.ShouldBeTrue();
    }

    [Fact]
    public void Should_Fail_When_Property_Is_Null()
    {
        var doc = new TestDocument
        {
            Pages = { new TestPage { Name = null } }
        };

        var validator = TestHelper.CreateValidator("Name != null", typeof(TestPage));

        var result = validator.Validate(doc);

        result.Results.Count.ShouldBe(1);
        result.Results[0].Passed.ShouldBeFalse();
        result.IsCompliant.ShouldBeFalse();
    }
}