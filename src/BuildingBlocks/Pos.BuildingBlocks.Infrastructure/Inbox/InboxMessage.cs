namespace Pos.BuildingBlocks.Infrastructure.Inbox;

/// <summary>Records that a consumer already processed a message, making consumers idempotent.</summary>
public sealed class InboxMessage
{
    public required string MessageId { get; init; }

    public required string Consumer { get; init; }

    public DateTime ProcessedOnUtc { get; init; }
}
