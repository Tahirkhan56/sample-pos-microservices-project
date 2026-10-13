using Microsoft.Extensions.Logging;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.Payments.HandleWebhook;

/// <summary>Processes a signed webhook from the payment provider: the source of truth for payment outcomes.</summary>
public sealed record HandlePaymentWebhookCommand(string Payload, string SignatureHeader) : ICommand;

internal sealed partial class HandlePaymentWebhookCommandHandler(
    IPaymentGateway gateway,
    IPaymentRepository payments,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<HandlePaymentWebhookCommandHandler> logger) : ICommandHandler<HandlePaymentWebhookCommand>
{
    public async Task<Result> HandleAsync(HandlePaymentWebhookCommand command, CancellationToken cancellationToken)
    {
        Result<PaymentWebhookEvent> parsed = gateway.ParseWebhook(command.Payload, command.SignatureHeader);
        if (parsed.IsFailure)
        {
            return parsed;
        }

        PaymentWebhookEvent webhookEvent = parsed.Value;
        if (webhookEvent.Kind == PaymentWebhookEventKind.Ignored || webhookEvent.ProviderPaymentId is null)
        {
            return Result.Success();
        }

        Payment? payment = await payments.GetByProviderPaymentIdAsync(webhookEvent.ProviderPaymentId, cancellationToken);
        if (payment is null)
        {
            // Acknowledge anyway so Stripe stops retrying events that belong to another system.
            LogUnknownPayment(logger, webhookEvent.ProviderPaymentId);
            return Result.Success();
        }

        Result outcome = webhookEvent.Kind == PaymentWebhookEventKind.Succeeded
            ? payment.MarkSucceeded(timeProvider.GetUtcNow().UtcDateTime)
            : payment.MarkFailed(webhookEvent.FailureReason ?? "Card payment failed.");

        if (outcome.IsFailure)
        {
            LogRejected(logger, payment.Id, outcome.Error.Description);
            return Result.Success();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Webhook for unknown provider payment {ProviderPaymentId}")]
    private static partial void LogUnknownPayment(ILogger logger, string providerPaymentId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Webhook rejected for payment {PaymentId}: {Reason}")]
    private static partial void LogRejected(ILogger logger, Guid paymentId, string reason);
}
