namespace Pos.BuildingBlocks.Application.Messaging;

#pragma warning disable CA1040 // Marker interfaces are intentional: they drive compile-time handler matching.

/// <summary>A request that changes state and returns no value.</summary>
public interface ICommand;

/// <summary>A request that changes state and returns a value.</summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommand<TResponse>;

#pragma warning restore CA1040
