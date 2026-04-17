namespace VeraPdf.NET.Validation.Parsing.Tokenizer;

public enum TokenType
{
    Identifier,
    Number,
    String,
    Boolean,
    Null,

    And, Or, Not,

    Equal, NotEqual,
    Less, LessEqual,
    Greater, GreaterEqual,

    Dot,
    Comma,

    OpenParen,
    CloseParen,

    End
}
