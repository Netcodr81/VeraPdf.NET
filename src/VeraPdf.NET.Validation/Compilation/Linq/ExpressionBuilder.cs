using System.Linq.Expressions;
using System.Reflection;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Context;
using VeraPdf.NET.Validation.Parsing.AST;

namespace VeraPdf.NET.Validation.Compilation.Linq;

internal static class ExpressionBuilder
{
    // ------------------------------------------------------------
    // Cached MethodInfo (no runtime reflection overhead)
    // ------------------------------------------------------------

    private static readonly MethodInfo GetPropertyMethod =
        typeof(IValidationContext).GetMethod(nameof(IValidationContext.GetProperty))!;

    private static readonly MethodInfo SetCurrentMethod =
        typeof(ReusableScopedContext).GetMethod(nameof(ReusableScopedContext.SetCurrent))!;

    private static readonly MethodInfo CoerceMethod =
        typeof(TypeCoercion).GetMethod(nameof(TypeCoercion.Coerce))!;

    private static readonly MethodInfo ToBoolMethod =
        typeof(TypeCoercion).GetMethod(nameof(TypeCoercion.ToBool))!;

    private static readonly MethodInfo ObjectEqualsMethod =
        typeof(object).GetMethod(nameof(object.Equals), new[] { typeof(object), typeof(object) })!;

    // ------------------------------------------------------------
    // Entry Point
    // ------------------------------------------------------------

    /// <summary>
    /// Builds expression tree from AST.
    ///
    /// IMPORTANT:
    /// - ctx is Expression to support scoped contexts
    /// - Return type is always object-compatible
    /// </summary>
    public static Expression Build(AstNode node, Expression ctx)
    {
        return node switch
        {
            BinaryNode b => BuildBinary(b, ctx),
            UnaryNode u => BuildUnary(u, ctx),
            PropertyNode p => BuildProperty(p, ctx),
            ConstantNode c => Expression.Constant(c.Value), // already object-safe
            FunctionNode f => BuildFunction(f, ctx),
            _ => throw new NotSupportedException($"Unsupported node: {node.GetType().Name}")
        };
    }

    // ------------------------------------------------------------
    // Property Access
    // ------------------------------------------------------------

    /// <summary>
    /// Delegates property resolution to context.
    /// Avoids reflection in expression tree.
    /// </summary>
    private static Expression BuildProperty(PropertyNode node, Expression ctx)
    {
        return Expression.Call(
            ctx,
            GetPropertyMethod,
            Expression.Constant(node.Path)
        );
    }

    // ------------------------------------------------------------
    // Binary Operations (REDUCED BOXING)
    // ------------------------------------------------------------

    /// <summary>
    /// Builds binary operations with minimal boxing.
    /// </summary>
    private static Expression BuildBinary(BinaryNode node, Expression ctx)
    {
        var left = EnsureObject(Build(node.Left, ctx));
        var right = EnsureObject(Build(node.Right, ctx));

        // Coerce only once
        var coerceCall = Expression.Call(CoerceMethod, left, right);

        var leftVal = Expression.Field(coerceCall, "Item1");
        var rightVal = Expression.Field(coerceCall, "Item2");
        var valueEquals = Expression.Call(ObjectEqualsMethod, leftVal, rightVal);

        return node.Operator switch
        {
            "==" => valueEquals,
            "!=" => Expression.Not(valueEquals),

            "<" => SafeCompare(leftVal, rightVal, ExpressionType.LessThan),
            ">" => SafeCompare(leftVal, rightVal, ExpressionType.GreaterThan),
            "<=" => SafeCompare(leftVal, rightVal, ExpressionType.LessThanOrEqual),
            ">=" => SafeCompare(leftVal, rightVal, ExpressionType.GreaterThanOrEqual),

            "&&" => Expression.AndAlso(ToBool(leftVal), ToBool(rightVal)),
            "||" => Expression.OrElse(ToBool(leftVal), ToBool(rightVal)),

            _ => throw new NotSupportedException($"Operator {node.Operator} not supported")
        };
    }

    // ------------------------------------------------------------
    // Unary Operations
    // ------------------------------------------------------------

    private static Expression BuildUnary(UnaryNode node, Expression ctx)
    {
        var operand = EnsureObject(Build(node.Operand, ctx));

        return node.Operator switch
        {
            "!" => Expression.Not(ToBool(operand)),
            _ => throw new NotSupportedException($"Unary operator {node.Operator} not supported")
        };
    }

    // ------------------------------------------------------------
    // Function Dispatcher
    // ------------------------------------------------------------

