using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.Contracts.Payment;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.IntegrationEvents;

/// <summary>Saga completion: reserved stock leaves the shelf once payment succeeds.</summary>
public sealed class PaymentSucceededIntegrationEventHandler(IStockItemRepository stockItems, IUnitOfWork unitOfWork)
    : IIntegrationEventHandler<PaymentSucceededIntegrationEvent>
{
    public async Task HandleAsync(PaymentSucceededIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        foreach (StockItem item in await stockItems.GetReservedForOrderAsync(integrationEvent.OrderId, cancellationToken))
        {
            item.Commit(integrationEvent.OrderId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
