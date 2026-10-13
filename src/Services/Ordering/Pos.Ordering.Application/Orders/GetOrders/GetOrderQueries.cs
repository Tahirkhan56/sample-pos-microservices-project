using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Application.Orders.GetOrders;

public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderResponse>;

public sealed record GetOrdersQuery(OrderStatus? Status, int Page = 1, int PageSize = 20) : IQuery<PagedResult<OrderResponse>>;

internal sealed class GetOrderByIdQueryHandler(IOrderRepository orders) : IQueryHandler<GetOrderByIdQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> HandleAsync(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        Order? order = await orders.GetByIdAsync(query.OrderId, cancellationToken);
        return order is null ? OrderErrors.NotFound(query.OrderId) : order.ToResponse();
    }
}

internal sealed class GetOrdersQueryHandler(IOrderRepository orders) : IQueryHandler<GetOrdersQuery, PagedResult<OrderResponse>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<OrderResponse>>> HandleAsync(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        (IReadOnlyList<Order> items, int totalCount) = await orders.GetPageAsync(query.Status, page, pageSize, cancellationToken);
        return new PagedResult<OrderResponse>([.. items.Select(order => order.ToResponse())], page, pageSize, totalCount);
    }
}
