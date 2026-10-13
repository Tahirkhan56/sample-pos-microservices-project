using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application;

namespace Pos.Payments.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsApplication(this IServiceCollection services) =>
        services.AddApplicationHandlers(typeof(DependencyInjection).Assembly);
}
