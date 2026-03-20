using Microsoft.Extensions.Logging;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public interface IRecurringJobRegistrar
{
    Task RegisterAsync(CancellationToken cancellationToken = default);
}

public sealed class HangfireRecurringJobRegistrar : IRecurringJobRegistrar
{
    private readonly IEnumerable<IRecurringJobDefinition> _definitions;
    private readonly IRecurringJobScheduler _scheduler;
    private readonly HangfireOptions _options;
    private readonly ILogger<HangfireRecurringJobRegistrar> _logger;

    public HangfireRecurringJobRegistrar(
        IEnumerable<IRecurringJobDefinition> definitions,
        IRecurringJobScheduler scheduler,
        Microsoft.Extensions.Options.IOptions<HangfireOptions> options,
        ILogger<HangfireRecurringJobRegistrar> logger)
    {
        _definitions = definitions;
        _scheduler = scheduler;
        _options = options.Value;
        _logger = logger;
    }

    public Task RegisterAsync(CancellationToken cancellationToken = default)
    {
        foreach (var definition in _definitions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!definition.IsEnabled(_options))
            {
                _logger.LogInformation("Skipping recurring Hangfire job {RecurringJobId} because it is disabled by configuration.", definition.RecurringJobId);
                continue;
            }

            definition.Register(_scheduler, _options);
            _logger.LogInformation("Registered recurring Hangfire job {RecurringJobId}.", definition.RecurringJobId);
        }

        return Task.CompletedTask;
    }
}
