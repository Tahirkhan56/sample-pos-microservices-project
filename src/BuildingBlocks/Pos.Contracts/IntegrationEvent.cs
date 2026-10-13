namespace Pos.Contracts;

/// <summary>
/// Base type for messages published to other services. The concrete type name is used as the
/// RabbitMQ routing key, so renaming an event is a breaking change.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
