using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Infrastructure;
using Pos.Contracts.Catalog;
using Pos.Contracts.Ordering;
using Pos.Contracts.Payment;
using Pos.Inventory.Application.IntegrationEvents;
using Pos.Inventory.Domain.StockItems;
using Pos.Inventory.Infrastructure.Persistence;
using Pos.Inventory.Infrastructure.Persistence.Repositories;

namespace Pos.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("InventoryDb")
            ?? throw new InvalidOperationException("Connection string 'InventoryDb' is not configured.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IStockItemRepository, StockItemRepository>();

        services.AddMessagingInfrastructure<InventoryDbContext>(configuration);
        services
            .AddIntegrationEventHandler<ProductCreatedIntegrationEvent, ProductCreatedIntegrationEventHandler, InventoryDbContext>()
            .AddIntegrationEventHandler<OrderPlacedIntegrationEvent, OrderPlacedIntegrationEventHandler, InventoryDbContext>()
            .AddIntegrationEventHandler<OrderCancelledIntegrationEvent, OrderCancelledIntegrationEventHandler, InventoryDbContext>()
            .AddIntegrationEventHandler<PaymentSucceededIntegrationEvent, PaymentSucceededIntegrationEventHandler, InventoryDbContext>();

        services.AddHealthChecks().AddDbContextCheck<InventoryDbContext>("inventory-db", tags: ["ready"]);

        return services;
    }
}
