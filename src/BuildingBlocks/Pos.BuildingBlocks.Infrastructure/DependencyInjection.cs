using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pos.BuildingBlocks.Application.Abstractions;
using Pos.BuildingBlocks.Application.Messaging;
using Pos.BuildingBlocks.Infrastructure.Caching;
using Pos.BuildingBlocks.Infrastructure.DomainEvents;
using Pos.BuildingBlocks.Infrastructure.Inbox;
using Pos.BuildingBlocks.Infrastructure.Messaging;
using Pos.BuildingBlocks.Infrastructure.Outbox;
using Pos.BuildingBlocks.Infrastructure.Persistence;
using Pos.Contracts;

namespace Pos.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Wires the unit of work, domain-event dispatch, transactional outbox and RabbitMQ for a service whose
    /// <typeparamref name="TDbContext"/> has already been registered with its database provider.
    /// </summary>
    public static IServiceCollection AddMessagingInfrastructure<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TDbContext : ServiceDbContext
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TDbContext>());
        services.AddScoped<IEventBus, OutboxEventBus<TDbContext>>();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<OutboxProcessor<TDbContext>>();

        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton(GetOrAddSubscriptions(services));
        services.AddHostedService<RabbitMqConsumer>();

        services.AddHealthChecks()
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"]);

        return services;
    }

    /// <summary>
    /// Subscribes <typeparamref name="THandler"/> to <typeparamref name="TEvent"/>. The handler is wrapped in an
    /// inbox-based idempotency decorator so redelivered messages are processed at most once.
    /// </summary>
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler, TDbContext>(this IServiceCollection services)
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
        where TDbContext : DbContext, IMessagingDbContext
    {
        services.TryAddScoped<THandler>();
        services.AddScoped<IIntegrationEventHandler<TEvent>, IdempotentIntegrationEventHandler<TEvent, THandler, TDbContext>>();

        GetOrAddSubscriptions(services).Add(typeof(TEvent).Name, async (provider, body, cancellationToken) =>
        {
            TEvent integrationEvent = JsonSerializer.Deserialize<TEvent>(body.Span, MessagingJson.Options)
                ?? throw new InvalidOperationException($"Message body could not be deserialized to {typeof(TEvent).Name}.");

            IIntegrationEventHandler<TEvent> handler = provider.GetRequiredService<IIntegrationEventHandler<TEvent>>();
            await handler.HandleAsync(integrationEvent, cancellationToken);
        });

        return services;
    }

    /// <summary>Registers a Redis-backed <see cref="ICacheService"/>.</summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration, string instanceName)
    {
        string connectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
        });
        services.AddSingleton<ICacheService, DistributedCacheService>();
        return services;
    }

    private static IntegrationEventSubscriptions GetOrAddSubscriptions(IServiceCollection services)
    {
        ServiceDescriptor? existing = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IntegrationEventSubscriptions));
        if (existing?.ImplementationInstance is IntegrationEventSubscriptions subscriptions)
        {
            return subscriptions;
        }

        subscriptions = new IntegrationEventSubscriptions();
        services.AddSingleton(subscriptions);
        return subscriptions;
    }
}
