namespace Template.Application.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T evt, CancellationToken ct = default)
        where T : class;
}
