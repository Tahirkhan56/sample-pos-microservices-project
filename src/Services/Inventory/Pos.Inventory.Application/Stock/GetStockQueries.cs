using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Application.Stock;

public sealed record GetStockByProductIdQuery(Guid ProductId) : IQuery<StockItemResponse>;

public sealed record GetStockItemsQuery(int Page = 1, int PageSize = 20) : IQuery<PagedResult<StockItemResponse>>;

internal sealed class GetStockByProductIdQueryHandler(IStockItemRepository stockItems)
    : IQueryHandler<GetStockByProductIdQuery, StockItemResponse>
{
    public async Task<Result<StockItemResponse>> HandleAsync(GetStockByProductIdQuery query, CancellationToken cancellationToken)
    {
        StockItem? item = await stockItems.GetByProductIdAsync(query.ProductId, cancellationToken);
        return item is null ? StockItemErrors.NotFound(query.ProductId) : item.ToResponse();
    }
}

internal sealed class GetStockItemsQueryHandler(IStockItemRepository stockItems)
    : IQueryHandler<GetStockItemsQuery, PagedResult<StockItemResponse>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<StockItemResponse>>> HandleAsync(GetStockItemsQuery query, CancellationToken cancellationToken)
    {
        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        (IReadOnlyList<StockItem> items, int totalCount) = await stockItems.GetPageAsync(page, pageSize, cancellationToken);
        return new PagedResult<StockItemResponse>([.. items.Select(item => item.ToResponse())], page, pageSize, totalCount);
    }
}
