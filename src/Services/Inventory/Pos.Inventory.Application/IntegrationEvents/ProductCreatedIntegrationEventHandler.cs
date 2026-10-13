using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Catalog;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.IntegrationEvents;

/// <summary>Opens an empty stock record for every new catalog product.</summary>
public sealed class ProductCreatedIntegrationEventHandler(IStockItemRepository stockItems, IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<ProductCreatedIntegrationEvent>
{
    public async Task HandleAsync(ProductCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        if (await stockItems.ExistsForProductAsync(integrationEvent.ProductId, cancellationToken))
        {
            return;
        }

        stockItems.Add(StockItem.Create(integrationEvent.ProductId, integrationEvent.Sku, integrationEvent.Name));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
