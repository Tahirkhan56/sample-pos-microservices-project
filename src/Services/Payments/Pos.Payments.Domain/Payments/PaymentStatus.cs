namespace Pos.Payments.Domain.Payments;

public enum PaymentStatus
{
    /// <summary>Waiting for the till to start a card payment.</summary>
    Pending = 0,

    /// <summary>A provider payment (Stripe PaymentIntent) exists and is in progress.</summary>
    Processing = 1,

    Succeeded = 2,

    /// <summary>Last attempt failed; the customer may retry with another card.</summary>
    Failed = 3,

    Cancelled = 4,
}
