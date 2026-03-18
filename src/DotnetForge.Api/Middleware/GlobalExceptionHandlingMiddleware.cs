using System.Diagnostics;
using DotnetForge.Api.Exceptions;
using DotnetForge.Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace DotnetForge.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemDetailsResponse(context, exception);
        }
    }

    private async Task WriteProblemDetailsResponse(HttpContext context, Exception exception)
    {
        var (statusCode, title, type) = MapException(exception);

        _logger.LogError(exception,
            "Unhandled exception mapped to HTTP {StatusCode}. CorrelationId: {CorrelationId}, TraceId: {TraceId}",
            statusCode,
            context.GetCorrelationId(),
            Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = ShouldIncludeExceptionDetail() ? exception.Message : GetSafeDetail(statusCode),
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = context.GetCorrelationId();
        problemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        if (exception is ApiValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problemDetails);
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

    private bool ShouldIncludeExceptionDetail() => _environment.IsDevelopment();

    private static string GetSafeDetail(int statusCode)
        => statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : "Request could not be processed.";
}
