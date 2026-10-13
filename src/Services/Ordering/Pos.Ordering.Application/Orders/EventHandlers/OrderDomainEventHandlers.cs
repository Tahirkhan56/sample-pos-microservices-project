using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Ordering;
using Pos.Ordering.Domain.Orders.Events;

namespace Pos.Ordering.Application.Orders.EventHandlers;

/// <summary>Translates internal domain events into public integration events (written to the outbox).</summary>
internal sealed class OrderPlacedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<OrderPlacedDomainEvent>
{
    public Task HandleAsync(OrderPlacedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(
            new OrderPlacedIntegrationEvent(
                domainEvent.OrderId,
                [.. domainEvent.Items.Select(item => new OrderItemContract(item.ProductId, item.Quantity))]),
            cancellationToken);
}

internal sealed class OrderAwaitingPaymentDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<OrderAwaitingPaymentDomainEvent>
{
    public Task HandleAsync(OrderAwaitingPaymentDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(
            new OrderReadyForPaymentIntegrationEvent(
                domainEvent.OrderId,
                domainEvent.OrderNumber,
                domainEvent.Total.Amount,
                domainEvent.Total.Currency),
            cancellationToken);
}

internal sealed class OrderCancelledDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<OrderCancelledDomainEvent>
{
    public Task HandleAsync(OrderCancelledDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(new OrderCancelledIntegrationEvent(domainEvent.OrderId, domainEvent.Reason), cancellationToken);
}
