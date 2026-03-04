using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Template.Application.Abstractions;
using Template.Application.Auth;
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

        services.AddOptions<ExternalAuthOptions>()
            .Bind(configuration.GetSection(ExternalAuthOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options =>
                !string.IsNullOrWhiteSpace(options.Providers.Google.ClientId) &&
                !string.IsNullOrWhiteSpace(options.Providers.Microsoft.ClientId) &&
                !string.IsNullOrWhiteSpace(options.Providers.Apple.ClientId),
                "ExternalAuth provider client ids are required.")
            .ValidateOnStart();
        services.AddSingleton<IExternalIdTokenValidator, OidcExternalIdTokenValidator>();
        services.AddScoped<ExternalSignInService>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32,
                "Jwt:SigningKey must be at least 32 bytes.")
            .ValidateOnStart();
        services.AddSingleton<IJwtTokenIssuer, JwtTokenIssuer>();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .Validate(options => options.LifetimeDays > 0, "RefreshTokens:LifetimeDays must be greater than zero.")
            .ValidateOnStart();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

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
