using DotnetForge.Application.Common;

namespace DotnetForge.Application.Greetings;

public interface IGreetingApplicationService
{
    Task<Result<GreetingResponse>> CreateGreetingAsync(GreetingRequest request, CancellationToken cancellationToken = default);

    Task<Result<GreetingResponse>> GetGreetingByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
