using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Inventory;
using Pos.Contracts.Ordering;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.IntegrationEvents;

/// <summary>
/// Saga step: reserve stock for every line of the order, all-or-nothing, and report the outcome.
/// </summary>
public sealed class OrderPlacedIntegrationEventHandler(
    IStockItemRepository stockItems,
    IEventBus eventBus,
    IUnitOfWork unitOfWork) : IIntegrationEventHandler<OrderPlacedIntegrationEvent>
{
    public async Task HandleAsync(OrderPlacedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        Dictionary<Guid, int> requested = integrationEvent.Items
            .GroupBy(item => item.ProductId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        Dictionary<Guid, StockItem> items = (await stockItems.GetByProductIdsAsync(requested.Keys, cancellationToken))
            .ToDictionary(item => item.ProductId);

        string? failureReason = FindFailure(requested, items);
        if (failureReason is not null)
        {
            await eventBus.PublishAsync(new StockReservationFailedIntegrationEvent(integrationEvent.OrderId, failureReason), cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        foreach ((Guid productId, int quantity) in requested)
        {
            items[productId].Reserve(integrationEvent.OrderId, quantity);
        }

        await eventBus.PublishAsync(new StockReservedIntegrationEvent(integrationEvent.OrderId), cancellationToken);

        // A concurrent reservation on the same rows raises a concurrency exception; the consumer retries the message.
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? FindFailure(Dictionary<Guid, int> requested, Dictionary<Guid, StockItem> items)
    {
        foreach ((Guid productId, int quantity) in requested)
        {
            if (!items.TryGetValue(productId, out StockItem? item))
            {
                return $"Product '{productId}' is not stocked.";
            }

            if (!item.CanReserve(quantity))
            {
                return $"Insufficient stock for '{item.ProductName}': requested {quantity}, available {item.QuantityAvailable}.";
            }
        }

        return null;
    }
}
