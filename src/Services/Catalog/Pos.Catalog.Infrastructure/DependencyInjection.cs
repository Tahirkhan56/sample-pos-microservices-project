using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Infrastructure;
using Pos.Catalog.Domain.Products;
using Pos.Catalog.Infrastructure.Persistence;
using Pos.Catalog.Infrastructure.Persistence.Repositories;

namespace Pos.Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException("Connection string 'CatalogDb' is not configured.");

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddMessagingInfrastructure<CatalogDbContext>(configuration);
        services.AddRedisCache(configuration, instanceName: "pos:catalog:");

        services.AddHealthChecks().AddDbContextCheck<CatalogDbContext>("catalog-db", tags: ["ready"]);

        return services;
    }
}
