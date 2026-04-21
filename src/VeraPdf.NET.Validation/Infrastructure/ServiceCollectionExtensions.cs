using Microsoft.Extensions.DependencyInjection;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Compilation.Linq;
using VeraPdf.NET.Validation.Execution;
using VeraPdf.NET.Validation.Profiles;
using VeraPdf.NET.Validation.RuleSets.Loading;
using VeraPdf.NET.Validation.Rules;
using VeraPdf.NET.Validation.Traversal;

namespace VeraPdf.NET.Validation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeraPdfValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configureValidation = null,
        Action<ProfileOptions>? configureProfiles = null)
    {
        var validationOptions = new ValidationOptions();
        configureValidation?.Invoke(validationOptions);

        var profileOptions = new ProfileOptions();
        configureProfiles?.Invoke(profileOptions);

        services.AddSingleton(validationOptions);
        services.AddSingleton(profileOptions);

        services.AddSingleton<IObjectWalker, ReflectionObjectWalker>();
        services.AddSingleton<IRuleEngine, LinqRuleEngine>();

        services.AddSingleton<RuleSetBuilder>();
        services.AddSingleton<RuleSetCache>();
        services.AddSingleton<RuleSetProvider>();

        services.AddSingleton<IValidator>(sp =>
        {
            var walker = sp.GetRequiredService<IObjectWalker>();
            var engine = sp.GetRequiredService<IRuleEngine>();
            var ruleCompiler = new RuleCompiler(engine);

            if (!profileOptions.UseCompiledProfileArtifact)
            {
                return new Validator(Array.Empty<IRule>(), walker, validationOptions);
            }

            var provider = sp.GetRequiredService<RuleSetProvider>();

            var selectedProfileId = profileOptions.SelectedProfileId
                ?? provider.GetAvailableProfileIds().FirstOrDefault()
                ?? throw new InvalidOperationException("No validation profiles were found in the compiled profile artifact.");

            var profile = provider.GetProfile(selectedProfileId, profileOptions.IgnoreRulesWithUnknownTargetType);

            var rules = profile.Rules
                .Select(ruleCompiler.Compile)
                .ToList();

            return new Validator(rules, walker, validationOptions);
        });

        return services;
    }
}