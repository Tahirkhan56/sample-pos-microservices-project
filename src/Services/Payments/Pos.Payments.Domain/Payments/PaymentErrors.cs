using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Payments.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error ProviderPaymentMismatch =
        Error.Conflict("Payment.ProviderMismatch", "The payment is already linked to a different provider payment.");

    public static Error NotFoundForOrder(Guid orderId) =>
        Error.NotFound("Payment.NotFound", $"No payment exists for order '{orderId}'. The order may still be reserving stock.");

    public static Error AlreadyPaid(Guid orderId) => Error.Conflict("Payment.AlreadyPaid", $"Order '{orderId}' is already paid.");

    public static Error Cancelled(Guid orderId) => Error.Conflict("Payment.Cancelled", $"The payment for order '{orderId}' was cancelled.");

    public static Error InvalidTransition(PaymentStatus from, PaymentStatus to) =>
        Error.Conflict("Payment.InvalidTransition", $"Cannot move a payment from {from} to {to}.");
}
