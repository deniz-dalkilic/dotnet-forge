using System.Diagnostics;
using Hangfire.Server;
using DotnetForge.Infrastructure.Observability;
using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure.BackgroundProcessing.Jobs;

public sealed class TriggeredGreetingJob
{
    private readonly ILogger<TriggeredGreetingJob> _logger;

    public TriggeredGreetingJob(ILogger<TriggeredGreetingJob> logger)
    {
        _logger = logger;
    }

    public Task RunAsync(string greeting, string triggerSource, string? correlationId = null, PerformContext? performContext = null)
    {
        using var activity = ForgeTelemetry.ActivitySource.StartActivity(nameof(TriggeredGreetingJob), ActivityKind.Internal);
        using var scope = BackgroundJobScope.Begin(_logger, nameof(TriggeredGreetingJob), correlationId, performContext);
        var startedAt = Stopwatch.GetTimestamp();

        activity?.SetTag("forge.job.trigger_source", triggerSource);
        ForgeTelemetry.BackgroundJobsStarted.Add(1, new KeyValuePair<string, object?>("job.name", nameof(TriggeredGreetingJob)));

        try
        {
            _logger.LogInformation(
                "Triggered greeting job executed. Greeting={Greeting} TriggerSource={TriggerSource} TimestampUtc={TimestampUtc}",
                greeting,
                triggerSource,
                DateTimeOffset.UtcNow);

            ForgeTelemetry.BackgroundJobsCompleted.Add(1, new KeyValuePair<string, object?>("job.name", nameof(TriggeredGreetingJob)));
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            ForgeTelemetry.BackgroundJobsFailed.Add(1, new KeyValuePair<string, object?>("job.name", nameof(TriggeredGreetingJob)));
            throw;
        }
        finally
        {
            ForgeTelemetry.BackgroundJobDurationMs.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                new KeyValuePair<string, object?>("job.name", nameof(TriggeredGreetingJob)));
        }
    }
}
