using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;

namespace Pos.Payments.Application.Abstractions;

/// <summary>
/// Port to the card payment provider. The Stripe adapter lives in Infrastructure, so the application
/// layer never depends on the Stripe SDK and the provider can be swapped or faked in tests.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Creates a provider payment. When <paramref name="paymentMethodId"/> is supplied it is confirmed immediately.</summary>
    Task<Result<ProviderPaymentResult>> CreatePaymentAsync(
        ProviderPaymentRequest request,
        string? paymentMethodId,
        CancellationToken cancellationToken);

    Task<Result<ProviderPaymentResult>> GetPaymentAsync(string providerPaymentId, CancellationToken cancellationToken);

    Task<Result<ProviderPaymentResult>> ConfirmPaymentAsync(
        string providerPaymentId,
        string paymentMethodId,
        CancellationToken cancellationToken);

    Task<Result> CancelPaymentAsync(string providerPaymentId, CancellationToken cancellationToken);

    /// <summary>Verifies the webhook signature and translates the provider event.</summary>
    Result<PaymentWebhookEvent> ParseWebhook(string payload, string signatureHeader);
}

public sealed record ProviderPaymentRequest(Guid PaymentId, Guid OrderId, string OrderNumber, Money Amount);

public sealed record ProviderPaymentResult(
    string ProviderPaymentId,
    string? ClientSecret,
    ProviderPaymentStatus Status,
    string? FailureReason);

public enum ProviderPaymentStatus
{
    RequiresPaymentMethod = 0,
    RequiresConfirmation = 1,
    RequiresAction = 2,
    Processing = 3,
    Succeeded = 4,
    Failed = 5,
    Canceled = 6,
}

public sealed record PaymentWebhookEvent(PaymentWebhookEventKind Kind, string? ProviderPaymentId, string? FailureReason)
{
    public static readonly PaymentWebhookEvent Ignored = new(PaymentWebhookEventKind.Ignored, null, null);
}

public enum PaymentWebhookEventKind
{
    Ignored = 0,
    Succeeded = 1,
    Failed = 2,
}
