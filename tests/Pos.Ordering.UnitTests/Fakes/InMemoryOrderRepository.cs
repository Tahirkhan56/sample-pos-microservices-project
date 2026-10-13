using Pos.BuildingBlocks.Application.Abstractions;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.UnitTests.Fakes;

internal sealed class InMemoryOrderRepository : IOrderRepository, IUnitOfWork
{
    public List<Order> Orders { get; } = [];

    public int SaveChangesCalls { get; private set; }

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult(Orders.Find(order => order.Id == orderId));

    public Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPageAsync(
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        List<Order> filtered = [.. Orders.Where(order => status is null || order.Status == status)];
        return Task.FromResult<(IReadOnlyList<Order>, int)>((filtered, filtered.Count));
    }

    public void Add(Order order) => Orders.Add(order);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}
