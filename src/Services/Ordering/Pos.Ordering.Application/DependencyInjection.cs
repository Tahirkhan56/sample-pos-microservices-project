using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application;

namespace Pos.Ordering.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderingApplication(this IServiceCollection services) =>
        services.AddApplicationHandlers(typeof(DependencyInjection).Assembly);
}
