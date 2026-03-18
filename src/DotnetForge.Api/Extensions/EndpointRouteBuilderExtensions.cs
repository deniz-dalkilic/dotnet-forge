using System.Diagnostics;
using DotnetForge.Api.Exceptions;
using DotnetForge.Application.Greetings;

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

        endpoints.MapPost("/api/greetings", async (
            GreetingRequest request,
            IGreetingApplicationService service,
            ILoggerFactory loggerFactory,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var logger = loggerFactory.CreateLogger("DotnetForge.Api.Greetings");
            logger.LogInformation("Greeting endpoint executed for {Name}", request.Name);

            var result = await service.CreateGreetingAsync(request, cancellationToken);
            if (result.IsFailure)
            {
                return Results.ValidationProblem(
                    errors: result.ValidationErrors ?? new Dictionary<string, string[]>(),
                    title: "Validation failed",
                    type: "https://datatracker.ietf.org/doc/html/rfc9457",
                    extensions: new Dictionary<string, object?>
                    {
                        ["correlationId"] = httpContext.GetCorrelationId(),
                        ["traceId"] = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier
                    });
            }

            return Results.Ok(result.Value);
        })
        .WithName("CreateGreeting")
        .WithTags("Greetings")
        .WithSummary("Creates a greeting using the application layer service.");

        endpoints.MapHealthChecks("/health/live").WithName("HealthLive").WithTags("Health");
        endpoints.MapHealthChecks("/health/ready").WithName("HealthReady").WithTags("Health");

        return endpoints;
    }

    public static IEndpointRouteBuilder MapDiagnosticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/__diagnostics/errors").WithTags("Diagnostics");

        group.MapGet("/validation", () =>
        {
            throw new ApiValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["The Name field is required."]
            });
        });

        group.MapGet("/unexpected", () => { throw new Exception("Simulated unexpected failure for diagnostics testing."); });

        return endpoints;
    }
}
