using Pos.Contracts;

namespace Pos.BuildingBlocks.Application.Messaging;

/// <summary>Handles an integration event received from another service via the message broker.</summary>
/// <typeparam name="TIntegrationEvent">The integration event type.</typeparam>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken);
}
