using DotnetForge.Infrastructure.BackgroundProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotnetForge.Worker.Tests.BackgroundProcessing;

[TestClass]
public sealed class RecurringHeartbeatJobDefinitionTests
{
    [TestMethod]
    public void Register_UsesConfiguredCronExpression()
    {
        var scheduler = new RecordingRecurringJobScheduler();
        var definition = new RecurringHeartbeatJobDefinition();
        var options = new HangfireOptions
        {
            RecurringJobs = new RecurringJobOptions
            {
                Enabled = true,
                HeartbeatCron = "*/15 * * * *"
            }
        };

        definition.Register(scheduler, options);

        Assert.AreEqual("system:heartbeat", scheduler.LastRecurringJobId);
        Assert.AreEqual("*/15 * * * *", scheduler.LastCronExpression);
    }

    private sealed class RecordingRecurringJobScheduler : IRecurringJobScheduler
    {
        public string? LastRecurringJobId { get; private set; }

        public string? LastCronExpression { get; private set; }

        public void AddOrUpdate<TJob>(string recurringJobId, System.Linq.Expressions.Expression<Action<TJob>> methodCall, string cronExpression)
            where TJob : class
        {
            LastRecurringJobId = recurringJobId;
            LastCronExpression = cronExpression;
        }
    }
}
