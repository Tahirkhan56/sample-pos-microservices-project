namespace Pos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Low-level broker publisher used by the outbox relay.</summary>
public interface IMessagePublisher
{
    Task PublishAsync(string messageId, string messageType, string payload, CancellationToken cancellationToken);
}
