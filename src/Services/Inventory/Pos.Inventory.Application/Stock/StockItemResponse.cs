using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.Stock;

public sealed record StockItemResponse(
    Guid ProductId,
    string Sku,
    string ProductName,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable);

internal static class StockItemMappings
{
    public static StockItemResponse ToResponse(this StockItem item) => new(
        item.ProductId,
        item.Sku,
        item.ProductName,
        item.QuantityOnHand,
        item.QuantityReserved,
        item.QuantityAvailable);
}
