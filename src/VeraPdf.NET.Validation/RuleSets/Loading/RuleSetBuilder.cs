using System;
using System.Collections.Generic;
using System.Text;
using VeraPdf.NET.Validation.Infrastructure;
using VeraPdf.NET.Validation.Profiles;
using VeraPdf.NET.Validation.Profiles.Converters;
using VeraPdf.NET.Validation.RuleSets.Normalization;

namespace VeraPdf.NET.Validation.RuleSets.Loading;

internal sealed class RuleSetBuilder
{
    public ValidationProfile Build(string profileId, string xml, bool ignoreUnknownTargetTypes)
    {
        var parsed = VeraPdfXmlProfileConverter.Convert(xml);

        var normalizedRules = new List<RuleDefinition>();

        foreach (var rule in parsed.Rules)
        {
            try
            {
                normalizedRules.Add(new RuleDefinition
                {
                    Id = RuleNormalizer.NormalizeRuleId(rule.Id),
                    TargetTypeName = rule.TargetTypeName,
                    TargetType = TypeMapper.MapType(rule.TargetTypeName),
                    Test = rule.Test
                });
            }
            catch when (ignoreUnknownTargetTypes)
            {
                // Type not ported yet; skip until model coverage is complete.
            }
        }

        return new ValidationProfile
        {
            Name = parsed.Name,
            Rules = normalizedRules
        };
    }
}
