using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.Catalog.Domain.Products.Events;

public sealed record ProductCreatedDomainEvent(Guid ProductId, string Sku, string Name) : DomainEvent;
