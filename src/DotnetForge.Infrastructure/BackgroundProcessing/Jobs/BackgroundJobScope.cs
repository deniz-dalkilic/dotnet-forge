using System.Diagnostics;
using Hangfire.Server;
using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure.BackgroundProcessing.Jobs;

public static class BackgroundJobScope
{
    public static IDisposable? Begin(ILogger logger, string jobName, string? correlationId, PerformContext? performContext)
    {
        var effectiveCorrelationId = string.IsNullOrWhiteSpace(correlationId)
            ? Guid.NewGuid().ToString("n")
            : correlationId;

        return logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = effectiveCorrelationId,
            ["HangfireJobId"] = performContext?.BackgroundJob?.Id,
            ["JobName"] = jobName,
            ["TraceId"] = Activity.Current?.TraceId.ToString()
        });
    }
}
