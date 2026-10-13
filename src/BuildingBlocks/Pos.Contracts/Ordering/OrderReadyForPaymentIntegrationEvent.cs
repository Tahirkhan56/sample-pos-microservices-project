namespace Pos.Contracts.Ordering;

public sealed record OrderReadyForPaymentIntegrationEvent(
    Guid OrderId,
    string OrderNumber,
    decimal Amount,
    string Currency) : IntegrationEvent;
