using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.BuildingBlocks.Infrastructure.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}
