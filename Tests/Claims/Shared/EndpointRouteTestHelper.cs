using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Features;
using Xunit;

namespace Modules.Claims.Features.Tests.Shared;

internal static class EndpointRouteTestHelper
{
    /// <summary>
    /// ASP.NET Core's RequestDelegateFactory infers each Handle parameter's binding source (route/query/service/body) by
    /// checking whether the DI container knows the type as a registered service. The endpoint's real handler/validator
    /// interfaces must therefore be registered here (any implementation works, since the compiled delegate is never
    /// invoked -- only its route/HTTP-method/name metadata is inspected).
    /// </summary>
    internal static RouteEndpoint MapSingleEndpoint(IEndpointModule module, Action<IServiceCollection> configureServices)
    {
        var builder = WebApplication.CreateBuilder();
        configureServices(builder.Services);
        var app = builder.Build();

        module.AddRoutes(app);

        IEndpointRouteBuilder routeBuilder = app;
        var endpoints = routeBuilder.DataSources
            .SelectMany(dataSource => dataSource.Endpoints)
            .ToList();

        return (RouteEndpoint)Assert.Single(endpoints);
    }
}
