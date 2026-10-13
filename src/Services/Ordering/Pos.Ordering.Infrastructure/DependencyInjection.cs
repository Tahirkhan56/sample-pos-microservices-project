using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pos.BuildingBlocks.Infrastructure;
using Pos.Contracts.Inventory;
using Pos.Contracts.Payment;
using Pos.Ordering.Application.Abstractions;
using Pos.Ordering.Application.IntegrationEvents;
using Pos.Ordering.Domain.Orders;
using Pos.Ordering.Infrastructure.Catalog;
using Pos.Ordering.Infrastructure.Persistence;
using Pos.Ordering.Infrastructure.Persistence.Repositories;

namespace Pos.Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("OrderingDb")
            ?? throw new InvalidOperationException("Connection string 'OrderingDb' is not configured.");

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddOptions<CatalogServiceOptions>()
            .Bind(configuration.GetSection(CatalogServiceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ICatalogService, CatalogHttpService>((provider, client) =>
                client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<CatalogServiceOptions>>().Value.BaseUrl))
            .AddStandardResilienceHandler();

        services.AddMessagingInfrastructure<OrderingDbContext>(configuration);
        services
            .AddIntegrationEventHandler<StockReservedIntegrationEvent, StockReservedIntegrationEventHandler, OrderingDbContext>()
            .AddIntegrationEventHandler<StockReservationFailedIntegrationEvent, StockReservationFailedIntegrationEventHandler, OrderingDbContext>()
            .AddIntegrationEventHandler<PaymentSucceededIntegrationEvent, PaymentSucceededIntegrationEventHandler, OrderingDbContext>();

        services.AddHealthChecks().AddDbContextCheck<OrderingDbContext>("ordering-db", tags: ["ready"]);

        return services;
    }
}
