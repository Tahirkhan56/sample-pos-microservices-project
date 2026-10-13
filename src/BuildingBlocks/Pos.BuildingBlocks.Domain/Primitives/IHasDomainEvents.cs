namespace Pos.BuildingBlocks.Domain.Primitives;

/// <summary>Implemented by aggregates that record domain events.</summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
