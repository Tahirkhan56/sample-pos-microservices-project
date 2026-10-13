using Microsoft.EntityFrameworkCore;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Infrastructure.Messaging;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Contracts;

namespace Pos.BuildingBlocks.Infrastructure.Inbox;

/// <summary>
/// Decorator that skips messages this consumer already processed. The inbox row is added to the same
/// DbContext as the handler's changes, so "processed" and "business change" commit atomically.
/// </summary>
internal sealed class IdempotentIntegrationEventHandler<TEvent, THandler, TDbContext>(
    THandler inner,
    TDbContext dbContext,
    TimeProvider timeProvider) : IIntegrationEventHandler<TEvent>
    where TEvent : IntegrationEvent
    where THandler : IIntegrationEventHandler<TEvent>
    where TDbContext : DbContext, IMessagingDbContext
{
    private static readonly string ConsumerName = typeof(THandler).FullName ?? typeof(THandler).Name;

    public async Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        string messageId = MessageContext.Current?.MessageId ?? integrationEvent.Id.ToString();

        bool alreadyProcessed = await dbContext.InboxMessages
            .AnyAsync(message => message.MessageId == messageId && message.Consumer == ConsumerName, cancellationToken);
        if (alreadyProcessed)
        {
            return;
        }

        dbContext.InboxMessages.Add(new InboxMessage
        {
            MessageId = messageId,
            Consumer = ConsumerName,
            ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime,
        });

        await inner.HandleAsync(integrationEvent, cancellationToken);

        // Persists the inbox row (and anything the handler left unsaved).
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
