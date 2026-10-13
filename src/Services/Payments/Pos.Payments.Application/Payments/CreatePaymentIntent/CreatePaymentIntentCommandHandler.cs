using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.Payments.CreatePaymentIntent;

internal sealed class CreatePaymentIntentCommandHandler(
    IPaymentRepository payments,
    IPaymentGateway gateway,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<CreatePaymentIntentCommand, PaymentIntentResponse>
{
    public async Task<Result<PaymentIntentResponse>> HandleAsync(CreatePaymentIntentCommand command, CancellationToken cancellationToken)
    {
        Payment? payment = await payments.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (payment is null)
        {
            return PaymentErrors.NotFoundForOrder(command.OrderId);
        }

        Result canAccept = payment.EnsureCanAcceptPayment();
        if (canAccept.IsFailure)
        {
            return canAccept.Error;
        }

        Result<ProviderPaymentResult> providerResult = await CallGatewayAsync(payment, command.PaymentMethodId, cancellationToken);
        if (providerResult.IsFailure)
        {
            return providerResult.Error;
        }

        ProviderPaymentResult provider = providerResult.Value;
        Result attach = payment.AttachProviderPayment(provider.ProviderPaymentId);
        if (attach.IsFailure)
        {
            return attach.Error;
        }

        // Apply synchronous outcomes immediately; the webhook confirms them later (idempotently).
        Result outcome = provider.Status switch
        {
            ProviderPaymentStatus.Succeeded => payment.MarkSucceeded(timeProvider.GetUtcNow().UtcDateTime),
            ProviderPaymentStatus.Failed => payment.MarkFailed(provider.FailureReason ?? "Card payment failed."),
            _ => Result.Success(),
        };
        if (outcome.IsFailure)
        {
            return outcome.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PaymentIntentResponse(
            payment.Id,
            payment.OrderId,
            payment.Amount.Amount,
            payment.Amount.Currency,
            payment.Status,
            provider.ProviderPaymentId,
            provider.ClientSecret,
            provider.Status,
            payment.FailureReason);
    }

    private Task<Result<ProviderPaymentResult>> CallGatewayAsync(Payment payment, string? paymentMethodId, CancellationToken cancellationToken)
    {
        if (payment.ProviderPaymentId is null)
        {
            var request = new ProviderPaymentRequest(payment.Id, payment.OrderId, payment.OrderNumber, payment.Amount);
            return gateway.CreatePaymentAsync(request, paymentMethodId, cancellationToken);
        }

        return string.IsNullOrWhiteSpace(paymentMethodId)
            ? gateway.GetPaymentAsync(payment.ProviderPaymentId, cancellationToken)
            : gateway.ConfirmPaymentAsync(payment.ProviderPaymentId, paymentMethodId, cancellationToken);
    }
}
