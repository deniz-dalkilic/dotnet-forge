namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public interface IReferenceScenarioJobDispatcher
{
    string EnqueueGreetingFollowUp(string greetingMessage, string correlationId);
}
