using System.Diagnostics;
using Hangfire.Server;
using DotnetForge.Infrastructure.Observability;
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
        using var activity = ForgeTelemetry.ActivitySource.StartActivity(nameof(RecurringHeartbeatJob), ActivityKind.Internal);
        using var scope = BackgroundJobScope.Begin(_logger, nameof(RecurringHeartbeatJob), correlationId, performContext);
        var startedAt = Stopwatch.GetTimestamp();

        ForgeTelemetry.BackgroundJobsStarted.Add(1, new KeyValuePair<string, object?>("job.name", nameof(RecurringHeartbeatJob)));

        try
        {
            _logger.LogInformation(
                "Recurring heartbeat job executed at {TimestampUtc}. JobId={JobId}",
                performContext?.BackgroundJob?.Id ?? "unknown");

            ForgeTelemetry.BackgroundJobsCompleted.Add(1, new KeyValuePair<string, object?>("job.name", nameof(RecurringHeartbeatJob)));
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            ForgeTelemetry.BackgroundJobsFailed.Add(1, new KeyValuePair<string, object?>("job.name", nameof(RecurringHeartbeatJob)));
            throw;
        }
        finally
        {
            ForgeTelemetry.BackgroundJobDurationMs.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                new KeyValuePair<string, object?>("job.name", nameof(RecurringHeartbeatJob)));
        }
    }
}
