using DotnetForge.Application;
using DotnetForge.Infrastructure;
using DotnetForge.Infrastructure.BackgroundProcessing;

namespace DotnetForge.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddForgeHangfire(configuration, HangfireHostRole.Api);
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
