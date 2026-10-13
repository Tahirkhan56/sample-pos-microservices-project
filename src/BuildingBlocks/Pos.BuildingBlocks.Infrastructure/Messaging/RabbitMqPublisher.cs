using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Pos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Publishes persistent JSON messages to the shared topic exchange.</summary>
internal sealed class RabbitMqPublisher(RabbitMqConnection connection, IOptions<RabbitMqOptions> options)
    : IMessagePublisher, IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IChannel? _channel;

    public async Task PublishAsync(string messageId, string messageType, string payload, CancellationToken cancellationToken)
    {
        // Channels are not thread-safe for publishing; serialize access.
        await _gate.WaitAsync(cancellationToken);
        try
        {
            IChannel channel = await GetChannelAsync(cancellationToken);
            var properties = new BasicProperties
            {
                MessageId = messageId,
                Type = messageType,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
            };

            await channel.BasicPublishAsync(
                exchange: options.Value.ExchangeName,
                routingKey: messageType,
                mandatory: false,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(payload),
                cancellationToken: cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        _gate.Dispose();
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        IConnection amqp = await connection.GetConnectionAsync(cancellationToken);
        _channel = await amqp.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            exchange: options.Value.ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        return _channel;
    }
}
