using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// Declares this service's queue (with a dead-letter queue), binds it to every subscribed event and
/// dispatches deliveries to handlers. Failed messages are retried <see cref="RabbitMqOptions.MaxRetryCount"/>
/// times and then dead-lettered.
/// </summary>
internal sealed partial class RabbitMqConsumer(
    RabbitMqConnection connection,
    IntegrationEventSubscriptions subscriptions,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConsumer> logger) : BackgroundService
{
    private const string RetryHeader = "x-retry-count";
    private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(5);

    private readonly RabbitMqOptions _options = options.Value;
    private IChannel? _channel;
    private CancellationToken _stoppingToken;

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        if (subscriptions.RoutingKeys.Count == 0)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(stoppingToken);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogConnectFailed(logger, exception, ConnectRetryDelay.TotalSeconds);
                await Task.Delay(ConnectRetryDelay, stoppingToken);
            }
        }
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        IConnection amqp = await connection.GetConnectionAsync(cancellationToken);
        _channel = await amqp.CreateChannelAsync(cancellationToken: cancellationToken);

        string deadLetterExchange = $"{_options.ExchangeName}.dlx";
        string deadLetterQueue = $"{_options.QueueName}.dlq";

        await _channel.ExchangeDeclareAsync(_options.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(deadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(deadLetterQueue, deadLetterExchange, routingKey: _options.QueueName, cancellationToken: cancellationToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = deadLetterExchange,
            ["x-dead-letter-routing-key"] = _options.QueueName,
        };
        await _channel.QueueDeclareAsync(_options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArguments, cancellationToken: cancellationToken);

        foreach (string routingKey in subscriptions.RoutingKeys)
        {
            await _channel.QueueBindAsync(_options.QueueName, _options.ExchangeName, routingKey, cancellationToken: cancellationToken);
        }

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.PrefetchCount, global: false, cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;
        await _channel.BasicConsumeAsync(_options.QueueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        string routingKeys = string.Join(", ", subscriptions.RoutingKeys);
        LogConsuming(logger, _options.QueueName, routingKeys);
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs delivery)
    {
        IChannel channel = ((AsyncEventingBasicConsumer)sender).Channel;
        string messageType = delivery.BasicProperties.Type ?? delivery.RoutingKey;

        try
        {
            if (!subscriptions.TryGet(messageType, out var handler))
            {
                LogUnknownMessage(logger, messageType);
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            MessageContext.Current = new MessageContext(delivery.BasicProperties.MessageId ?? Guid.NewGuid().ToString());
            await handler(scope.ServiceProvider, delivery.Body, _stoppingToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            await RetryOrDeadLetterAsync(channel, delivery, messageType, exception);
        }
        finally
        {
            MessageContext.Current = null;
        }
    }

    private async Task RetryOrDeadLetterAsync(IChannel channel, BasicDeliverEventArgs delivery, string messageType, Exception exception)
    {
        int retryCount = ReadRetryCount(delivery.BasicProperties);
        if (retryCount >= _options.MaxRetryCount)
        {
            LogDeadLettered(logger, exception, messageType, retryCount);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        LogRetrying(logger, exception, messageType, retryCount + 1, _options.MaxRetryCount);

        var properties = new BasicProperties(delivery.BasicProperties)
        {
            Headers = new Dictionary<string, object?>(delivery.BasicProperties.Headers ?? new Dictionary<string, object?>())
            {
                [RetryHeader] = retryCount + 1,
            },
        };

        // Re-enqueue at the tail of this service's queue via the default exchange, then ack the original.
        await channel.BasicPublishAsync(string.Empty, _options.QueueName, mandatory: false, properties, delivery.Body);
        await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
    }

    private static int ReadRetryCount(IReadOnlyBasicProperties properties) =>
        properties.Headers is not null && properties.Headers.TryGetValue(RetryHeader, out object? value) && value is not null
            ? Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture)
            : 0;

    [LoggerMessage(Level = LogLevel.Information, Message = "Consuming queue {Queue} for events: {RoutingKeys}")]
    private static partial void LogConsuming(ILogger logger, string queue, string routingKeys);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RabbitMQ unavailable, retrying in {DelaySeconds}s")]
    private static partial void LogConnectFailed(ILogger logger, Exception exception, double delaySeconds);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No handler registered for message type {MessageType}; dead-lettering")]
    private static partial void LogUnknownMessage(ILogger logger, string messageType);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Handling {MessageType} failed; retry {Attempt}/{MaxAttempts}")]
    private static partial void LogRetrying(ILogger logger, Exception exception, string messageType, int attempt, int maxAttempts);

    [LoggerMessage(Level = LogLevel.Error, Message = "Handling {MessageType} failed after {RetryCount} retries; dead-lettering")]
    private static partial void LogDeadLettered(ILogger logger, Exception exception, string messageType, int retryCount);
}
