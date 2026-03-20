using Hangfire.Server;
using Hangfire.States;
using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure.BackgroundProcessing.Jobs;

public sealed class RecurringHeartbeatJob
{
    private readonly ILogger<RecurringHeartbeatJob> _logger;

    public RecurringHeartbeatJob(ILogger<RecurringHeartbeatJob> logger)
    {
        _logger = logger;
    }

    public void Run(string? correlationId = null, PerformContext? performContext = null)
    {
        using var scope = BackgroundJobScope.Begin(
            _logger,
            nameof(RecurringHeartbeatJob),
            correlationId,
            performContext);

        var queueName = performContext?.BackgroundJob?.Job?.Queue
                        ?? EnqueuedState.DefaultQueue;

        _logger.LogInformation(
            "Recurring heartbeat job executed at {TimestampUtc}. Queue={Queue}",
            DateTimeOffset.UtcNow,
            queueName);
    }
}
