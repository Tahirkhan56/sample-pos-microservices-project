using System.ComponentModel.DataAnnotations;

namespace Pos.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; init; } = 2;

    [Range(1, 1000)]
    public int BatchSize { get; init; } = 50;

    [Range(1, 100)]
    public int MaxAttempts { get; init; } = 10;
}
