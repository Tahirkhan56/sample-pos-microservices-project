namespace Pos.Contracts.Catalog;

public sealed record ProductCreatedIntegrationEvent(Guid ProductId, string Sku, string Name) : IntegrationEvent;
