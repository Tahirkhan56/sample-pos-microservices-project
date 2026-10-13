namespace Pos.BuildingBlocks.Application.Messaging;

/// <summary>A request that reads state and never changes it.</summary>
/// <typeparam name="TResponse">The response type.</typeparam>
#pragma warning disable CA1040 // Marker interface is intentional for CQRS dispatch.
public interface IQuery<TResponse>;
#pragma warning restore CA1040
