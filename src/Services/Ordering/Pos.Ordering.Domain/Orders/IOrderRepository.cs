namespace Pos.Ordering.Domain.Orders;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPageAsync(
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    void Add(Order order);
}
