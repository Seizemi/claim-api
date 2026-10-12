using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Features.Abstractions;

namespace Modules.Users.Features.Extensions;

internal static class HandlerRegistrationExtensions
{
    internal static IServiceCollection RegisterHandlersFromAssemblyContaining(this IServiceCollection services, Type marker)
    {
        RegisterCommandHandlers(services, marker.Assembly);
        return services;
    }

    private static void RegisterCommandHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && t.IsAssignableTo(typeof(IHandler)))
            .ToList();

        foreach (var implementationType in handlerTypes)
        {
            var interfaceType = implementationType.GetInterfaces()
                .FirstOrDefault(i => i != typeof(IHandler) && i.IsAssignableTo(typeof(IHandler)));

            if (interfaceType is not null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }
}
