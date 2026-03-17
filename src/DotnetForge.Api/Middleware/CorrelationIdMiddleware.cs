using System.Diagnostics;

namespace DotnetForge.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var incomingCorrelationId = context.Request.Headers[CorrelationConstants.HeaderName].ToString();
        var correlationId = string.IsNullOrWhiteSpace(incomingCorrelationId)
            ? Guid.NewGuid().ToString("n")
            : incomingCorrelationId.Trim();

        context.Items[CorrelationConstants.ItemKey] = correlationId;
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["TraceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        });

        await _next(context);
    }
}
