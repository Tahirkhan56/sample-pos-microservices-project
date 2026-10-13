namespace Pos.BuildingBlocks.Infrastructure.Messaging;

/// <summary>Maps routing keys (event type names) to strongly typed handler invocations.</summary>
public sealed class IntegrationEventSubscriptions
{
    private readonly Dictionary<string, Func<IServiceProvider, ReadOnlyMemory<byte>, CancellationToken, Task>> _handlers =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> RoutingKeys => _handlers.Keys;

    internal void Add(string routingKey, Func<IServiceProvider, ReadOnlyMemory<byte>, CancellationToken, Task> handler)
    {
        if (!_handlers.TryAdd(routingKey, handler))
        {
            throw new InvalidOperationException($"A handler for '{routingKey}' is already registered.");
        }
    }

    internal bool TryGet(string routingKey, out Func<IServiceProvider, ReadOnlyMemory<byte>, CancellationToken, Task> handler) =>
        _handlers.TryGetValue(routingKey, out handler!);
}