    private static Expression BuildFunction(FunctionNode node, Expression ctx)
    {
        var name = node.Name;

        switch (name.ToUpperInvariant())
        {
            case "COUNT": return BuildCount(node, ctx);
            case "ANY": return BuildAny(node, ctx);
            case "ALL": return BuildAll(node, ctx);
        }

        if (!FunctionRegistry.Functions.TryGetValue(name, out var method))
            throw new NotSupportedException($"Function '{name}' not supported");

        var args = node.Arguments
            .Select(arg => EnsureObject(Build(arg, ctx))) // avoid double boxing
            .ToArray();

        return Expression.Call(method, args);
    }

    // ------------------------------------------------------------
    // COUNT
    // ------------------------------------------------------------

    private static Expression BuildCount(FunctionNode node, Expression ctx)
    {
        var collection = ToEnumerable(Build(node.Arguments[0], ctx));

        var countCall = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Count),
            new[] { typeof(object) },
            collection
        );

        return Expression.Condition(
            Expression.Equal(collection, Expression.Constant(null)),
            Expression.Constant(0),
            countCall
        );
    }

    // ------------------------------------------------------------
    // ANY (ZERO ALLOCATION + REDUCED BOXING)
    // ------------------------------------------------------------

    private static Expression BuildAny(FunctionNode node, Expression ctx)
    {
        var collection = ToEnumerable(Build(node.Arguments[0], ctx));

        var itemParam = Expression.Parameter(typeof(object), "x");

        var scopedVar = Expression.Variable(typeof(ReusableScopedContext), "scoped");

        var assignScoped = Expression.Assign(
            scopedVar,
            Expression.New(typeof(ReusableScopedContext).GetConstructor(new[] { typeof(IValidationContext) })!, ctx)
        );

        var setCurrent = Expression.Call(scopedVar, SetCurrentMethod, itemParam);

        var predicateBody = Build(node.Arguments[1], scopedVar);

        var predicate = Expression.Lambda<Func<object, bool>>(
            Expression.Block(
                setCurrent,
                ToBool(EnsureObject(predicateBody))
            ),
            itemParam
        );

        var anyCall = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Any),
            new[] { typeof(object) },
            collection,
            predicate
        );

        return Expression.Block(
            new[] { scopedVar },
            assignScoped,
            Expression.Condition(
                Expression.Equal(collection, Expression.Constant(null)),
                Expression.Constant(false),
                anyCall
            )
        );
    }

    // ------------------------------------------------------------
    // ALL
    // ------------------------------------------------------------

    private static Expression BuildAll(FunctionNode node, Expression ctx)
    {
        var collection = ToEnumerable(Build(node.Arguments[0], ctx));

        var itemParam = Expression.Parameter(typeof(object), "x");

        var scopedVar = Expression.Variable(typeof(ReusableScopedContext), "scoped");

        var assignScoped = Expression.Assign(
            scopedVar,
            Expression.New(typeof(ReusableScopedContext).GetConstructor(new[] { typeof(IValidationContext) })!, ctx)
        );

        var setCurrent = Expression.Call(scopedVar, SetCurrentMethod, itemParam);

        var predicateBody = Build(node.Arguments[1], scopedVar);

        var predicate = Expression.Lambda<Func<object, bool>>(
            Expression.Block(
                setCurrent,
                ToBool(EnsureObject(predicateBody))
            ),
            itemParam
        );

        var allCall = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.All),
            new[] { typeof(object) },
            collection,
            predicate
        );

        return Expression.Block(
            new[] { scopedVar },
            assignScoped,
            Expression.Condition(
                Expression.Equal(collection, Expression.Constant(null)),
                Expression.Constant(false),
                allCall
            )
        );
    }

    // ------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------

    /// <summary>
    /// Ensures expression is object-typed WITHOUT double boxing.
    /// </summary>
    private static Expression EnsureObject(Expression expr)
    {
        return expr.Type == typeof(object)
            ? expr
            : Expression.Convert(expr, typeof(object));
    }

    private static Expression ToEnumerable(Expression expr)
    {
        var input = EnsureObject(expr);

        return Expression.Call(
            typeof(CollectionHelpers),
            nameof(CollectionHelpers.ToObjectEnumerable),
            null,
            input
        );
    }

    private static Expression SafeCompare(Expression left, Expression right, ExpressionType type)
    {
        var nullCheck = Expression.OrElse(
            Expression.Equal(left, Expression.Constant(null)),
            Expression.Equal(right, Expression.Constant(null))
        );

        var compare = Expression.MakeBinary(
            type,
            Expression.Convert(left, typeof(double)),
            Expression.Convert(right, typeof(double))
        );

        return Expression.Condition(nullCheck, Expression.Constant(false), compare);
    }

    private static Expression ToBool(Expression expr)
    {
        return Expression.Call(ToBoolMethod, expr);
    }
}