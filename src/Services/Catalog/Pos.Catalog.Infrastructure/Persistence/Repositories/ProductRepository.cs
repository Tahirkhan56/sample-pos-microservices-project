using Microsoft.EntityFrameworkCore;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await dbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPageAsync(
        string? search,
        string? category,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> query = dbContext.Products.AsNoTracking().Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(product => EF.Functions.ILike(product.Name, pattern) || EF.Functions.ILike(product.Sku, pattern));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(product => product.Category == category);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        List<Product> items = await query
            .OrderBy(product => product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken) =>
        dbContext.Products.AnyAsync(product => product.Sku == sku, cancellationToken);

    public void Add(Product product) => dbContext.Products.Add(product);
}
