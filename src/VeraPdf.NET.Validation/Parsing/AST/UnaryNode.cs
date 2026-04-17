namespace VeraPdf.NET.Validation.Parsing.AST;

public sealed class UnaryNode : AstNode
{
    public AstNode Operand { get; init; } = default!;
    public string Operator { get; init; } = default!;
}
