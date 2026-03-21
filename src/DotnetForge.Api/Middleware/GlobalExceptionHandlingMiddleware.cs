using System.Diagnostics;
using DotnetForge.Api.Exceptions;
using DotnetForge.Api.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DotnetForge.Api.Middleware;

public static class GlobalExceptionHandlingMiddleware
{
    public static void ConfigureProblemDetails(ProblemDetailsContext context)
    {
        var exception = context.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is null)
        {
            return;
        }

        var (statusCode, title, type) = MapException(exception);
        context.ProblemDetails.Status = statusCode;
        context.ProblemDetails.Title = title;
        context.ProblemDetails.Type = type;
        context.ProblemDetails.Detail = GetDetail(context.HttpContext, exception, statusCode);
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.GetCorrelationId();
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.HttpContext.TraceIdentifier;

        if (exception is ApiValidationException validationException)
        {
            context.ProblemDetails.Extensions["errors"] = validationException.Errors;
        }
    }

    public static void LogUnhandledException(HttpContext context, ILogger logger)
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is null)
        {
            return;
        }

        var (statusCode, _, _) = MapException(exception);
        logger.LogError(
            exception,
            "Unhandled exception mapped to HTTP {StatusCode}. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            statusCode,
            context.GetCorrelationId(),
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);
    }

    private static (int statusCode, string title, string type) MapException(Exception exception)
        => exception switch
        {
            ApiValidationException => (StatusCodes.Status400BadRequest, "Validation failed", "https://datatracker.ietf.org/doc/html/rfc9457"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found", "https://datatracker.ietf.org/doc/html/rfc9457"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", "https://datatracker.ietf.org/doc/html/rfc9457"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://datatracker.ietf.org/doc/html/rfc9457"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", "https://datatracker.ietf.org/doc/html/rfc9457"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error", "https://datatracker.ietf.org/doc/html/rfc9457")
        };

    private static string GetDetail(HttpContext context, Exception exception, int statusCode)
    {
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        return statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : "Request could not be processed.";
    }
}
