using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    string Category,
    decimal Price,
    string Currency,
    bool IsActive);

internal static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.Category,
        product.Price.Amount,
        product.Price.Currency,
        product.IsActive);
}
