using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DotnetForge.Infrastructure.Observability;

public static class ForgeTelemetry
{
    public const string ActivitySourceName = "DotnetForge.Observability";
    public const string MeterName = "DotnetForge.Observability";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    public static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> BackgroundJobsStarted = Meter.CreateCounter<long>("forge.background.jobs.started");
    public static readonly Counter<long> BackgroundJobsCompleted = Meter.CreateCounter<long>("forge.background.jobs.completed");
    public static readonly Counter<long> BackgroundJobsFailed = Meter.CreateCounter<long>("forge.background.jobs.failed");
    public static readonly Histogram<double> BackgroundJobDurationMs = Meter.CreateHistogram<double>("forge.background.jobs.duration", unit: "ms");
}
