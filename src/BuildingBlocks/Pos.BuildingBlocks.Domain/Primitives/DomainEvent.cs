namespace Pos.BuildingBlocks.Domain.Primitives;

/// <summary>Convenience base record for domain events.</summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.CreateVersion7();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}
