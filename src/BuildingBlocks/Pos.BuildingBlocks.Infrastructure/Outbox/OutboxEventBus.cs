using System.Text.Json;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Contracts;

namespace Pos.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// <see cref="IEventBus"/> that writes to the outbox table. The message is committed together with the
/// business data by <see cref="IUnitOfWork.SaveChangesAsync"/> and published later by <see cref="OutboxProcessor{TDbContext}"/>.
/// </summary>
internal sealed class OutboxEventBus<TDbContext>(TDbContext dbContext, TimeProvider timeProvider) : IEventBus
    where TDbContext : IMessagingDbContext
{
    public Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), MessagingJson.Options),
            OccurredOnUtc = timeProvider.GetUtcNow().UtcDateTime,
        });

        return Task.CompletedTask;
    }
}
