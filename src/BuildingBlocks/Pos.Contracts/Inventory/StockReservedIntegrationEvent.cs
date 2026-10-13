namespace Pos.Contracts.Inventory;

public sealed record StockReservedIntegrationEvent(Guid OrderId) : IntegrationEvent;
