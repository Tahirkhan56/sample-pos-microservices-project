using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.Ordering.Domain.Orders.Events;

public sealed record OrderCancelledDomainEvent(Guid OrderId, string Reason) : DomainEvent;
