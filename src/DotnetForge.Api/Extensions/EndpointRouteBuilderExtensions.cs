namespace DotnetForge.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapForgeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("DotnetForge.Api.Root");
            logger.LogInformation("Root endpoint executed");

            return Results.Ok(new
            {
                service = "DotnetForge.Api",
                status = "ok",
                timestampUtc = DateTimeOffset.UtcNow
            });
        })
        .WithName("GetRoot")
        .WithTags("System")
        .WithSummary("Returns API status information.");

        endpoints.MapGet("/ping", (ILoggerFactory loggerFactory) =>
        {
            var logger = loggerFactory.CreateLogger("DotnetForge.Api.Ping");
            logger.LogInformation("Ping endpoint executed");

            return Results.Ok(new
            {
                message = "pong",
                timestampUtc = DateTimeOffset.UtcNow
            });
        })
        .WithName("Ping")
        .WithTags("System")
        .WithSummary("Simple health ping endpoint.");

        endpoints.MapHealthChecks("/health/live").WithName("HealthLive").WithTags("Health");
        endpoints.MapHealthChecks("/health/ready").WithName("HealthReady").WithTags("Health");

        return endpoints;
    }
}
