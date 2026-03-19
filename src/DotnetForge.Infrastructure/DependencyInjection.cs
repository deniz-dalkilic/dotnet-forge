using DotnetForge.Application.Greetings;
using DotnetForge.Infrastructure.Greetings;
using DotnetForge.Infrastructure.HealthChecks;
using DotnetForge.Infrastructure.Options;
using DotnetForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);
        services.AddScoped<IGreetingRepository, GreetingRepository>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                $"{DatabaseOptions.SectionName}:ConnectionString is required.")
            .ValidateOnStart();

        services.AddDbContext<ForgeDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(databaseOptions.ConnectionString, npgsql =>
            {
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "forge");
            });

            options.UseLoggerFactory(loggerFactory);

            if (databaseOptions.EnableSensitiveDataLogging)
            {
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddHealthChecks()
            .AddCheck<ForgeDbContextHealthCheck>("postgresql");

        return services;
    }
}
