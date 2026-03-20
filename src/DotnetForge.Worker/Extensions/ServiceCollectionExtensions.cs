using DotnetForge.Application;
using DotnetForge.Infrastructure;
using DotnetForge.Infrastructure.BackgroundProcessing;

namespace DotnetForge.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddForgeHangfire(configuration, HangfireHostRole.Worker);
        services.AddHostedService<Worker>();
        services.AddHostedService<RecurringJobRegistrationWorker>();

        return services;
    }
}
