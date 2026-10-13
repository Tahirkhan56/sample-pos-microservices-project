using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application;

namespace Pos.Inventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryApplication(this IServiceCollection services) =>
        services.AddApplicationHandlers(typeof(DependencyInjection).Assembly);
}
