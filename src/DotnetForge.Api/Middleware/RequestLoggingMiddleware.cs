using System.Diagnostics;
using DotnetForge.Api.Extensions;

namespace DotnetForge.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Request completed {@RequestLog}",
                new
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path.Value,
                    StatusCode = context.Response.StatusCode,
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    CorrelationId = context.GetCorrelationId(),
                    TraceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
                });
        }
    }
}
