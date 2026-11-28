using System.Text.RegularExpressions;

namespace VeraPdf.Library.Results;

/// <summary>Location of an assertion in the validation model.</summary>
public sealed record Location(string? Level, string? Context)
{
    private static readonly Regex Dereference = new(@"\([0-9]+ ([a-zA-Z]{4})", RegexOptions.CultureInvariant);

    /// <summary>Creates a location using the reference implementation's context normalization.</summary>
    public static Location FromValues(string? level, string? context)
    {
        if (context is null)
            return new Location(level, null);
        var first = Dereference.Match(context);
        return new Location(level, first.Success
            ? Dereference.Replace(context, _ => "(" + first.Groups[1].Value)
            : context);
    }
}
