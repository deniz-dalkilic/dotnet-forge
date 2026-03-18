using DotnetForge.Application.Greetings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetForge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();
        services.AddScoped<IGreetingApplicationService, GreetingApplicationService>();

        return services;
    }
}
