namespace Pos.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Marker for something meaningful that happened inside an aggregate.
/// Domain events are dispatched in-process before the unit of work commits.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredOnUtc { get; }
}
