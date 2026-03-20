using System.Linq.Expressions;

namespace DotnetForge.Infrastructure.BackgroundProcessing;

public interface IRecurringJobDefinition
{
    string RecurringJobId { get; }

    bool IsEnabled(HangfireOptions options);

    void Register(IRecurringJobScheduler scheduler, HangfireOptions options);
}
