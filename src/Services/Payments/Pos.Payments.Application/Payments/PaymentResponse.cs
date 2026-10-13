using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Application.Payments;

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    string? ProviderPaymentId,
    string? FailureReason,
    DateTime CreatedOnUtc,
    DateTime? CompletedOnUtc);

internal static class PaymentMappings
{
    public static PaymentResponse ToResponse(this Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.OrderNumber,
        payment.Amount.Amount,
        payment.Amount.Currency,
        payment.Status,
        payment.ProviderPaymentId,
        payment.FailureReason,
        payment.CreatedOnUtc,
        payment.CompletedOnUtc);
}
