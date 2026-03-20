using DotnetForge.Infrastructure.BackgroundProcessing.Jobs;
using DotnetForge.Infrastructure.Options;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public static class HangfireServiceCollectionExtensions
{
    public static IServiceCollection AddForgeHangfire(
        this IServiceCollection services,
        IConfiguration configuration,
        HangfireHostRole hostRole)
    {
        services.AddOptions<HangfireOptions>()
            .Bind(configuration.GetSection(HangfireOptions.SectionName))
            .ValidateOnStart();

        services.TryAddSingleton<IBackgroundJobDispatcher, HangfireBackgroundJobDispatcher>();
        services.TryAddSingleton<IRecurringJobDefinition, RecurringHeartbeatJobDefinition>();
        services.TryAddSingleton<IRecurringJobScheduler, HangfireRecurringJobScheduler>();
        services.TryAddSingleton<IRecurringJobRegistrar, HangfireRecurringJobRegistrar>();
        services.TryAddTransient<RecurringHeartbeatJob>();
        services.TryAddTransient<TriggeredGreetingJob>();

        services.AddHangfire((serviceProvider, hangfireConfiguration) =>
        {
            var hangfireOptions = serviceProvider.GetRequiredService<IOptions<HangfireOptions>>().Value;
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var connectionString = string.IsNullOrWhiteSpace(hangfireOptions.ConnectionString)
                ? databaseOptions.ConnectionString
                : hangfireOptions.ConnectionString;

            hangfireConfiguration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(storage => storage.UseNpgsqlConnection(connectionString), new PostgreSqlStorageOptions
                {
                    SchemaName = hangfireOptions.SchemaName,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    InvisibilityTimeout = TimeSpan.FromMinutes(5)
                });
        });

        if (hostRole == HangfireHostRole.Worker)
        {
            services.AddHangfireServer((serviceProvider, options) =>
            {
                var hangfireOptions = serviceProvider.GetRequiredService<IOptions<HangfireOptions>>().Value;
                options.ServerName = $"{Environment.MachineName}:{AppDomain.CurrentDomain.FriendlyName}";
                options.WorkerCount = hangfireOptions.Server.WorkerCount;
                options.Queues = hangfireOptions.Server.Queues;
            });
        }

        return services;
    }
}
