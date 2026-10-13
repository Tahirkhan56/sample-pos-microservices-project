using Microsoft.Extensions.Logging;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Contracts.Inventory;
using Pos.Contracts.Payment;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.IntegrationEvents;

/// <summary>Saga: stock reserved → order can be paid.</summary>
public sealed class StockReservedIntegrationEventHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    ILogger<StockReservedIntegrationEventHandler> logger) : IIntegrationEventHandler<StockReservedIntegrationEvent>
{
    public async Task HandleAsync(StockReservedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Order? order = await orders.GetByIdAsync(integrationEvent.OrderId, cancellationToken);
        if (order is null)
        {
            OrderingLog.OrderMissing(logger, integrationEvent.OrderId, nameof(StockReservedIntegrationEvent));
            return;
        }

        Result result = order.MarkAwaitingPayment();
        if (result.IsFailure)
        {
            OrderingLog.TransitionRejected(logger, order.Id, result.Error.Description);
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Saga: stock unavailable → cancel the order.</summary>
public sealed class StockReservationFailedIntegrationEventHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    ILogger<StockReservationFailedIntegrationEventHandler> logger) : IIntegrationEventHandler<StockReservationFailedIntegrationEvent>
{
    public async Task HandleAsync(StockReservationFailedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Order? order = await orders.GetByIdAsync(integrationEvent.OrderId, cancellationToken);
        if (order is null)
        {
            OrderingLog.OrderMissing(logger, integrationEvent.OrderId, nameof(StockReservationFailedIntegrationEvent));
            return;
        }

        Result result = order.Cancel(integrationEvent.Reason);
        if (result.IsFailure)
        {
            OrderingLog.TransitionRejected(logger, order.Id, result.Error.Description);
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Saga: payment captured → order is paid.</summary>
public sealed class PaymentSucceededIntegrationEventHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<PaymentSucceededIntegrationEventHandler> logger) : IIntegrationEventHandler<PaymentSucceededIntegrationEvent>
{
    public async Task HandleAsync(PaymentSucceededIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Order? order = await orders.GetByIdAsync(integrationEvent.OrderId, cancellationToken);
        if (order is null)
        {
            OrderingLog.OrderMissing(logger, integrationEvent.OrderId, nameof(PaymentSucceededIntegrationEvent));
            return;
        }

        Result result = order.MarkPaid(integrationEvent.PaymentId, timeProvider.GetUtcNow().UtcDateTime);
        if (result.IsFailure)
        {
            OrderingLog.TransitionRejected(logger, order.Id, result.Error.Description);
            return;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static partial class OrderingLog
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Order {OrderId} not found while handling {EventName}")]
    public static partial void OrderMissing(ILogger logger, Guid orderId, string eventName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order {OrderId} rejected state transition: {Reason}")]
    public static partial void TransitionRejected(ILogger logger, Guid orderId, string reason);
}
