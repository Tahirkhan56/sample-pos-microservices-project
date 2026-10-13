using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Ordering.Application.Abstractions;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.Orders.PlaceOrder;

/// <summary>
/// Starts the order saga: prices the basket from the catalog, persists the order as <c>Pending</c> and
/// (via the outbox) asks Inventory to reserve stock.
/// </summary>
internal sealed class PlaceOrderCommandHandler(
    ICatalogService catalog,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ICommandHandler<PlaceOrderCommand, OrderResponse>
{
    public async Task<Result<OrderResponse>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        Guid[] productIds = [.. command.Items.Select(item => item.ProductId).Distinct()];

        Result<IReadOnlyList<CatalogProduct>> lookup = await catalog.GetProductsAsync(productIds, cancellationToken);
        if (lookup.IsFailure)
        {
            return lookup.Error;
        }

        Dictionary<Guid, CatalogProduct> products = lookup.Value.ToDictionary(product => product.Id);
        var drafts = new List<OrderLineDraft>(command.Items.Count);

        foreach (PlaceOrderItem item in command.Items)
        {
            if (!products.TryGetValue(item.ProductId, out CatalogProduct? product) || !product.IsActive)
            {
                return Error.Validation("Order.ProductUnavailable", $"Product '{item.ProductId}' is not available for sale.");
            }

            Result<Money> price = Money.Create(product.Price, product.Currency);
            if (price.IsFailure)
            {
                return price.Error;
            }

            drafts.Add(new OrderLineDraft(product.Id, product.Name, price.Value, item.Quantity));
        }

        Result<Order> order = Order.Place(command.CashierId, drafts, timeProvider.GetUtcNow().UtcDateTime);
        if (order.IsFailure)
        {
            return order.Error;
        }

        orders.Add(order.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Value.ToResponse();
    }
}
