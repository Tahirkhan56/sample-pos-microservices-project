using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Ordering.Application.Abstractions;

/// <summary>Synchronous query port to the Catalog service, used to snapshot prices when an order is placed.</summary>
public interface ICatalogService
{
    Task<Result<IReadOnlyList<CatalogProduct>>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
}

public sealed record CatalogProduct(Guid Id, string Sku, string Name, decimal Price, string Currency, bool IsActive);
