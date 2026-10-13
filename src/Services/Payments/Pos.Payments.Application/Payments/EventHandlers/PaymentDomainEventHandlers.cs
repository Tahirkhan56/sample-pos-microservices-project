using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Payment;
using Pos.Payments.Domain.Payments.Events;

namespace Pos.Payments.Application.Payments.EventHandlers;

internal sealed class PaymentSucceededDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<PaymentSucceededDomainEvent>
{
    public Task HandleAsync(PaymentSucceededDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(
            new PaymentSucceededIntegrationEvent(domainEvent.PaymentId, domainEvent.OrderId, domainEvent.Amount.Amount, domainEvent.Amount.Currency),
            cancellationToken);
}

internal sealed class PaymentFailedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<PaymentFailedDomainEvent>
{
    public Task HandleAsync(PaymentFailedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(
            new PaymentFailedIntegrationEvent(domainEvent.PaymentId, domainEvent.OrderId, domainEvent.Reason),
            cancellationToken);
}
