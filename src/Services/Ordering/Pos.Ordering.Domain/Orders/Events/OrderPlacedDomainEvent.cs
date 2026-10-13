using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.Ordering.Domain.Orders.Events;

public sealed record OrderPlacedDomainEvent(Guid OrderId, IReadOnlyList<OrderPlacedDomainEvent.Item> Items) : DomainEvent
{
    public sealed record Item(Guid ProductId, int Quantity);
}
