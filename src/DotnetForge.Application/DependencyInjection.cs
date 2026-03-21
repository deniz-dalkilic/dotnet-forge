using DotnetForge.Application.Greetings;
using DotnetForge.Application.ReferenceScenarios.Greetings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();
        services.AddScoped<IGreetingApplicationService, GreetingApplicationService>();
        services.AddScoped<IReferenceScenarioGreetingService, ReferenceScenarioGreetingService>();

        return services;
    }
}
