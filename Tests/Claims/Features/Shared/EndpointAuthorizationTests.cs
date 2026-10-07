using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;
using Xunit;

namespace Modules.Claims.Features.Tests.Features.Shared;

public sealed class EndpointAuthorizationTests
{
    [Fact]
    public void MapEndpointModules_EveryClaimsEndpoint_RequiresAgentPolicy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        RegisterHandleServices(builder.Services);
        var app = builder.Build();

        // Act
        app.MapEndpointModulesFromAssemblyContaining(typeof(DependencyInjection));

        // Assert
        IEndpointRouteBuilder routeBuilder = app;
        var endpoints = routeBuilder.DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToList();
        Assert.NotEmpty(endpoints);
        Assert.All(endpoints, endpoint =>
        {
            var policies = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Select(data => data.Policy);
            Assert.True(policies.Contains(AuthPolicies.Agent), $"{endpoint.RoutePattern.RawText} does not require the {AuthPolicies.Agent} policy.");
            Assert.Null(endpoint.Metadata.GetMetadata<IAllowAnonymous>());
        });
    }

    /// <summary>
    /// Registers every interface taken by an endpoint's Handle method so the RequestDelegateFactory infers it as a
    /// service (see EndpointRouteTestHelper). The factories are never called: only the endpoints' metadata is inspected.
    /// </summary>
    private static void RegisterHandleServices(IServiceCollection services)
    {
        var serviceTypes = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IEndpointModule)))
            .Select(type => type.GetMethod("Handle", BindingFlags.NonPublic | BindingFlags.Static))
            .OfType<MethodInfo>()
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(type => type.IsInterface)
            .Distinct();

        foreach (var serviceType in serviceTypes)
        {
            services.AddSingleton(serviceType, _ => throw new NotSupportedException());
        }
    }
}
