using Microsoft.EntityFrameworkCore;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class StockItemRepository(InventoryDbContext dbContext) : IStockItemRepository
{
    public Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.StockItems.FirstOrDefaultAsync(item => item.ProductId == productId, cancellationToken);

    public async Task<IReadOnlyList<StockItem>> GetByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        Guid[] ids = [.. productIds];
        return await dbContext.StockItems
            .Include(item => item.Reservations.Where(reservation => reservation.Status == ReservationStatus.Active))
            .Where(item => ids.Contains(item.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockItem>> GetReservedForOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        await dbContext.StockItems
            .Include(item => item.Reservations.Where(reservation => reservation.Status == ReservationStatus.Active))
            .Where(item => item.Reservations.Any(r => r.OrderId == orderId && r.Status == ReservationStatus.Active))
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<StockItem> Items, int TotalCount)> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        IQueryable<StockItem> query = dbContext.StockItems.AsNoTracking();
        int totalCount = await query.CountAsync(cancellationToken);
        List<StockItem> items = await query
            .OrderBy(item => item.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<bool> ExistsForProductAsync(Guid productId, CancellationToken cancellationToken) =>
        dbContext.StockItems.AnyAsync(item => item.ProductId == productId, cancellationToken);

    public void Add(StockItem stockItem) => dbContext.StockItems.Add(stockItem);
}
