using DotnetForge.Infrastructure.BackgroundProcessing.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public interface IBackgroundJobDispatcher
{
    string EnqueueGreeting(string greeting, string correlationId);

    string ScheduleGreeting(string greeting, string correlationId, TimeSpan delay);
}

public sealed class HangfireBackgroundJobDispatcher : IBackgroundJobDispatcher
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<HangfireBackgroundJobDispatcher> _logger;

    public HangfireBackgroundJobDispatcher(IBackgroundJobClient backgroundJobClient, ILogger<HangfireBackgroundJobDispatcher> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    public string EnqueueGreeting(string greeting, string correlationId)
    {
        var jobId = _backgroundJobClient.Enqueue<TriggeredGreetingJob>(job =>
            job.RunAsync(greeting, "fire-and-forget", correlationId, null));

        _logger.LogInformation(
            "Queued fire-and-forget Hangfire job {HangfireJobId} with correlation {CorrelationId}.",
            jobId,
            correlationId);

        return jobId;
    }

    public string ScheduleGreeting(string greeting, string correlationId, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule<TriggeredGreetingJob>(job =>
            job.RunAsync(greeting, "scheduled", correlationId, null), delay);

        _logger.LogInformation(
            "Queued scheduled Hangfire job {HangfireJobId} with delay {DelaySeconds}s and correlation {CorrelationId}.",
            jobId,
            delay.TotalSeconds,
            correlationId);

        return jobId;
    }
}
