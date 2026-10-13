using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Inventory.Domain.StockItems;

public static class StockItemErrors
{
    public static readonly Error InvalidQuantity =
        Error.Validation("Stock.InvalidQuantity", "Quantity must be greater than zero.");

    public static Error NotFound(Guid productId) =>
        Error.NotFound("Stock.NotFound", $"No stock record exists for product '{productId}'.");

    public static Error InsufficientStock(Guid productId, int requested, int available) =>
        Error.Conflict("Stock.Insufficient", $"Product '{productId}': requested {requested}, available {available}.");
}
