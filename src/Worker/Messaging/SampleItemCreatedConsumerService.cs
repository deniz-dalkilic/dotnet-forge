using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Template.Infrastructure.Messaging;

namespace Template.Worker.Messaging;

public sealed class SampleItemCreatedConsumerService : BackgroundService
{
    private const string QueueName = "sample-item-created.worker";
    private const string RoutingKey = "SampleItemCreated";

    private readonly RabbitMqOptions _options;
    private readonly ILogger<SampleItemCreatedConsumerService> _logger;

    public SampleItemCreatedConsumerService(IOptions<RabbitMqOptions> options, ILogger<SampleItemCreatedConsumerService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reconnectAttempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            reconnectAttempt++;

            try
            {
                await ConsumeUntilConnectionDropsAsync(reconnectAttempt, stoppingToken);
                reconnectAttempt = 0;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(Math.Pow(2, reconnectAttempt), 30));
                _logger.LogWarning(
                    ex,
                    "RabbitMQ consumer loop failed on attempt {Attempt}. Reconnecting in {DelaySeconds}s",
                    reconnectAttempt,
                    delay.TotalSeconds);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }

    private async Task ConsumeUntilConnectionDropsAsync(int attempt, CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            VirtualHost = _options.VHost,
            UserName = _options.Username,
            Password = _options.Password,
        };

        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: _options.Exchange,
            routingKey: RoutingKey,
            cancellationToken: ct);

        _logger.LogInformation(
            "RabbitMQ consumer connected on attempt {Attempt}. Exchange: {Exchange}, Queue: {Queue}, RoutingKey: {RoutingKey}",
            attempt,
            _options.Exchange,
            QueueName,
            RoutingKey);

        var disconnected = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        connection.ConnectionShutdownAsync += (_, args) =>
        {
            if (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "RabbitMQ connection shutdown detected. ReplyCode: {ReplyCode}, ReplyText: {ReplyText}",
                    args.ReplyCode,
                    args.ReplyText);
                disconnected.TrySetResult();
            }

            return Task.CompletedTask;
        };

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            _logger.LogInformation("Received {RoutingKey} event payload: {Payload}", ea.RoutingKey, body);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: ct);
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: ct);

        using var registration = ct.Register(() => disconnected.TrySetResult());
        await disconnected.Task.WaitAsync(ct);
    }
}
