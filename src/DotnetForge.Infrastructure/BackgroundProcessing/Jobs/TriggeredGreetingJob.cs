using Hangfire.Server;
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
        using var scope = BackgroundJobScope.Begin(_logger, nameof(TriggeredGreetingJob), correlationId, performContext);

        _logger.LogInformation(
            "Triggered greeting job executed. Greeting={Greeting} TriggerSource={TriggerSource} TimestampUtc={TimestampUtc}",
            greeting,
            triggerSource,
            DateTimeOffset.UtcNow);

        return Task.CompletedTask;
    }
}
