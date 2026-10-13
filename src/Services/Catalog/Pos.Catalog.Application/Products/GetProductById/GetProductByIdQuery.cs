using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductResponse>;

/// <summary>Cache-aside read: Redis first, PostgreSQL on miss.</summary>
internal sealed class GetProductByIdQueryHandler(IProductRepository products, ICacheService cache)
    : IQueryHandler<GetProductByIdQuery, ProductResponse>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<Result<ProductResponse>> HandleAsync(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        string cacheKey = CatalogCacheKeys.Product(query.ProductId);
        ProductResponse? cached = await cache.GetAsync<ProductResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        Product? product = await products.GetByIdAsync(query.ProductId, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(query.ProductId);
        }

        ProductResponse response = product.ToResponse();
        await cache.SetAsync(cacheKey, response, CacheDuration, cancellationToken);
        return response;
    }
}
