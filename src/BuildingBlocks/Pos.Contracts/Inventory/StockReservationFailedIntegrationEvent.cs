namespace Pos.Contracts.Inventory;

public sealed record StockReservationFailedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
