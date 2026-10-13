namespace Pos.Contracts.Payment;

public sealed record PaymentSucceededIntegrationEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Currency) : IntegrationEvent;
