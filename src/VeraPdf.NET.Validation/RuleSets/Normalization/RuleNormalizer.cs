using System;
using System.Collections.Generic;
using System.Text;

namespace VeraPdf.NET.Validation.RuleSets.Normalization;

internal static class RuleNormalizer
{
    public static string NormalizeProfileId(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new ArgumentException("Profile id cannot be empty.", nameof(profileId));

        return profileId.Trim().Replace('\\', '/');
    }

    public static string NormalizeRuleId(string ruleId)
    {
        if (string.IsNullOrWhiteSpace(ruleId))
            throw new ArgumentException("Rule id cannot be empty.", nameof(ruleId));

        return ruleId.Trim();
    }
}
