using VeraPdf.NET.Validation.Parsing.AST;
using VeraPdf.NET.Validation.Parsing.Tokenizer;

namespace VeraPdf.NET.Validation.Parsing.Parser;

public sealed class ExpressionParser
{
    private List<Token> _tokens = new();
    private int _pos;

    public AstNode Parse(string input)
    {
        var tokenizer = new Tokenizer.Tokenizer();
        _tokens = tokenizer.Tokenize(input).ToList();
        _pos = 0;

        return ParseOr();
    }

    private AstNode ParseOr()
    {
        var left = ParseAnd();

        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = ParseAnd();

            left = new BinaryNode { Left = left, Right = right, Operator = op.Value };
        }

        return left;
    }

    private AstNode ParseAnd()
    {
        var left = ParseEquality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = ParseEquality();

            left = new BinaryNode { Left = left, Right = right, Operator = op.Value };
        }

        return left;
    }

    private AstNode ParseEquality()
    {
        var left = ParseComparison();

        while (Match(TokenType.Equal, TokenType.NotEqual))
        {
            var op = Previous();
            var right = ParseComparison();

            left = new BinaryNode { Left = left, Right = right, Operator = op.Value };
        }

        return left;
    }

    private AstNode ParseComparison()
    {
        var left = ParseFactor();

        while (Match(TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual))
        {
            var op = Previous();
            var right = ParseFactor();

            left = new BinaryNode { Left = left, Right = right, Operator = op.Value };
        }

        return left;
    }

    private AstNode ParseFactor()
    {
        if (Match(TokenType.Not))
        {
            return new UnaryNode
            {
                Operator = "!",
                Operand = ParseFactor()
            };
        }

        return ParsePrimary();
    }

    private AstNode ParsePrimary()
    {
        if (Match(TokenType.Number))
            return new ConstantNode { Value = int.Parse(Previous().Value) };

        if (Match(TokenType.String))
            return new ConstantNode { Value = Previous().Value };

        if (Match(TokenType.Boolean))
            return new ConstantNode { Value = bool.Parse(Previous().Value) };

        if (Match(TokenType.Null))
            return new ConstantNode { Value = null };

        if (Match(TokenType.Identifier))
        {
            var name = Previous().Value;

            if (Match(TokenType.OpenParen))
                return ParseFunction(name);

            return ParseProperty(name);
        }

        if (Match(TokenType.OpenParen))
        {
            var expr = ParseOr();
            Consume(TokenType.CloseParen);
            return expr;
        }

        throw new Exception("Invalid expression");
    }

    private AstNode ParseFunction(string name)
    {
        var args = new List<AstNode>();

        if (!Check(TokenType.CloseParen))
        {
            do
            {
                args.Add(ParseOr());
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.CloseParen);

        return new FunctionNode { Name = name, Arguments = args };
    }

    private AstNode ParseProperty(string name)
    {
        var path = name;

        while (Match(TokenType.Dot))
        {
            var next = Consume(TokenType.Identifier);
            path += "." + next.Value;
        }

        return new PropertyNode { Path = path };
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                _pos++;
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type) =>
        _pos < _tokens.Count && _tokens[_pos].Type == type;

    private Token Consume(TokenType type)
    {
        if (!Check(type)) throw new Exception($"Expected {type}");
        return _tokens[_pos++];
    }

    private Token Previous() => _tokens[_pos - 1];
}
