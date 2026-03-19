namespace DotnetForge.Application.Greetings;

public interface IGreetingCache
{
    Task<GreetingResponse?> GetOrCreateAsync(
        Guid id,
        Func<CancellationToken, Task<GreetingResponse?>> factory,
        CancellationToken cancellationToken = default);

    Task SetAsync(GreetingResponse response, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
