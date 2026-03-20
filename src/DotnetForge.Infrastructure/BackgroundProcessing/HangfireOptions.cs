namespace DotnetForge.Infrastructure.BackgroundProcessing;

public sealed class HangfireOptions
{
    public const string SectionName = "Hangfire";

    public string? ConnectionString { get; init; }

    public string SchemaName { get; init; } = "hangfire";

    public string DashboardPath { get; init; } = "/hangfire";

    public bool EnableDashboard { get; init; } = true;

    public bool QueueJobsViaApi { get; init; } = true;

    public ServerOptions Server { get; init; } = new();

    public RecurringJobOptions RecurringJobs { get; init; } = new();
}

public sealed class ServerOptions
{
    public string[] Queues { get; init; } = ["default"];

    public int WorkerCount { get; init; } = Math.Max(1, Environment.ProcessorCount);
}

public sealed class RecurringJobOptions
{
    public bool Enabled { get; init; } = true;

    public string HeartbeatCron { get; init; } = "*/5 * * * *";
}

public enum HangfireHostRole
{
    Api,
    Worker
}
