namespace Pos.Contracts.Payment;

public sealed record PaymentFailedIntegrationEvent(Guid PaymentId, Guid OrderId, string Reason) : IntegrationEvent;
