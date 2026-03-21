using DotnetForge.Application.Greetings;

namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public sealed record ReferenceScenarioGreetingResponse(
    Guid Id,
    string Name,
    string Message,
    DateTimeOffset CreatedAtUtc,
    string BackgroundJobId,
    string CorrelationId,
    string Scenario,
    string TriggerSource)
{
    public static ReferenceScenarioGreetingResponse Create(
        GreetingResponse greeting,
        string backgroundJobId,
        string correlationId,
        string triggerSource) =>
        new(
            greeting.Id,
            greeting.Name,
            greeting.Message,
            greeting.CreatedAtUtc,
            backgroundJobId,
            correlationId,
            "reference-scenario.greetings",
            triggerSource);
}
