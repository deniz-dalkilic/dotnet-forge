using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Template.Application.Abstractions;

namespace Template.Infrastructure.Caching;

public sealed class DistributedCacheService(IDistributedCache cache) : ICache
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await cache.GetStringAsync(key, cancellationToken);

        if (value is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value, SerializerOptions);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(value, SerializerOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        return cache.SetStringAsync(key, payload, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(key, cancellationToken);
    }
}
