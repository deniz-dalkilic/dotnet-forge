namespace DotnetForge.Application.Greetings;

public sealed record GreetingResponse(string Name, string Message, DateTimeOffset CreatedAtUtc);
