using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Payments.Domain.Payments;

namespace Pos.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options, IDomainEventDispatcher domainEventDispatcher)
    : ServiceDbContext(options, domainEventDispatcher)
{
    public DbSet<Payment> Payments => Set<Payment>();
}
