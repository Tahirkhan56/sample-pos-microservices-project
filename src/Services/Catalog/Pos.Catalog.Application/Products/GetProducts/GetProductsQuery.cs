using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Application.Pagination;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.GetProducts;

public sealed record GetProductsQuery(string? Search, string? Category, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<ProductResponse>>;

internal sealed class GetProductsQueryHandler(IProductRepository products)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductResponse>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<PagedResult<ProductResponse>>> HandleAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        int page = Math.Max(1, query.Page);
        int pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        (IReadOnlyList<Product> items, int totalCount) =
            await products.GetPageAsync(query.Search, query.Category, page, pageSize, cancellationToken);

        return new PagedResult<ProductResponse>([.. items.Select(product => product.ToResponse())], page, pageSize, totalCount);
    }
}
