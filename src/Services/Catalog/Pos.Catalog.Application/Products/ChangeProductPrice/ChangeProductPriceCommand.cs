using FluentValidation;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(Guid ProductId, decimal Price, string Currency) : ICommand;

internal sealed class ChangeProductPriceCommandValidator : AbstractValidator<ChangeProductPriceCommand>
{
    public ChangeProductPriceCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.Currency).NotEmpty().Length(3);
    }
}

internal sealed class ChangeProductPriceCommandHandler(IProductRepository products, IUnitOfWork unitOfWork, ICacheService cache)
    : ICommandHandler<ChangeProductPriceCommand>
{
    public async Task<Result> HandleAsync(ChangeProductPriceCommand command, CancellationToken cancellationToken)
    {
        Product? product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        Result<Money> price = Money.Create(command.Price, command.Currency);
        if (price.IsFailure)
        {
            return price;
        }

        Result result = product.ChangePrice(price.Value);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CatalogCacheKeys.Product(product.Id), cancellationToken);
        return Result.Success();
    }
}
