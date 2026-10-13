using Pos.BuildingBlocks.Domain.Results;

namespace Pos.Catalog.Domain.Products;

public static class ProductErrors
{
    public static readonly Error SkuRequired = Error.Validation("Product.SkuRequired", "SKU is required.");

    public static readonly Error NameRequired = Error.Validation("Product.NameRequired", "Name is required.");

    public static Error NotFound(Guid productId) =>
        Error.NotFound("Product.NotFound", $"Product '{productId}' was not found.");

    public static Error DuplicateSku(string sku) =>
        Error.Conflict("Product.DuplicateSku", $"A product with SKU '{sku}' already exists.");

    public static Error Inactive(Guid productId) =>
        Error.Conflict("Product.Inactive", $"Product '{productId}' is inactive.");
}
