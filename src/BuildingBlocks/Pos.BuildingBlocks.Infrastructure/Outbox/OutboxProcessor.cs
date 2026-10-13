using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pos.BuildingBlocks.Infrastructure.Messaging;
using Pos.BuildingBlocks.Infrastructure.Persistence;

namespace Pos.BuildingBlocks.Infrastructure.Outbox;

/// <summary>Background worker that relays unprocessed outbox messages to RabbitMQ.</summary>
internal sealed partial class OutboxProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory,
    IMessagePublisher publisher,
    TimeProvider timeProvider,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext, IMessagingDbContext
{
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), timeProvider);

        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogBatchFailed(logger, exception);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        List<OutboxMessage> messages = await dbContext.OutboxMessages
            .Where(message => message.ProcessedOnUtc == null && message.Attempts < _options.MaxAttempts)
            .OrderBy(message => message.OccurredOnUtc)
            .Take(_options.BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (OutboxMessage message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.Id.ToString(), message.Type, message.Content, cancellationToken);
                message.ProcessedOnUtc = timeProvider.GetUtcNow().UtcDateTime;
                message.Error = null;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.Attempts++;
                message.Error = exception.Message;
                LogPublishFailed(logger, exception, message.Id, message.Type, message.Attempts);
            }
        }

        // Base SaveChanges: outbox bookkeeping must not trigger domain event dispatch.
        await dbContext.SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox batch processing failed")]
    private static partial void LogBatchFailed(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Publishing outbox message {MessageId} ({MessageType}) failed, attempt {Attempt}")]
    private static partial void LogPublishFailed(ILogger logger, Exception exception, Guid messageId, string messageType, int attempt);
}
