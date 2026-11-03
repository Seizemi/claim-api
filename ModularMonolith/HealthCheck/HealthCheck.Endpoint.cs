using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ModularMonolith.Routes;
using Modules.Common.Features;

namespace ModularMonolith.HealthCheck;

internal sealed class HealthCheckEndpoint : IEndpointModule
{
    // Set by the image build (Dockerfile, SOURCE_VERSION): the commits the running image was built from.
    internal const string VersionSetting = "App:Version";
    internal const string VersionHeader = "X-App-Version";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var version = app.ServiceProvider.GetRequiredService<IConfiguration>()[VersionSetting];

        // Liveness for the App Service health check: anonymous, runs no check (database included), exposes only the
        // version, which lets the deployment pipeline wait for the new container instead of the previous one.
        app.MapHealthChecks(RouteConsts.Liveness, new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = (context, report) =>
                {
                    if (!string.IsNullOrEmpty(version))
                    {
                        context.Response.Headers[VersionHeader] = version;
                    }

                    context.Response.ContentType = "text/plain";
                    return context.Response.WriteAsync(report.Status.ToString());
                }
            })
            .AllowAnonymous();

        // Readiness, including the database: requires a session like every other endpoint.
        app.MapHealthChecks(RouteConsts.Readiness);
    }
}
