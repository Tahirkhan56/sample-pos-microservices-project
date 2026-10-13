using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Catalog.Domain.Products;

namespace Pos.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options, IDomainEventDispatcher domainEventDispatcher)
    : ServiceDbContext(options, domainEventDispatcher)
{
    public DbSet<Product> Products => Set<Product>();
}
