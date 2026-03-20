using System.Linq.Expressions;
using Hangfire;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public interface IRecurringJobScheduler
{
    void AddOrUpdate<TJob>(string recurringJobId, Expression<Action<TJob>> methodCall, string cronExpression)
        where TJob : class;
}

public sealed class HangfireRecurringJobScheduler : IRecurringJobScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireRecurringJobScheduler(IRecurringJobManager recurringJobManager)
    {
        _recurringJobManager = recurringJobManager;
    }

    public void AddOrUpdate<TJob>(string recurringJobId, Expression<Action<TJob>> methodCall, string cronExpression)
        where TJob : class
    {
        _recurringJobManager.AddOrUpdate(recurringJobId, methodCall, cronExpression, new Hangfire.RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Utc
        });
    }
}
