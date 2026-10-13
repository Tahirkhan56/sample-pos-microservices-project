namespace Pos.Catalog.Application.Products;

internal static class CatalogCacheKeys
{
    public static string Product(Guid productId) => $"product:{productId}";
}
