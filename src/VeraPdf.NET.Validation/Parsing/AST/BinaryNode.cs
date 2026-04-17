namespace VeraPdf.NET.Validation.Parsing.AST;

public sealed class BinaryNode : AstNode
{
    public AstNode Left { get; init; } = default!;
    public AstNode Right { get; init; } = default!;
    public string Operator { get; init; } = default!;
}
