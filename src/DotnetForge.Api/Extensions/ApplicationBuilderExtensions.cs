using DotnetForge.Api.Middleware;
using Scalar.AspNetCore;

namespace DotnetForge.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference("/scalar", options => options
                .WithTitle(".NET Forge Clean Architecture Template")
                .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                .DisableAgent());
            app.MapDiagnosticsEndpoints();
        }

        app.MapForgeEndpoints();

        return app;
    }
}
