using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.GetProductsByIds;

/// <summary>Batch lookup used by the Ordering service to price an order in one round trip.</summary>
public sealed record GetProductsByIdsQuery(IReadOnlyCollection<Guid> ProductIds) : IQuery<IReadOnlyList<ProductResponse>>;

internal sealed class GetProductsByIdsQueryHandler(IProductRepository products)
    : IQueryHandler<GetProductsByIdsQuery, IReadOnlyList<ProductResponse>>
{
    private const int MaxIds = 200;

    public async Task<Result<IReadOnlyList<ProductResponse>>> HandleAsync(GetProductsByIdsQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductIds.Count is 0 or > MaxIds)
        {
            return Error.Validation("Product.Lookup", $"Between 1 and {MaxIds} product ids must be supplied.");
        }

        IReadOnlyList<Product> found = await products.GetByIdsAsync(query.ProductIds, cancellationToken);
        return found.Select(product => product.ToResponse()).ToList();
    }
}
