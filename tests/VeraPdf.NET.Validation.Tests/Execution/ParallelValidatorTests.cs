using Shouldly;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Execution;
using VeraPdf.NET.Validation.Tests.TestModels;
using VeraPdf.NET.Validation.Tests.Utils;
using VeraPdf.NET.Validation.Traversal;

namespace VeraPdf.NET.Validation.Tests.Execution;

public class ParallelValidatorTests
{
    [Fact]
    public void Parallel_And_Sequential_Should_Match()
    {
        var doc = new TestDocument();

        for (var i = 0; i < 100; i++)
        {
            doc.Pages.Add(new TestPage { Number = i });
        }

        var rule = TestHelper.BuildRule("Number >= 0", typeof(TestPage));
        var walker = new ReflectionObjectWalker();

        var sequential = new Validator(
            new[] { rule },
            walker,
            new ValidationOptions { EnableParallel = false });

        var parallel = new Validator(
            new[] { rule },
            walker,
            new ValidationOptions { EnableParallel = true });

        var sequentialResult = sequential.Validate(doc);
        var parallelResult = parallel.Validate(doc);

        parallelResult.Results.Count.ShouldBe(sequentialResult.Results.Count);
        parallelResult.IsCompliant.ShouldBe(sequentialResult.IsCompliant);
    }
}