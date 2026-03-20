using DotnetForge.Infrastructure.BackgroundProcessing;

namespace DotnetForge.Worker;

public sealed class RecurringJobRegistrationWorker : BackgroundService
{
    private readonly IRecurringJobRegistrar _recurringJobRegistrar;
    private readonly ILogger<RecurringJobRegistrationWorker> _logger;

    public RecurringJobRegistrationWorker(
        IRecurringJobRegistrar recurringJobRegistrar,
        ILogger<RecurringJobRegistrationWorker> logger)
    {
        _recurringJobRegistrar = recurringJobRegistrar;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Registering recurring Hangfire jobs for worker host.");
        await _recurringJobRegistrar.RegisterAsync(stoppingToken);
        _logger.LogInformation("Recurring Hangfire job registration completed.");
    }
}
