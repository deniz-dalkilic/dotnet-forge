using DotnetForge.Application.Common;

namespace DotnetForge.Application.ReferenceScenarios.Greetings;

public interface IReferenceScenarioGreetingService
{
    Task<Result<ReferenceScenarioGreetingResponse>> ExecuteAsync(
        ReferenceScenarioGreetingRequest request,
        string correlationId,
        CancellationToken cancellationToken = default);

    Task<Result<ReferenceScenarioGreetingDetailsResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
