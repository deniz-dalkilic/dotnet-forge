using DotnetForge.Application;

namespace DotnetForge.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddApplication();
        services.AddProblemDetails();
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
