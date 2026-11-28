namespace VeraPdf.Library.Validation;

/// <summary>Configuration data for a future validator; it does not perform validation.</summary>
public sealed class ValidatorConfiguration
{
    public const int DefaultDisplayedFailedChecks = 100;

    public string Flavour { get; init; } = "NO_FLAVOUR";
    public string DefaultFlavour { get; init; } = "PDFA_1_B";
    public bool RecordPasses { get; init; }

    /// <summary>A positive value enables fail-fast; zero and negative values do not.</summary>
    public int MaxFails { get; init; } = -1;

    public bool Debug { get; init; }
    public bool LogsEnabled { get; init; }
    public string LoggingLevel { get; init; } = "WARNING";

    /// <summary>Maximum reported failed checks per rule; -1 means unlimited.</summary>
    public int MaxDisplayedFailedChecks { get; init; } = DefaultDisplayedFailedChecks;

    public bool ShowErrorMessages { get; init; } = true;
    public string Password { get; init; } = "";
    public bool ShowProgress { get; init; }
    public bool NonPdfExtension { get; init; }

    // Avoid including the password in log messages.
    public override string ToString() =>
        $"ValidatorConfiguration [recordPasses={RecordPasses}, maxFails={MaxFails}, flavour={Flavour}]";
}
