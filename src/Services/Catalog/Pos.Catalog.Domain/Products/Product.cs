using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Domain.Results;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Catalog.Domain.Products.Events;

namespace Pos.Catalog.Domain.Products;

/// <summary>A sellable item in the store catalog.</summary>
public sealed class Product : AggregateRoot<Guid>
{
    public const int SkuMaxLength = 32;
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;
    public const int CategoryMaxLength = 100;

    private Product(Guid id, string sku, string name, string? description, string category, Money price)
        : base(id)
    {
        Sku = sku;
        Name = name;
        Description = description;
        Category = category;
        Price = price;
        IsActive = true;
    }

    private Product()
    {
    }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string Category { get; private set; } = string.Empty;

    public Money Price { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public static Result<Product> Create(string sku, string name, string? description, string category, Money price)
    {
        ArgumentNullException.ThrowIfNull(price);

        if (string.IsNullOrWhiteSpace(sku))
        {
            return ProductErrors.SkuRequired;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return ProductErrors.NameRequired;
        }

        var product = new Product(
            Guid.CreateVersion7(),
            sku.Trim().ToUpperInvariant(),
            name.Trim(),
            description?.Trim(),
            category.Trim(),
            price);

        product.Raise(new ProductCreatedDomainEvent(product.Id, product.Sku, product.Name));
        return product;
    }

    public Result UpdateDetails(string name, string? description, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ProductErrors.NameRequired;
        }

        Name = name.Trim();
        Description = description?.Trim();
        Category = category.Trim();
        return Result.Success();
    }

    public Result ChangePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (!IsActive)
        {
            return ProductErrors.Inactive(Id);
        }

        if (newPrice == Price)
        {
            return Result.Success();
        }

        Money oldPrice = Price;
        Price = newPrice;
        Raise(new ProductPriceChangedDomainEvent(Id, oldPrice, newPrice));
        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return ProductErrors.Inactive(Id);
        }

        IsActive = false;
        return Result.Success();
    }
}
