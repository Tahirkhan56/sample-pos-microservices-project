using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Domain.Primitives;

namespace Pos.BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>Resolves and invokes every <see cref="IDomainEventHandler{TDomainEvent}"/> for each event.</summary>
internal sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    private static readonly ConcurrentDictionary<Type, HandlerInvoker> Invokers = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            HandlerInvoker invoker = Invokers.GetOrAdd(
                domainEvent.GetType(),
                eventType => (HandlerInvoker)Activator.CreateInstance(typeof(HandlerInvoker<>).MakeGenericType(eventType))!);

            await invoker.InvokeAsync(serviceProvider, domainEvent, cancellationToken);
        }
    }

    private abstract class HandlerInvoker
    {
        public abstract Task InvokeAsync(IServiceProvider provider, IDomainEvent domainEvent, CancellationToken cancellationToken);
    }

    private sealed class HandlerInvoker<TDomainEvent> : HandlerInvoker
        where TDomainEvent : IDomainEvent
    {
        public override async Task InvokeAsync(IServiceProvider provider, IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            foreach (IDomainEventHandler<TDomainEvent> handler in provider.GetServices<IDomainEventHandler<TDomainEvent>>())
            {
                await handler.HandleAsync((TDomainEvent)domainEvent, cancellationToken);
            }
        }
    }
}
