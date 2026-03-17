using DotnetForge.Api.Middleware;

namespace DotnetForge.Api.Extensions;

public static class HttpContextExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationConstants.ItemKey, out var value) && value is string correlationId)
        {
            return correlationId;
        }

        return context.TraceIdentifier;
    }
}
