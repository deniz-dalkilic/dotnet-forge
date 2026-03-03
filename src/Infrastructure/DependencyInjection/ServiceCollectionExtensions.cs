using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions;
using Template.Infrastructure.Auth;
using Template.Infrastructure.Caching;
using Template.Infrastructure.Data;
using Template.Infrastructure.Messaging;
using Template.Infrastructure.Data.Repositories;

namespace Template.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IExternalIdentityRepository, ExternalIdentityRepository>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        });
        services.AddSingleton<ICache, DistributedCacheService>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddSingleton<IEventBus, RabbitMqEventBus>();

        services.Configure<KeycloakClientCredentialsOptions>(configuration.GetSection(KeycloakClientCredentialsOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var options = new KeycloakClientCredentialsOptions();
            configuration.GetSection(KeycloakClientCredentialsOptions.SectionName).Bind(options);
            return options;
        });
        services.AddHttpClient<ServiceTokenProvider>();

        return services;
    }
}
