using Microsoft.Extensions.DependencyInjection;
using VeraPdf.NET.Validation.Abstractions;
using VeraPdf.NET.Validation.Execution;
using VeraPdf.NET.Validation.Traversal;

namespace VeraPdf.NET.Validation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVeraPdfValidation(
        this IServiceCollection services,
        Action<ValidationOptions>? configure = null)
    {
        var options = new ValidationOptions();

        configure?.Invoke(options);

        services.AddSingleton(options);

        services.AddSingleton<IObjectWalker, ReflectionObjectWalker>();

        services.AddSingleton<IValidator>(sp =>
        {
            var walker = sp.GetRequiredService<IObjectWalker>();

            // You will inject rules here later
            return new Validator(new List<IRule>(), walker);
        });

        return services;
    }
}