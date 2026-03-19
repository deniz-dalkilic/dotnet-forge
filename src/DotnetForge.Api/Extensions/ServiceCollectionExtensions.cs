using DotnetForge.Application;
using DotnetForge.Infrastructure;

namespace DotnetForge.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddProblemDetails();
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
