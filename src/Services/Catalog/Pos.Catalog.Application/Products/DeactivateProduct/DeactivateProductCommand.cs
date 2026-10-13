using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.DeactivateProduct;

public sealed record DeactivateProductCommand(Guid ProductId) : ICommand;

internal sealed class DeactivateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork, ICacheService cache)
    : ICommandHandler<DeactivateProductCommand>
{
    public async Task<Result> HandleAsync(DeactivateProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        Result result = product.Deactivate();
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CatalogCacheKeys.Product(product.Id), cancellationToken);
        return Result.Success();
    }
}
