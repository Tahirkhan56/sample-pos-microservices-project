using FluentValidation;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products.CreateProduct;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(Product.SkuMaxLength);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(command => command.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(command => command.Category).NotEmpty().MaximumLength(Product.CategoryMaxLength);
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.Currency).NotEmpty().Length(3);
    }
}
