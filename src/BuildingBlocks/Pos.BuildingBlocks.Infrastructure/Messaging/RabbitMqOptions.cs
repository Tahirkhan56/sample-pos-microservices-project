using System.ComponentModel.DataAnnotations;

namespace Pos.BuildingBlocks.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    public string HostName { get; init; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; init; } = 5672;

    [Required]
    public string UserName { get; init; } = "guest";

    [Required]
    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    /// <summary>Topic exchange shared by all services.</summary>
    [Required]
    public string ExchangeName { get; init; } = "pos.events";

    /// <summary>Durable queue owned by this service, e.g. <c>pos.inventory</c>.</summary>
    [Required]
    public string QueueName { get; init; } = string.Empty;

    [Range(1, 1000)]
    public ushort PrefetchCount { get; init; } = 16;

    /// <summary>Redeliveries before a message is dead-lettered.</summary>
    [Range(0, 20)]
    public int MaxRetryCount { get; init; } = 3;
}
