using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Inventory.Domain.StockItems;

namespace Pos.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options, IDomainEventDispatcher domainEventDispatcher)
    : ServiceDbContext(options, domainEventDispatcher)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
}
