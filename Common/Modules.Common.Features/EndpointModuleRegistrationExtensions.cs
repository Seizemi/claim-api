using Microsoft.AspNetCore.Routing;

namespace Modules.Common.Features;

public static class EndpointModuleRegistrationExtensions
{
    /// <summary>
    /// Maps the routes of every IEndpointModule found in the assembly containing the specified type
    /// </summary>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="marker">A type from the assembly where endpoint modules are located</param>
    /// <returns>The endpoint route builder for chaining</returns>
    public static IEndpointRouteBuilder MapEndpointModulesFromAssemblyContaining(this IEndpointRouteBuilder app, Type marker)
    {
        var assembly = marker.Assembly;

        var moduleTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                && t.IsAssignableTo(typeof(IEndpointModule)));

        foreach (var moduleType in moduleTypes)
        {
            var module = (IEndpointModule)Activator.CreateInstance(moduleType)!;
            module.AddRoutes(app);
        }

        return app;
    }
}
