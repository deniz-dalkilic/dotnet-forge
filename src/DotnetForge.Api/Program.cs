using DotnetForge.Api.Extensions;
using DotnetForge.Api.Middleware;
using DotnetForge.Infrastructure.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddForgeObservability("DotnetForge.Api", "1.0.0", includeAspNetCoreInstrumentation: true);
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ProblemDetailsExceptionHandler.ConfigureProblemDetails;
});
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DotnetForge.Api.ExceptionHandler");

        var statusCode = ProblemDetailsExceptionHandler.ApplyExceptionResponse(context);
        ProblemDetailsExceptionHandler.LogUnhandledException(context, logger, statusCode);

        var problemDetailsService = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Http.IProblemDetailsService>();
        await problemDetailsService.WriteAsync(new Microsoft.AspNetCore.Http.ProblemDetailsContext
        {
            HttpContext = context
        });
    });
});

app.Logger.LogInformation("Starting {ApplicationName} in {EnvironmentName}",
    app.Environment.ApplicationName,
    app.Environment.EnvironmentName);

await app.UseApiPipelineAsync();

app.Logger.LogInformation("{ApplicationName} started and endpoints registered", app.Environment.ApplicationName);

app.Run();

public partial class Program;
