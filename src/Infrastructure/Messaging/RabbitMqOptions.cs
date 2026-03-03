namespace Template.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "template.events";
}
