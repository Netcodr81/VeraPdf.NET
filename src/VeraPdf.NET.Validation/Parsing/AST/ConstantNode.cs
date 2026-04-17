namespace VeraPdf.NET.Validation.Parsing.AST;

public sealed class ConstantNode : AstNode
{
    public object? Value { get; init; }
}
