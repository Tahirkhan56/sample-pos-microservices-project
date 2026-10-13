using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Ordering.Domain.Orders;

namespace Pos.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options, IDomainEventDispatcher domainEventDispatcher)
    : ServiceDbContext(options, domainEventDispatcher)
{
    public DbSet<Order> Orders => Set<Order>();
}
