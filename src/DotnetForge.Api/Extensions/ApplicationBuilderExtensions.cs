using DotnetForge.Api.Middleware;
using DotnetForge.Infrastructure.Options;
using DotnetForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

namespace DotnetForge.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task<WebApplication> UseApiPipelineAsync(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        await app.ApplyDatabaseMigrationsAsync();

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

    private static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        var databaseOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DatabaseOptions>>().Value;
        if (!databaseOptions.ApplyMigrationsOnStartup)
        {
            app.Logger.LogInformation("Skipping database migrations on startup by configuration.");
            return;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ForgeDbContext>();

        app.Logger.LogInformation("Applying database migrations for {DbContext}.", nameof(ForgeDbContext));
        await dbContext.Database.MigrateAsync();
    }
}
