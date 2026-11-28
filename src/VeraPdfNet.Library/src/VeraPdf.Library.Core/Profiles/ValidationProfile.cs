namespace VeraPdf.Library.Profiles;

public sealed record ProfileDetails(string? Name, string? Description, string? Creator, string? Created);

public sealed record RuleId(string Specification, string Clause, int TestNumber);

public sealed record RuleReference(string? Specification, string? Clause);

public sealed record ErrorArgument(string Expression, string? Name);

public sealed record ErrorDetails(string? Message, IReadOnlyList<ErrorArgument> Arguments);

public sealed record Rule(
    RuleId Id,
    string Object,
    bool? Deferred,
    string? Tags,
    string? Description,
    string? Test,
    ErrorDetails? Error,
    IReadOnlyList<RuleReference> References)
{
    /// <summary>Returns the comma-separated tags using ordinal, case-sensitive comparison.</summary>
    public IReadOnlySet<string> GetTags()
    {
        var tags = new HashSet<string>(StringComparer.Ordinal);
        if (Tags is null)
            return tags;

        var parts = Tags.Split(',');
        var count = parts.Length;
        while (count > 0 && parts[count - 1].Length == 0 && Tags.Length != 0)
            count--;
        for (var i = 0; i < count; i++)
            tags.Add(parts[i]);
        return tags;
    }
}

public sealed record Variable(string Name, string Object, string? DefaultValue, string? Value);

/// <summary>A profile's data; this object does not execute its validation rules.</summary>
public sealed class ValidationProfile
{
    internal ValidationProfile(
        string? flavour, ProfileDetails? details, string? hash,
        IReadOnlyList<Rule> rules, IReadOnlyList<Variable> variables)
    {
        Flavour = flavour;
        Details = details;
        Hash = hash;
        Rules = rules;
        Variables = variables;
    }

    public string? Flavour { get; }
    public ProfileDetails? Details { get; }
    public string? Hash { get; }
    public IReadOnlyList<Rule> Rules { get; }
    public IReadOnlyList<Variable> Variables { get; }

    /// <summary>Creates a custom profile from explicit values without executing its rules.</summary>
    public static ValidationProfile Create(
        string flavour, ProfileDetails details, string hash,
        IEnumerable<Rule> rules, IEnumerable<Variable> variables)
    {
        ArgumentNullException.ThrowIfNull(flavour);
        ArgumentNullException.ThrowIfNull(details);
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(variables);
        var copiedRules = new List<Rule>();
        foreach (var rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);
            ArgumentNullException.ThrowIfNull(rule.Id);
            ArgumentNullException.ThrowIfNull(rule.Object);
            ArgumentNullException.ThrowIfNull(rule.References);
            ArgumentNullException.ThrowIfNull(rule.Id.Specification);
            ArgumentNullException.ThrowIfNull(rule.Id.Clause);
            if (rule.References.Any(reference => reference is null))
                throw new ArgumentException("Rule references cannot contain null.", nameof(rules));
            ErrorDetails? error = null;
            if (rule.Error is not null)
            {
                ArgumentNullException.ThrowIfNull(rule.Error.Arguments);
                if (rule.Error.Arguments.Any(argument => argument is null || argument.Expression is null))
                    throw new ArgumentException("Rule arguments and expressions cannot be null.", nameof(rules));
                error = rule.Error with { Arguments = Array.AsReadOnly(rule.Error.Arguments.ToArray()) };
            }
            copiedRules.Add(rule with
            {
                Error = error,
                References = Array.AsReadOnly(rule.References.ToArray())
            });
        }
        var copiedVariables = variables.ToArray();
        if (copiedVariables.Any(variable => variable is null || variable.Name is null || variable.Object is null))
            throw new ArgumentException("Variables, their names and object types cannot be null.", nameof(variables));
        return new ValidationProfile(flavour, details, hash,
            Array.AsReadOnly(copiedRules.ToArray()), Array.AsReadOnly(copiedVariables));
    }

    public IEnumerable<Rule> GetRulesByObject(string objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        return Rules.Where(rule => string.Equals(rule.Object, objectName, StringComparison.Ordinal));
    }

    public IEnumerable<Variable> GetVariablesByObject(string objectName)
    {
        ArgumentNullException.ThrowIfNull(objectName);
        return Variables.Where(variable => string.Equals(variable.Object, objectName, StringComparison.Ordinal));
    }

    /// <summary>Returns the last rule with the given ID, or null when none exists.</summary>
    public Rule? GetRuleById(RuleId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return Rules.LastOrDefault(rule => rule.Id == id);
    }

    /// <summary>Returns all distinct tags in ordinal order.</summary>
    public IReadOnlyList<string> GetTags() =>
        Array.AsReadOnly(Rules.SelectMany(rule => rule.GetTags())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(tag => tag, StringComparer.Ordinal)
            .ToArray());

    /// <summary>Filters rules using veraPDF's include/exclude tag semantics.</summary>
    public ValidationProfile WithTags(IEnumerable<string> includeTags, IEnumerable<string> excludeTags)
    {
        ArgumentNullException.ThrowIfNull(includeTags);
        ArgumentNullException.ThrowIfNull(excludeTags);
        var include = new HashSet<string>(includeTags, StringComparer.Ordinal);
        var exclude = new HashSet<string>(excludeTags, StringComparer.Ordinal);
        var rules = exclude.Contains("*")
            ? []
            : Rules.Where(rule =>
            {
                var tags = rule.GetTags();
                return tags.Count == 0 ||
                       ((include.Contains("*") || tags.Overlaps(include)) && !tags.Overlaps(exclude));
            }).ToArray();
        return new ValidationProfile(Flavour, Details, Hash, Array.AsReadOnly(rules), Variables);
    }
}
