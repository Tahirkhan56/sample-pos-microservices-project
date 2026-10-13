using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Catalog.Domain.Products;
using Pos.Catalog.Domain.Products.Events;

namespace Pos.Catalog.UnitTests;

public sealed class ProductTests
{
    private static readonly Money Price = Money.Create(3.50m, "USD").Value;

    [Fact]
    public void Create_WithValidData_NormalizesSkuAndRaisesCreatedEvent()
    {
        Product product = Product.Create(" cof-cap-001 ", "Cappuccino", null, "Coffee", Price).Value;

        Assert.Equal("COF-CAP-001", product.Sku);
        Assert.True(product.IsActive);
        var domainEvent = Assert.IsType<ProductCreatedDomainEvent>(Assert.Single(product.DomainEvents));
        Assert.Equal(product.Id, domainEvent.ProductId);
    }

    [Fact]
    public void Create_WithoutName_ReturnsFailure()
    {
        var result = Product.Create("SKU-1", " ", null, "Coffee", Price);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameRequired, result.Error);
    }

    [Fact]
    public void ChangePrice_ToNewPrice_UpdatesPriceAndRaisesEvent()
    {
        Product product = CreateProduct();
        Money newPrice = Money.Create(4.00m, "USD").Value;

        var result = product.ChangePrice(newPrice);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPrice, product.Price);
        Assert.Single(product.DomainEvents.OfType<ProductPriceChangedDomainEvent>());
    }

    [Fact]
    public void ChangePrice_ToSamePrice_DoesNotRaiseEvent()
    {
        Product product = CreateProduct();

        product.ChangePrice(Money.Create(3.50m, "USD").Value);

        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void ChangePrice_WhenInactive_ReturnsFailure()
    {
        Product product = CreateProduct();
        product.Deactivate();

        var result = product.ChangePrice(Money.Create(9m, "USD").Value);

        Assert.True(result.IsFailure);
        Assert.Equal("Product.Inactive", result.Error.Code);
    }

    [Fact]
    public void Deactivate_Twice_ReturnsFailure()
    {
        Product product = CreateProduct();

        Assert.True(product.Deactivate().IsSuccess);
        Assert.True(product.Deactivate().IsFailure);
    }

    private static Product CreateProduct()
    {
        Product product = Product.Create("SKU-1", "Cappuccino", null, "Coffee", Price).Value;
        product.ClearDomainEvents();
        return product;
    }
}
