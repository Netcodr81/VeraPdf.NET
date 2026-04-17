namespace VeraPdf.NET.Validation.Parsing.Tokenizer;

public sealed class Tokenizer
{
    private string _input = "";
    private int _pos;

    public IEnumerable<Token> Tokenize(string input)
    {
        _input = input;
        _pos = 0;

        while (!IsAtEnd())
        {
            SkipWhitespace();

            if (IsAtEnd()) break;

            char c = Peek();

            if (char.IsLetter(c))
                yield return ReadIdentifier();
            else if (char.IsDigit(c))
                yield return ReadNumber();
            else
                yield return ReadSymbol();
        }

        yield return new Token(TokenType.End, "");
    }

    private Token ReadIdentifier()
    {
        int start = _pos;

        while (!IsAtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
            Advance();

        var value = _input[start.._pos];

        return value switch
        {
            "true" => new Token(TokenType.Boolean, value),
            "false" => new Token(TokenType.Boolean, value),
            "null" => new Token(TokenType.Null, value),
            "AND" => new Token(TokenType.And, value),
            "OR" => new Token(TokenType.Or, value),
            "ANY" => new Token(TokenType.Identifier, value),
            "ALL" => new Token(TokenType.Identifier, value),
            _ => new Token(TokenType.Identifier, value)
        };
    }

    private Token ReadNumber()
    {
        int start = _pos;

        while (!IsAtEnd() && char.IsDigit(Peek()))
            Advance();

        return new Token(TokenType.Number, _input[start.._pos]);
    }

    private Token ReadSymbol()
    {
        char c = Advance();

        return c switch
        {
            '(' => new Token(TokenType.OpenParen, "("),
            ')' => new Token(TokenType.CloseParen, ")"),
            '.' => new Token(TokenType.Dot, "."),
            ',' => new Token(TokenType.Comma, ","),

            '!' when Match('=') => new Token(TokenType.NotEqual, "!="),
            '!' => new Token(TokenType.Not, "!"),

            '=' when Match('=') => new Token(TokenType.Equal, "=="),

            '<' when Match('=') => new Token(TokenType.LessEqual, "<="),
            '<' => new Token(TokenType.Less, "<"),

            '>' when Match('=') => new Token(TokenType.GreaterEqual, ">="),
            '>' => new Token(TokenType.Greater, ">"),

            '&' when Match('&') => new Token(TokenType.And, "&&"),
            '|' when Match('|') => new Token(TokenType.Or, "||"),

            '"' => ReadString(),

            _ => throw new Exception($"Unexpected character: {c}")
        };
    }

    private Token ReadString()
    {
        int start = _pos;

        while (!IsAtEnd() && Peek() != '"')
            Advance();

        var value = _input[start.._pos];
        Advance(); // closing "

        return new Token(TokenType.String, value);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || _input[_pos] != expected) return false;
        _pos++;
        return true;
    }

    private char Advance() => _input[_pos++];
    private char Peek() => _input[_pos];
    private bool IsAtEnd() => _pos >= _input.Length;

    private void SkipWhitespace()
    {
        while (!IsAtEnd() && char.IsWhiteSpace(Peek()))
            Advance();
    }
}
