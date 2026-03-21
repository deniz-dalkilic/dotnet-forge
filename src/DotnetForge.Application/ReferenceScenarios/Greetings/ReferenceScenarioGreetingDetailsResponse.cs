using DotnetForge.Application.Greetings;

namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public sealed record ReferenceScenarioGreetingDetailsResponse(
    Guid Id,
    string Name,
    string Message,
    DateTimeOffset CreatedAtUtc,
    string Scenario,
    string RetrievalPattern)
{
    public static ReferenceScenarioGreetingDetailsResponse Create(GreetingResponse greeting) =>
        new(
            greeting.Id,
            greeting.Name,
            greeting.Message,
            greeting.CreatedAtUtc,
            "reference-scenario.greetings",
            "cache-aside");
}
