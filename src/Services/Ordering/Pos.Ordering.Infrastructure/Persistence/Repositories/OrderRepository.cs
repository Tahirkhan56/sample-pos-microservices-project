using Microsoft.EntityFrameworkCore;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Orders
            .Include(order => order.Lines)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPageAsync(
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(order => order.Status == status);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<Order> items = await query
            .Include(order => order.Lines)
            .OrderByDescending(order => order.CreatedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public void Add(Order order) => dbContext.Orders.Add(order);
}
