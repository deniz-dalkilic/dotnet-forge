namespace DotnetForge.Domain.Greetings;

public sealed class Greeting
{
    private Greeting()
    {
    }

    private Greeting(Guid id, string name, string message, DateTimeOffset createdAtUtc)
    {
        Id = id;
        Name = name;
        Message = message;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Greeting Create(string name, DateTimeOffset now)
    {
        var normalizedName = name.Trim();
        return new Greeting(Guid.NewGuid(), normalizedName, $"Hello, {normalizedName}!", now);
    }
}
