namespace VeraPdf.NET.Validation.Parsing.AST;

public sealed class PropertyNode : AstNode
{
    public string Path { get; init; } = default!;
}
