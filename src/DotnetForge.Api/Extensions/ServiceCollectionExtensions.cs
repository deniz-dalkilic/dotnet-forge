namespace DotnetForge.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddHealthChecks();

        return services;
    }
}
