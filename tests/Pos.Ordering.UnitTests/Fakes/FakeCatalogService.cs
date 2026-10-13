using Pos.BuildingBlocks.Domain.Results;
using Pos.Ordering.Application.Abstractions;

namespace Pos.Ordering.UnitTests.Fakes;

internal sealed class FakeCatalogService : ICatalogService
{
    public List<CatalogProduct> Products { get; } = [];

    public Error? FailWith { get; set; }

    public Task<Result<IReadOnlyList<CatalogProduct>>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        if (FailWith is not null)
        {
            return Task.FromResult(Result.Failure<IReadOnlyList<CatalogProduct>>(FailWith));
        }

        IReadOnlyList<CatalogProduct> found = [.. Products.Where(product => productIds.Contains(product.Id))];
        return Task.FromResult(Result.Success(found));
    }
}
