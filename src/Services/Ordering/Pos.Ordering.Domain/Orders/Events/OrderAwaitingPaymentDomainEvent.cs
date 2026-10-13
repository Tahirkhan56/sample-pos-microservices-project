using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Ordering.Domain.Orders.Events;

public sealed record OrderAwaitingPaymentDomainEvent(Guid OrderId, string OrderNumber, Money Total) : DomainEvent;
