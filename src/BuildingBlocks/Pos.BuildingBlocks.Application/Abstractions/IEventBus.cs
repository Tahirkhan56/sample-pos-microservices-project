using Pos.Contracts;

namespace Pos.BuildingBlocks.Application.Abstractions;

/// <summary>
/// Publishes integration events. Implementations store events in a transactional outbox,
/// so they are only sent if the surrounding unit of work commits.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
