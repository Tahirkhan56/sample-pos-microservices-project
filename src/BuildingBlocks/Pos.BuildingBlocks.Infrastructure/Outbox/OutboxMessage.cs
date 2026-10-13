namespace Pos.BuildingBlocks.Infrastructure.Outbox;

/// <summary>An integration event persisted in the same transaction as the business change.</summary>
public sealed class OutboxMessage
{
    public Guid Id { get; init; }

    /// <summary>Event type name; used as the RabbitMQ routing key.</summary>
    public required string Type { get; init; }

    /// <summary>JSON payload.</summary>
    public required string Content { get; init; }

    public DateTime OccurredOnUtc { get; init; }

    public DateTime? ProcessedOnUtc { get; set; }

    public int Attempts { get; set; }

    public string? Error { get; set; }
}
