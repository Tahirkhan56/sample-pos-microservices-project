using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.BuildingBlocks.Application.Messaging;

/// <summary>Handles a domain event in-process, inside the same unit of work that raised it.</summary>
/// <typeparam name="TDomainEvent">The domain event type.</typeparam>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken);
}
