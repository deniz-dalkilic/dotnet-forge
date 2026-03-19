using DotnetForge.Application.Greetings;
using DotnetForge.Infrastructure.Caching.Greetings;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DotnetForge.Infrastructure.Caching;

public static class DependencyInjection
{
    public static IServiceCollection AddForgeCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .ValidateOnStart();

        services.AddHybridCache();
        services.AddOptions<HybridCacheOptions>()
            .Configure<IOptions<CacheOptions>>((options, cacheOptionsAccessor) =>
            {
                var cacheOptions = cacheOptionsAccessor.Value;
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = cacheOptions.Hybrid.DefaultEntryExpiration,
                    LocalCacheExpiration = cacheOptions.Hybrid.LocalCacheExpiration
                };
                options.MaximumPayloadBytes = cacheOptions.Hybrid.MaximumPayloadBytes;
                options.MaximumKeyLength = cacheOptions.Hybrid.MaximumKeyLength;
            });

        services.TryAddScoped<IGreetingCache, HybridGreetingCache>();

        services.AddOptionalDistributedCaching(configuration);

        return services;
    }

    private static IServiceCollection AddOptionalDistributedCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var distributedCacheSection = configuration.GetSection($"{CacheOptions.SectionName}:{nameof(CacheOptions.Distributed)}");
        var options = distributedCacheSection.Get<DistributedCacheSettings>();

        if (options is null || !options.Enabled || string.IsNullOrWhiteSpace(options.Provider))
        {
            return services;
        }

        // Intentionally left as an extension point for future Redis / IDistributedCache providers.
        // V1 keeps in-memory HybridCache as the default and does not force a distributed dependency.
        return services;
    }
}
