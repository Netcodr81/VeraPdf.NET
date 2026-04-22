using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeraPdf.NET.Validation.Models;

/// <summary>
/// Represents the JSON envelope returned by veraPDF CLI.
/// </summary>
public sealed class VeraPdfCliReportEnvelope
{
    /// <summary>
    /// Gets the report payload returned by veraPDF.
    /// </summary>
    public VeraPdfCliReport? Report { get; init; }
}

/// <summary>
/// Represents the top-level report section returned by veraPDF CLI.
/// </summary>
public sealed class VeraPdfCliReport
{
    /// <summary>
    /// Gets build and release metadata emitted by veraPDF.
    /// </summary>
    public VeraPdfBuildInformation? BuildInformation { get; init; }

    /// <summary>
    /// Gets per-file job reports included in the execution.
    /// </summary>
    public IReadOnlyList<VeraPdfJobReport> Jobs { get; init; } = [];

    /// <summary>
    /// Gets aggregate batch summary metrics for the run.
    /// </summary>
    public VeraPdfBatchSummary? BatchSummary { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Contains veraPDF build metadata used to identify runtime version and release details.
/// </summary>
public sealed class VeraPdfBuildInformation
{
    /// <summary>
    /// Gets release descriptors published by the executing veraPDF build.
    /// </summary>
    public IReadOnlyList<VeraPdfReleaseDetail> ReleaseDetails { get; init; } = [];
}

/// <summary>
/// Represents a single veraPDF release descriptor from build metadata.
/// </summary>
public sealed class VeraPdfReleaseDetail
{
    public string? Id { get; init; }

    public string? Version { get; init; }

    public long? BuildDate { get; init; }
}

/// <summary>
/// Represents validation output for one processed input document within a batch.
/// </summary>
public sealed class VeraPdfJobReport
{
    public VeraPdfItemDetails? ItemDetails { get; init; }

    public IReadOnlyList<VeraPdfValidationResult> ValidationResult { get; init; } = [];

    public VeraPdfProcessingTime? ProcessingTime { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Describes the input item processed by veraPDF in a single job.
/// </summary>
public sealed class VeraPdfItemDetails
{
    public string? Name { get; init; }

    public long? Size { get; init; }
}

/// <summary>
/// Represents conformance evaluation results for a specific profile execution.
/// </summary>
public sealed class VeraPdfValidationResult
{
    public VeraPdfValidationDetails? Details { get; init; }

    public string? JobEndStatus { get; init; }

    public string? ProfileName { get; init; }

    public string? Statement { get; init; }

    public bool? Compliant { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Contains aggregate pass/fail counts and rule-level summaries for one validation result.
/// </summary>
public sealed class VeraPdfValidationDetails
{
    public int? PassedRules { get; init; }

    public int? FailedRules { get; init; }

    public int? PassedChecks { get; init; }

    public int? FailedChecks { get; init; }

    public IReadOnlyList<VeraPdfRuleSummary> RuleSummaries { get; init; } = [];
}

/// <summary>
/// Represents aggregated outcome details for a single validation rule.
/// </summary>
public sealed class VeraPdfRuleSummary
{
    public string? RuleStatus { get; init; }

    public string? Specification { get; init; }

    public string? Clause { get; init; }

    public int? TestNumber { get; init; }

    public string? Status { get; init; }

    public int? FailedChecks { get; init; }

    public string? Description { get; init; }

    public string? Object { get; init; }

    public string? Test { get; init; }

    public IReadOnlyList<VeraPdfRuleCheck> Checks { get; init; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Represents an individual check execution within a rule summary.
/// </summary>
public sealed class VeraPdfRuleCheck
{
    public string? Status { get; init; }

    public string? Context { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<JsonElement> ErrorArguments { get; init; } = [];

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Captures start/finish/duration timing information reported by veraPDF.
/// </summary>
public sealed class VeraPdfProcessingTime
{
    public long? Start { get; init; }

    public long? Finish { get; init; }

    public long? Difference { get; init; }

    public string? Duration { get; init; }
}

/// <summary>
/// Represents execution-level summary metrics for all processed jobs.
/// </summary>
public sealed class VeraPdfBatchSummary
{
    public VeraPdfProcessingTime? Duration { get; init; }

    public int? TotalJobs { get; init; }

    public int? OutOfMemory { get; init; }

    public int? VeraExceptions { get; init; }

    public int? FailedEncryptedJobs { get; init; }

    public VeraPdfSimpleJobSummary? ValidationSummary { get; init; }

    public int? FailedParsingJobs { get; init; }

    public VeraPdfSimpleJobSummary? FeaturesSummary { get; init; }

    public VeraPdfSimpleJobSummary? RepairSummary { get; init; }

    public bool? MultiJob { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

/// <summary>
/// Represents a minimal grouped job summary used across multiple batch summary sections.
/// </summary>
public sealed class VeraPdfSimpleJobSummary
{
    public int? FailedJobCount { get; init; }

    public int? TotalJobCount { get; init; }

    public int? SuccessfulJobCount { get; init; }

    public int? NonCompliantPdfaCount { get; init; }

    public int? CompliantPdfaCount { get; init; }
}
