using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using StackExchange.Redis;
using Template.Application.Abstractions;
using Template.Infrastructure.Auth;
using Template.Infrastructure.Caching;
using Template.Infrastructure.Data;
using Template.Infrastructure.Jobs;
using Template.Infrastructure.Messaging;

namespace Template.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<RedisCacheOptions>(configuration.GetSection(RedisCacheOptions.SectionName));
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetSection(RedisCacheOptions.SectionName)["Configuration"] ?? "localhost:6379"));

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.Configure<KeycloakClientCredentialsOptions>(configuration.GetSection(KeycloakClientCredentialsOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var options = new KeycloakClientCredentialsOptions();
            configuration.GetSection(KeycloakClientCredentialsOptions.SectionName).Bind(options);
            return options;
        });
        services.AddHttpClient<ServiceTokenProvider>();

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(HeartbeatJob));
            q.AddJob<HeartbeatJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(t => t.ForJob(jobKey).WithSimpleSchedule(s => s.WithIntervalInMinutes(1).RepeatForever()));
        });

        return services;
    }
}
