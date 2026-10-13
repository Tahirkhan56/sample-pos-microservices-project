using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Domain.Primitives;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Inbox;
using Pos.BuildingBlocks.Infrastructure.Outbox;

namespace Pos.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for every service. Acts as the unit of work, owns the outbox/inbox tables and
/// dispatches domain events before committing, so handlers can enqueue integration events atomically.
/// </summary>
public abstract class ServiceDbContext(DbContextOptions options, IDomainEventDispatcher domainEventDispatcher)
    : DbContext(options), IUnitOfWork, IMessagingDbContext
{
    private const int MaxDispatchRounds = 10;

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyMessagingConfiguration();
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        // Handlers may raise further events, so keep dispatching until the aggregates are quiet.
        for (int round = 0; round < MaxDispatchRounds; round++)
        {
            List<IDomainEvent> domainEvents = ChangeTracker
                .Entries<IHasDomainEvents>()
                .Select(entry => entry.Entity)
                .SelectMany(aggregate =>
                {
                    IDomainEvent[] events = [.. aggregate.DomainEvents];
                    aggregate.ClearDomainEvents();
                    return events;
                })
                .ToList();

            if (domainEvents.Count == 0)
            {
                return;
            }

            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        throw new InvalidOperationException($"Domain events did not settle after {MaxDispatchRounds} dispatch rounds.");
    }
}
