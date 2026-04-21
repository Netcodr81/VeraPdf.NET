using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Context;
using VeraPdf.NET.Validation.Results;

namespace VeraPdf.NET.Validation.Execution;

/// <summary>
/// Main validation engine.
/// 
/// Supports:
/// - Sequential validation (default fallback)
/// - Parallel validation (high-performance mode)
/// </summary>
public sealed class Validator : IValidator
{
    private readonly IObjectWalker _walker;
    private readonly RuleDispatchTable _dispatch;
    private readonly ValidationOptions _options;

    public Validator(
        IEnumerable<IRule> rules,
        IObjectWalker walker,
        ValidationOptions? options = null)
    {
        _walker = walker;
        _dispatch = new RuleDispatchTable(rules);
        _options = options ?? new ValidationOptions();
    }

    /// <summary>
    /// Entry point for validation.
    /// Chooses sequential or parallel execution based on options.
    /// </summary>
    public ValidationResult Validate(object root)
    {
        return _options.EnableParallel
            ? ValidateParallel(root)
            : ValidateSequential(root);
    }

    // ------------------------------------------------------------
    // Sequential (fallback / debugging)
    // ------------------------------------------------------------

    private ValidationResult ValidateSequential(object root)
    {
        var results = new List<RuleResult>();
        long objectOrder = 0;

        foreach (var obj in _walker.Traverse(root))
        {
            var ctx = new CompiledValidationContext(obj);
            var rules = _dispatch.GetRules(obj.GetType());
            var ruleOrder = 0;

            foreach (var rule in rules)
            {
                var passed = rule.Evaluate(ctx);

                results.Add(new RuleResult(rule.Id, passed, obj, objectOrder, ruleOrder));

                if (_options.StopOnFirstFailure && !passed)
                    return new ValidationResult(results);

                ruleOrder++;
            }

            objectOrder++;
        }

        return new ValidationResult(results);
    }

    // ------------------------------------------------------------
    // Parallel Execution (HIGH PERFORMANCE)
    // ------------------------------------------------------------

    private ValidationResult ValidateParallel(object root)
    {
        var collector = new RuleResultCollector();

        var objects = _walker.Traverse(root);

        var partitioner = PartitionerHelper.Create(objects);

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism =
                _options.MaxDegreeOfParallelism ?? Environment.ProcessorCount
        };

        Parallel.ForEach(partitioner, parallelOptions, (obj, _, objectOrder) =>
        {
            var ctx = new CompiledValidationContext(obj);
            var rules = _dispatch.GetRules(obj.GetType());
            var ruleOrder = 0;

            foreach (var rule in rules)
            {
                var passed = rule.Evaluate(ctx);

                collector.Add(new RuleResult(rule.Id, passed, obj, objectOrder, ruleOrder));

                // NOTE:
                // StopOnFirstFailure is NOT safe in parallel mode
                // unless you introduce cancellation tokens
                ruleOrder++;
            }
        });

        return new ValidationResult(collector.ToList());
    }
}