using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Infrastructure.Inbox;
using Pos.BuildingBlocks.Infrastructure.Outbox;

namespace Pos.BuildingBlocks.Infrastructure.Persistence;

/// <summary>A DbContext that owns outbox and inbox tables.</summary>
public interface IMessagingDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<InboxMessage> InboxMessages { get; }
}
