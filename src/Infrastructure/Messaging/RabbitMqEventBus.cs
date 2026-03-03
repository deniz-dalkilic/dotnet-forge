using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Template.Application.Abstractions;

namespace Template.Infrastructure.Messaging;

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventBus> _logger;

    public RabbitMqEventBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqEventBus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T evt, CancellationToken ct = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(evt);

        var routingKey = typeof(T).Name;
        var payload = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(payload);

        await ExecuteWithRetryAsync(async attempt =>
        {
            var factory = CreateFactory();
            await using var connection = await factory.CreateConnectionAsync(ct);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

            await channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Type = typeof(T).FullName,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: _options.Exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);

            _logger.LogInformation(
                "Published integration event {EventType} to exchange {Exchange} with routing key {RoutingKey} on attempt {Attempt}",
                typeof(T).Name,
                _options.Exchange,
                routingKey,
                attempt);
        }, ct);
    }

    private ConnectionFactory CreateFactory() => new()
    {
        HostName = _options.Host,
        Port = _options.Port,
        VirtualHost = _options.VHost,
        UserName = _options.Username,
        Password = _options.Password
    };

    private async Task ExecuteWithRetryAsync(Func<int, Task> action, CancellationToken ct)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action(attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && !ct.IsCancellationRequested)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning(
                    ex,
                    "RabbitMQ publish attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds}s",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }

        throw new InvalidOperationException("RabbitMQ publish failed after all retry attempts.");
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
