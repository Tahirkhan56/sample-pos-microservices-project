using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Pos.BuildingBlocks.Application.Behaviors;
using Pos.BuildingBlocks.Application.Messaging;

namespace Pos.BuildingBlocks.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers every command, query and domain-event handler plus FluentValidation validators found in
    /// <paramref name="assembly"/>. Command handlers are wrapped in a validation decorator.
    /// </summary>
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        IEnumerable<Type> concreteTypes = assembly.GetTypes().Where(type => type is { IsAbstract: false, IsInterface: false });
        foreach (Type implementation in concreteTypes)
        {
            foreach (Type contract in implementation.GetInterfaces().Where(i => i.IsGenericType))
            {
                Type definition = contract.GetGenericTypeDefinition();

                if (definition == typeof(ICommandHandler<,>))
                {
                    RegisterDecorated(services, implementation, contract, typeof(ValidationDecorators.CommandHandler<,>));
                }
                else if (definition == typeof(ICommandHandler<>))
                {
                    RegisterDecorated(services, implementation, contract, typeof(ValidationDecorators.CommandBaseHandler<>));
                }
                else if (definition == typeof(IQueryHandler<,>)
                    || definition == typeof(IDomainEventHandler<>)
                    || definition == typeof(IIntegrationEventHandler<>))
                {
                    services.AddScoped(implementation);
                    if (definition != typeof(IIntegrationEventHandler<>))
                    {
                        services.AddScoped(contract, implementation);
                    }
                }
            }
        }

        return services;
    }

    private static void RegisterDecorated(IServiceCollection services, Type implementation, Type contract, Type openDecorator)
    {
        Type decorator = openDecorator.MakeGenericType(contract.GetGenericArguments());
        services.AddScoped(implementation);
        services.AddScoped(contract, provider =>
            ActivatorUtilities.CreateInstance(provider, decorator, provider.GetRequiredService(implementation)));
    }
}
