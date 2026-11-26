namespace VeraPdf.Model.Generation;

/// <summary>Strict reader for the veraPDF model snapshot's declaration-only DSL.</summary>
public sealed class DslReader
{
    private sealed record Token(string Text, int Line, bool Comment = false);
    private readonly IReadOnlyList<Token> _tokens;
    private readonly string _source;
    private int _position;

    private DslReader(string text, string source)
    {
        _source = source;
        _tokens = Tokenize(text, source);
    }

    public static IReadOnlyList<ModelEntity> Read(string text, string source) => new DslReader(text, source).ReadModel();

    private IReadOnlyList<ModelEntity> ReadModel()
    {
        Expect("package");
        var package = QualifiedName();
        Expect(";");
        var imports = new List<string>();
        while (Peek() == "import")
        {
            Take();
            imports.Add(QualifiedName(allowWildcard: true));
            Expect(";");
        }
        var entities = new List<ModelEntity>();
        while (Peek() != "")
        {
            var documentation = Comments();
            if (Peek() == "") break;
            var declarationLine = _tokens[_position].Line;
            Expect("type");
            var name = Identifier();
            string? baseName = null;
            if (Peek() == "extends")
            {
                Take();
                baseName = QualifiedName();
            }
            Expect("{");
            var properties = new List<ModelAttribute>();
            var links = new List<ModelAttribute>();
            while (Peek() != "}")
            {
                var memberDocumentation = Comments();
                if (Peek() == "}") break;
                var memberLine = _tokens[_position].Line;
                var kind = Take();
                if (kind is not ("property" or "link")) Fail("Expected property or link.");
                var memberName = Identifier();
                Expect(":");
                var memberType = kind == "link" ? QualifiedName() : Scalar();
                var multiplicity = "";
                if (kind == "link" && Peek() is "?" or "*" or "+") multiplicity = Take();
                Expect(";");
                var member = new ModelAttribute(memberName, memberType, multiplicity, memberDocumentation, memberLine);
                (kind == "property" ? properties : links).Add(member);
            }
            Expect("}");
            entities.Add(new ModelEntity(package, name, baseName, _source, imports.ToArray(),
                properties.ToArray(), links.ToArray(), documentation, declarationLine));
        }
        return entities;
    }

    private string Scalar()
    {
        var name = Identifier();
        if (Peek() == "<")
        {
            Take();
            name += "<" + Scalar() + ">";
            Expect(">");
        }
        return name;
    }

    private string QualifiedName(bool allowWildcard = false)
    {
        var result = Identifier();
        while (Peek() == ".")
        {
            Take();
            if (allowWildcard && Peek() == "*")
            {
                Take();
                return result + ".*";
            }
            result += "." + Identifier();
        }
        return result;
    }

    private IReadOnlyList<string> Comments()
    {
        var lines = new List<string>();
        while (_tokens[_position].Comment) lines.Add(Take().Trim());
        return lines;
    }

    private string Identifier()
    {
        var token = _tokens[_position];
        if (token.Comment || token.Text.Length == 0 ||
            !(char.IsAsciiLetter(token.Text[0]) || token.Text[0] == '_') ||
            !token.Text.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
        {
            Fail("Expected an identifier.");
        }
        return Take();
    }

    private string Peek() => _tokens[_position].Text;

    private string Take()
    {
        if (_position == _tokens.Count - 1) Fail("Unexpected end of file.");
        return _tokens[_position++].Text;
    }

    private void Expect(string value)
    {
        if (Peek() != value || _tokens[_position].Comment) Fail($"Expected '{value}'.");
        Take();
    }

    private void Fail(string message) =>
        throw new FormatException($"{_source}:{_tokens[_position].Line}: {message} Found '{Peek()}'.");

    private static IReadOnlyList<Token> Tokenize(string text, string source)
    {
        var result = new List<Token>();
        var line = 1;
        for (var i = 0; i < text.Length;)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c) || (i == 0 && c == '\uFEFF'))
            {
                if (c == '\n' || (c == '\r' && (i + 1 == text.Length || text[i + 1] != '\n'))) line++;
                i++;
                continue;
            }
            if (c == '%')
            {
                var start = ++i;
                while (i < text.Length && text[i] is not ('\r' or '\n')) i++;
                result.Add(new Token(text[start..i], line, Comment: true));
                continue;
            }
            if (char.IsAsciiLetter(c) || c == '_')
            {
                var start = i++;
                while (i < text.Length && (char.IsAsciiLetterOrDigit(text[i]) || text[i] == '_')) i++;
                result.Add(new Token(text[start..i], line));
                continue;
            }
            if ("{}:;.<>?*+".Contains(c, StringComparison.Ordinal))
            {
                result.Add(new Token(c.ToString(), line));
                i++;
                continue;
            }
            throw new FormatException($"{source}:{line}: Unsupported character '{c}'.");
        }
        result.Add(new Token("", line));
        return result;
    }
}
