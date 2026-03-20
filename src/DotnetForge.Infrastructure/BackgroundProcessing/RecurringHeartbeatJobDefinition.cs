using DotnetForge.Infrastructure.BackgroundProcessing.Jobs;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public sealed class RecurringHeartbeatJobDefinition : IRecurringJobDefinition
{
    public string RecurringJobId => "system:heartbeat";

    public bool IsEnabled(HangfireOptions options) => options.RecurringJobs.Enabled;

    public void Register(IRecurringJobScheduler scheduler, HangfireOptions options)
    {
        scheduler.AddOrUpdate<RecurringHeartbeatJob>(
            RecurringJobId,
            job => job.Run(null, null),
            options.RecurringJobs.HeartbeatCron);
    }
}
