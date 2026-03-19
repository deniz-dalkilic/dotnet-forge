using DotnetForge.Domain.Greetings;

namespace DotnetForge.Application.Greetings;

public sealed record GreetingResponse(string Name, string Message, DateTimeOffset CreatedAtUtc)
{
    public static GreetingResponse FromDomain(Greeting greeting) =>
        new(greeting.Name, greeting.Message, greeting.CreatedAtUtc);
}
