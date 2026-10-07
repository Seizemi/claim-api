using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ModularMonolith.Routes;
using Modules.Common.Features;

namespace ModularMonolith.HealthCheck;

internal sealed class HealthCheckEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Liveness for the App Service health check: anonymous, runs no check (database included), exposes nothing.
        app.MapHealthChecks(RouteConsts.Liveness, new HealthCheckOptions { Predicate = _ => false })
            .AllowAnonymous();

        // Readiness, including the database: requires a session like every other endpoint.
        app.MapHealthChecks(RouteConsts.Readiness);
    }
}
