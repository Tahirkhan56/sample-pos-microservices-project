using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Domain.ValueObjects;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Infrastructure.Persistence;

/// <summary>
/// Seeds demo products through the domain model, so ProductCreated events flow to Inventory
/// exactly as they would for products created via the API.
/// </summary>
public static class CatalogSeeder
{
    private static readonly (string Sku, string Name, string Category, decimal Price)[] DemoProducts =
    [
        ("COF-ESP-001", "Espresso", "Coffee", 2.50m),
        ("COF-LAT-001", "Caffe Latte", "Coffee", 3.75m),
        ("COF-CAP-001", "Cappuccino", "Coffee", 3.50m),
        ("BAK-CRO-001", "Butter Croissant", "Bakery", 2.95m),
        ("BAK-MUF-001", "Blueberry Muffin", "Bakery", 3.25m),
        ("BEV-WAT-001", "Sparkling Water 500ml", "Beverages", 1.80m),
    ];

    public static async Task SeedAsync(CatalogDbContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach ((string sku, string name, string category, decimal price) in DemoProducts)
        {
            Product product = Product.Create(sku, name, description: null, category, Money.Create(price, "USD").Value).Value;
            dbContext.Products.Add(product);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
