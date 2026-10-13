namespace Pos.Contracts.Ordering;

public sealed record OrderCancelledIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
