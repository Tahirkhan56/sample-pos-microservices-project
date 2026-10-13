using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Ordering;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.IntegrationEvents;

/// <summary>Compensation: return reserved stock when an order is cancelled.</summary>
public sealed class OrderCancelledIntegrationEventHandler(IStockItemRepository stockItems, IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<OrderCancelledIntegrationEvent>
{
    public async Task HandleAsync(OrderCancelledIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        foreach (StockItem item in await stockItems.GetReservedForOrderAsync(integrationEvent.OrderId, cancellationToken))
        {
            item.Release(integrationEvent.OrderId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
