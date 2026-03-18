namespace DotnetForge.Domain.Greetings;

public sealed record Greeting(string Name, string Message, DateTimeOffset CreatedAtUtc)
{
    public static Greeting Create(string name, DateTimeOffset now)
    {
        var normalizedName = name.Trim();
        return new Greeting(normalizedName, $"Hello, {normalizedName}!", now);
    }
}
