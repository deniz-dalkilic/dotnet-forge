namespace DotnetForge.Infrastructure.Caching;

public sealed class CacheOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; init; } = true;

    public HybridCacheSettings Hybrid { get; init; } = new();

    public DistributedCacheSettings Distributed { get; init; } = new();
}

public sealed class HybridCacheSettings
{
    public TimeSpan DefaultEntryExpiration { get; init; } = TimeSpan.FromMinutes(5);

    public TimeSpan LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(2);

    public int MaximumPayloadBytes { get; init; } = 1024 * 1024;

    public int MaximumKeyLength { get; init; } = 1024;
}

public sealed class DistributedCacheSettings
{
    public bool Enabled { get; init; }

    public string? Provider { get; init; }

    public string? ConnectionString { get; init; }

    public string? InstanceName { get; init; }
}
