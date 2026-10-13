using FluentValidation;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Results;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(Guid ProductId, string Name, string? Description, string Category) : ICommand;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(command => command.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(command => command.Category).NotEmpty().MaximumLength(Product.CategoryMaxLength);
    }
}

internal sealed class UpdateProductCommandHandler(IProductRepository products, IUnitOfWork unitOfWork, ICacheService cache)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        Product? product = await products.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(command.ProductId));
        }

        Result result = product.UpdateDetails(command.Name, command.Description, command.Category);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveAsync(CatalogCacheKeys.Product(product.Id), cancellationToken);
        return Result.Success();
    }
}
