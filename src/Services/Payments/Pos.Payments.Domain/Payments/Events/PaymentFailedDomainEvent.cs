using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.Payments.Domain.Payments.Events;

public sealed record PaymentFailedDomainEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;
