using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Catalog.Domain.Products.Events;
using Pos.Contracts.Catalog;

namespace Pos.Catalog.Application.Products.EventHandlers;

/// <summary>Tells other services (Inventory) that a new product exists.</summary>
internal sealed class ProductCreatedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<ProductCreatedDomainEvent>
{
    public Task HandleAsync(ProductCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(
            new ProductCreatedIntegrationEvent(domainEvent.ProductId, domainEvent.Sku, domainEvent.Name),
            cancellationToken);
}
