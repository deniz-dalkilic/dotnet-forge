using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Template.Application.Abstractions;
using Template.Application.UseCases.AppInfo;
using Template.Infrastructure.Caching;

namespace Template.Api.Endpoints;

public static class AppInfoEndpoints
{
    public static IEndpointRouteBuilder MapAppInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/app-info", (GetAppInfoService service) => Results.Ok(service.Execute()));

        endpoints.MapGet("/api/app-info-cached", async (GetAppInfoService service, ICache cache, CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.Build("app-info", "v1");
            var cached = await cache.GetAsync<AppInfoResponse>(cacheKey, cancellationToken);

            if (cached is not null)
            {
                return Results.Ok(cached);
            }

            var payload = service.Execute();
            await cache.SetAsync(cacheKey, payload, TimeSpan.FromSeconds(30), cancellationToken);
            return Results.Ok(payload);
        });

        return endpoints;
    }
}
