using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application;

namespace Pos.Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogApplication(this IServiceCollection services) =>
        services.AddApplicationHandlers(typeof(DependencyInjection).Assembly);
}
