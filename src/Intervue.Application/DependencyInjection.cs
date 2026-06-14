using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Intervue.Application.Common.Behaviors;

namespace Intervue.Application;

/// <summary>
/// Extension method to register all Application layer services in the DI container.
/// Called from Program.cs: builder.Services.AddApplication();
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        // Register pipeline behaviors (order matters: validation runs first, then exception handling)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionToResultBehavior<,>));

        return services;
    }
}
