using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VeraPdf.NET.Parser;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;

namespace VeraPdf.NET.Validation;

/// <summary>
/// Provides dependency injection registrations for veraPDF validation services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required to run parser prechecks and veraPDF standard validation.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Optional application configuration used to bind the VeraPdfRuntime section.</param>
    /// <param name="configure">Optional callback used to configure runtime options.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddVeraPdfValidation(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<VeraPdfRuntimeOptions>? configure = null)
    {
        var optionsBuilder = services.AddOptions<VeraPdfRuntimeOptions>();

        if (configuration is not null)
        {
            var runtimeSection = configuration.GetSection("VeraPdfRuntime");
            var hasRuntimeConfig = runtimeSection.Value is not null || runtimeSection.GetChildren().Any();

            if (hasRuntimeConfig)
            {
                optionsBuilder.Bind(runtimeSection);
                optionsBuilder
                    .Validate(options => File.Exists(options.JavaZipPath), "VeraPdfRuntime:JavaZipPath must point to an existing Java runtime archive.")
                    .Validate(options => File.Exists(options.VeraPdfZipPath), "VeraPdfRuntime:VeraPdfZipPath must point to an existing veraPDF runtime archive.")
                    .ValidateOnStart();
            }
        }

        if (configure is not null)
        {
            optionsBuilder.Configure(configure);
        }

        services.AddSingleton<PdfParser>();
        services.AddSingleton<IVeraPdfRuntimeProvisioner, VeraPdfRuntimeProvisioner>();
        services.AddSingleton<IProcessRunner, DefaultProcessRunner>();
        services.AddSingleton<IVeraPdfValidationService, VeraPdfValidationService>();

        return services;
    }

    /// <summary>
    /// Registers health checks for runtime provisioning dependencies used by validation.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddVeraPdfValidationHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks().AddCheck<VeraPdfRuntimeHealthCheck>("verapdf-runtime");
        return services;
    }
}
