using System;
using System.Collections.Concurrent;
using VeraPdf.NET.Validation.Profiles;

namespace VeraPdf.NET.Validation.RuleSets.Loading;

internal sealed class RuleSetCache
{
    private readonly ConcurrentDictionary<string, ValidationProfile> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public ValidationProfile GetOrAdd(string key, Func<string, ValidationProfile> factory)
    {
        return _cache.GetOrAdd(key, factory);
    }
}
