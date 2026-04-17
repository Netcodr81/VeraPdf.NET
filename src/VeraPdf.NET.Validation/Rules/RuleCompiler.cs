using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Parsing.Parser;
using VeraPdf.NET.Validation.Profiles;

namespace VeraPdf.NET.Validation.Rules;

public sealed class RuleCompiler
{
    private readonly ExpressionParser _parser = new();
    private readonly IRuleEngine _engine;

    public RuleCompiler(IRuleEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Compiles rule definition into executable rule.
    ///
    /// Uses resolved CLR Type instead of string.
    /// </summary>
    public IRule Compile(RuleDefinition def)
    {
        var ast = _parser.Parse(def.Test);
        var func = _engine.Compile(ast);

        return new CompiledRule(def.Id, def.TargetType, func);
    }
}