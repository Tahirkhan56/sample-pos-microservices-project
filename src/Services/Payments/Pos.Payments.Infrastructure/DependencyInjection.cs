using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pos.BuildingBlocks.Infrastructure;
using Pos.Contracts.Ordering;
using Pos.Payments.Application.Abstractions;
using Pos.Payments.Application.IntegrationEvents;
using Pos.Payments.Domain.Payments;
using Pos.Payments.Infrastructure.Gateways;
using Pos.Payments.Infrastructure.Persistence;
using Pos.Payments.Infrastructure.Persistence.Repositories;
using Stripe;

namespace Pos.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("PaymentsDb")
            ?? throw new InvalidOperationException("Connection string 'PaymentsDb' is not configured.");

        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IStripeClient>(provider =>
            new StripeClient(provider.GetRequiredService<IOptions<StripeOptions>>().Value.SecretKey));
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddMessagingInfrastructure<PaymentsDbContext>(configuration);
        services
            .AddIntegrationEventHandler<OrderReadyForPaymentIntegrationEvent, OrderReadyForPaymentIntegrationEventHandler, PaymentsDbContext>()
            .AddIntegrationEventHandler<OrderCancelledIntegrationEvent, OrderCancelledIntegrationEventHandler, PaymentsDbContext>();

        services.AddHealthChecks().AddDbContextCheck<PaymentsDbContext>("payments-db", tags: ["ready"]);

        return services;
    }
}
