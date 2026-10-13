namespace Pos.Catalog.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPageAsync(
        string? search,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken);

    void Add(Product product);
}
