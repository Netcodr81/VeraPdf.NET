namespace VeraPdf.NET.Validation.Parsing.AST;

public sealed class FunctionNode : AstNode
{
    public string Name { get; init; } = default!;
    public List<AstNode> Arguments { get; init; } = new();
}
