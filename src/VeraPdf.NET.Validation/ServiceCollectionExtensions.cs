using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VeraPdf.NET.Parser;
using VeraPdf.NET.Validation.Configuration;
using VeraPdf.NET.Validation.Internal;
using VeraPdf.NET.Validation.Models;

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
    /// <param name="configuration">Optional application configuration used to bind runtime and execution settings.</param>
    /// <param name="configure">Optional callback used to configure runtime options.</param>
    /// <param name="configureExecution">Optional callback used to configure default execution options.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddVeraPdfValidation(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<VeraPdfRuntimeOptions>? configure = null,
        Action<ValidationExecutionOptions>? configureExecution = null)
    {
        var runtimeOptionsBuilder = services.AddOptions<VeraPdfRuntimeOptions>();
        var executionOptionsBuilder = services.AddOptions<ValidationExecutionOptions>();

        if (configuration is not null)
        {
            var runtimeSection = configuration.GetSection("VeraPdfRuntime");
            var hasRuntimeConfig = runtimeSection.Value is not null || runtimeSection.GetChildren().Any();

            if (hasRuntimeConfig)
            {
                runtimeOptionsBuilder.Bind(runtimeSection);
                runtimeOptionsBuilder
                    .Validate(options => File.Exists(options.JavaZipPath), "VeraPdfRuntime:JavaZipPath must point to an existing Java runtime archive.")
                    .Validate(options => File.Exists(options.VeraPdfZipPath), "VeraPdfRuntime:VeraPdfZipPath must point to an existing veraPDF runtime archive.")
                    .ValidateOnStart();
            }

            var executionSection = configuration.GetSection("VeraPdfExecution");
            var hasExecutionConfig = executionSection.Value is not null || executionSection.GetChildren().Any();
            if (hasExecutionConfig)
            {
                executionOptionsBuilder.Bind(executionSection);
            }
        }

        if (configure is not null)
        {
            runtimeOptionsBuilder.Configure(configure);
        }

        if (configureExecution is not null)
        {
            executionOptionsBuilder.Configure(configureExecution);
        }

        services.AddSingleton<PdfParser>();
        services.AddSingleton<IVeraPdfRuntimeProvisioner, VeraPdfRuntimeProvisioner>();
        services.AddSingleton<IProcessRunner, DefaultProcessRunner>();
        services.AddSingleton<IVeraPdfValidationService, VeraPdfValidationService>();

        return services;
    }

    /// <summary>
    /// Registers VeraPdf.NET services using an optional package-level configuration callback.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configure">Optional callback used to configure runtime and default execution options.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddVeraPDFNet(
        this IServiceCollection services,
        Action<VeraPdfNetOptions>? configure = null)
    {
        var options = new VeraPdfNetOptions();
        configure?.Invoke(options);

        return services.AddVeraPdfValidation(
            configure: runtime =>
            {
                runtime.VeraPdfZipPath = options.Runtime.VeraPdfZipPath;
                runtime.JavaZipPath = options.Runtime.JavaZipPath;
                runtime.VeraPdfZipSha256 = options.Runtime.VeraPdfZipSha256;
                runtime.JavaZipSha256 = options.Runtime.JavaZipSha256;
                runtime.RuntimeRootPath = options.Runtime.RuntimeRootPath;
                runtime.MaxConcurrentValidations = options.Runtime.MaxConcurrentValidations;
                runtime.ProcessTimeout = options.Runtime.ProcessTimeout;
                runtime.PdfAArguments = options.Runtime.PdfAArguments;
                runtime.PdfUaArguments = options.Runtime.PdfUaArguments;
                runtime.Wcag22Arguments = options.Runtime.Wcag22Arguments;
                runtime.Wcag22PolicyFilePath = options.Runtime.Wcag22PolicyFilePath;
            },
            configureExecution: execution =>
            {
                execution.Wcag22PolicyFilePathOverride = options.Execution.Wcag22PolicyFilePathOverride;

                if (options.Execution.ProfileOverrides is null)
                {
                    execution.ProfileOverrides = null;
                    return;
                }

                execution.ProfileOverrides = new ValidationProfileOverrides
                {
                    PdfAArguments = options.Execution.ProfileOverrides.PdfAArguments,
                    PdfUaArguments = options.Execution.ProfileOverrides.PdfUaArguments,
                    Wcag22Arguments = options.Execution.ProfileOverrides.Wcag22Arguments
                };
            });
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
