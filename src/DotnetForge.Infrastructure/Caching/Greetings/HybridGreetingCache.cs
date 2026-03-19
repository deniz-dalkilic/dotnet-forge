using DotnetForge.Application.Greetings;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace DotnetForge.Infrastructure.Caching.Greetings;

public sealed class HybridGreetingCache : IGreetingCache
{
    private readonly HybridCache _hybridCache;
    private readonly CacheOptions _cacheOptions;

    public HybridGreetingCache(HybridCache hybridCache, IOptions<CacheOptions> cacheOptions)
    {
        ArgumentNullException.ThrowIfNull(hybridCache);
        ArgumentNullException.ThrowIfNull(cacheOptions);

        _hybridCache = hybridCache;
        _cacheOptions = cacheOptions.Value;
    }

    public Task<GreetingResponse?> GetOrCreateAsync(
        Guid id,
        Func<CancellationToken, Task<GreetingResponse?>> factory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (!_cacheOptions.Enabled)
        {
            return factory(cancellationToken);
        }

        return _hybridCache
            .GetOrCreateAsync(
                GetKey(id),
                factory,
                static (factory, ct) => new ValueTask<GreetingResponse?>(factory(ct)),
                cancellationToken: cancellationToken)
            .AsTask();
    }

    public Task SetAsync(
        GreetingResponse response,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!_cacheOptions.Enabled)
        {
            return Task.CompletedTask;
        }

        return _hybridCache
            .SetAsync(
                GetKey(response.Id),
                response,
                cancellationToken: cancellationToken)
            .AsTask();
    }

    public Task RemoveAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!_cacheOptions.Enabled)
        {
            return Task.CompletedTask;
        }

        return _hybridCache
            .RemoveAsync(GetKey(id), cancellationToken)
            .AsTask();
    }

    private static string GetKey(Guid id) => $"greetings:{id}";
}
