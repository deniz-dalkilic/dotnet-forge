using DotnetForge.Application.ReferenceScenarios.Greetings;
using DotnetForge.Infrastructure.BackgroundProcessing;

namespace DotnetForge.Infrastructure.ReferenceScenarios;

public sealed class ReferenceScenarioJobDispatcher : IReferenceScenarioJobDispatcher
{
    private readonly IBackgroundJobDispatcher _backgroundJobDispatcher;

    public ReferenceScenarioJobDispatcher(IBackgroundJobDispatcher backgroundJobDispatcher)
    {
        _backgroundJobDispatcher = backgroundJobDispatcher;
    }

    public string EnqueueGreetingFollowUp(string greetingMessage, string correlationId)
        => _backgroundJobDispatcher.EnqueueGreeting(greetingMessage, correlationId);
}
