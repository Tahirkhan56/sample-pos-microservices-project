using Microsoft.Extensions.Logging;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Contracts.Ordering;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.IntegrationEvents;

/// <summary>Saga: stock is reserved, so open a payment the till can collect.</summary>
public sealed class OrderReadyForPaymentIntegrationEventHandler(
    IPaymentRepository payments,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IIntegrationEventHandler<OrderReadyForPaymentIntegrationEvent>
{
    public async Task HandleAsync(OrderReadyForPaymentIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (await payments.GetByOrderIdAsync(integrationEvent.OrderId, cancellationToken) is not null)
        {
            return;
        }

        Money amount = Money.Create(integrationEvent.Amount, integrationEvent.Currency).Value;
        payments.Add(Payment.Create(integrationEvent.OrderId, integrationEvent.OrderNumber, amount, timeProvider.GetUtcNow().UtcDateTime));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Compensation: the order was cancelled, so void the payment and its Stripe PaymentIntent.</summary>
public sealed partial class OrderCancelledIntegrationEventHandler(
    IPaymentRepository payments,
    IPaymentGateway gateway,
    IUnitOfWork unitOfWork,
    ILogger<OrderCancelledIntegrationEventHandler> logger) : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    public async Task HandleAsync(OrderCancelledIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Payment? payment = await payments.GetByOrderIdAsync(integrationEvent.OrderId, cancellationToken);
        if (payment is null)
        {
            return;
        }

        Result result = payment.Cancel();
        if (result.IsFailure)
        {
            LogCannotCancel(logger, payment.Id, result.Error.Description);
            return;
        }

        if (payment.ProviderPaymentId is not null)
        {
            Result cancelAtProvider = await gateway.CancelPaymentAsync(payment.ProviderPaymentId, cancellationToken);
            if (cancelAtProvider.IsFailure)
            {
                LogCannotCancel(logger, payment.Id, cancelAtProvider.Error.Description);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Payment {PaymentId} could not be cancelled: {Reason}")]
    private static partial void LogCannotCancel(ILogger logger, Guid paymentId, string reason);
}
