using DotnetForge.Domain.Greetings;

namespace DotnetForge.Application.Greetings;

public sealed record GreetingResponse(Guid Id, string Name, string Message, DateTimeOffset CreatedAtUtc)
{
    public static GreetingResponse FromDomain(Greeting greeting) =>
        new(greeting.Id, greeting.Name, greeting.Message, greeting.CreatedAtUtc);
}
