using ModularMonolith.Routes;

namespace ModularMonolith.Spa;

/// <summary>
/// Serves the React app from the backend's origin. Production: the build in wwwroot with a fallback to index.html.
/// Development: a YARP route (ReverseProxy section) forwards every other request to the React dev server.
/// </summary>
internal static class SpaExtensions
{
    private const string ReverseProxySectionName = "ReverseProxy";
    private const string IndexFile = "index.html";

    internal static IServiceCollection AddSpaHosting(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddReverseProxy()
                .LoadFromConfig(configuration.GetSection(ReverseProxySectionName));
        }

        return services;
    }

    /// <summary>Must run before UseRouting and authentication: the files of the React build are public.</summary>
    internal static IApplicationBuilder UseSpaStaticFiles(this IApplicationBuilder app, IHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            app.UseStaticFiles();
        }

        return app;
    }

    internal static IEndpointRouteBuilder MapSpa(this IEndpointRouteBuilder app, IHostEnvironment environment)
    {
        app.Map(RouteConsts.UnknownApiRoute, NotFound);
        app.Map(RouteConsts.UnknownAuthRoute, NotFound).AllowAnonymous();

        if (environment.IsDevelopment())
        {
            // The route itself is declared anonymous in appsettings.Development.json.
            app.MapReverseProxy();
        }
        else
        {
            app.MapFallbackToFile(IndexFile).AllowAnonymous();
        }

        return app;
    }

    private static IResult NotFound() => Results.Problem(statusCode: StatusCodes.Status404NotFound);
}
