using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.CreateProduct;

internal sealed class CreateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        if (await products.SkuExistsAsync(command.Sku.Trim().ToUpperInvariant(), cancellationToken))
        {
            return ProductErrors.DuplicateSku(command.Sku);
        }

        Result<Money> price = Money.Create(command.Price, command.Currency);
        if (price.IsFailure)
        {
            return price.Error;
        }

        Result<Product> product = Product.Create(command.Sku, command.Name, command.Description, command.Category, price.Value);
        if (product.IsFailure)
        {
            return product.Error;
        }

        products.Add(product.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Value.Id;
    }
}
