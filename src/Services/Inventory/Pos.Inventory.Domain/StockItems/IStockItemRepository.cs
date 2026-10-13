namespace Pos.Inventory.Domain.StockItems;

public interface IStockItemRepository
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken);

    /// <summary>Loads stock items with their active reservations.</summary>
    Task<IReadOnlyList<StockItem>> GetByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    /// <summary>Loads stock items that hold an active reservation for <paramref name="orderId"/>.</summary>
    Task<IReadOnlyList<StockItem>> GetReservedForOrderAsync(Guid orderId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<StockItem> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken);

    void Add(StockItem stockItem);
}
