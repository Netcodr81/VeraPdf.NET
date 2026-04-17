using VeraPdf.NET.Validation.Parsing.AST;

namespace VeraPdf.NET.Validation.Abstractions;

public interface IRuleEngine
{
    /// <summary>
    /// Compiles an AST into an executable delegate.
    /// This is equivalent to veraPDF's rule interpretation layer.
    /// </summary>
    Func<IValidationContext, bool> Compile(AstNode node);
}
