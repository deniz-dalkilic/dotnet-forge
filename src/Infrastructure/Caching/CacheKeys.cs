namespace Template.Infrastructure.Caching;

public static class CacheKeys
{
    public static string Build(params string[] segments)
    {
        return string.Join(':', segments.Where(static s => !string.IsNullOrWhiteSpace(s)).Select(static s => s.Trim()));
    }
}
