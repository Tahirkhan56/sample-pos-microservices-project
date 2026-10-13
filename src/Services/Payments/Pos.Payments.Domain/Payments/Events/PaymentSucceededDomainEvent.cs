using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Payments.Domain.Payments.Events;

public sealed record PaymentSucceededDomainEvent(Guid PaymentId, Guid OrderId, Money Amount) : DomainEvent;
