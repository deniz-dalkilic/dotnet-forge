using DotnetForge.Domain.Greetings;

namespace DotnetForge.Application.Greetings;

public interface IGreetingRepository
{
    Task AddAsync(Greeting greeting, CancellationToken cancellationToken = default);

    Task<Greeting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
