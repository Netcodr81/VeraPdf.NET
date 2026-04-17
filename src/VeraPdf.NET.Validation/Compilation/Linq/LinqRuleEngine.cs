using System.Linq.Expressions;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Parsing.AST;

namespace VeraPdf.NET.Validation.Compilation.Linq;

public sealed class LinqRuleEngine : IRuleEngine
{
    /// <summary>
    /// Compiles AST into executable LINQ expression delegate.
    /// Core execution engine for validation.
    /// </summary>
    public Func<IValidationContext, bool> Compile(AstNode node)
    {
        var param = Expression.Parameter(typeof(IValidationContext), "ctx");

        var body = ExpressionBuilder.Build(node, param);

        return Expression.Lambda<Func<IValidationContext, bool>>(body, param).Compile();
    }
}