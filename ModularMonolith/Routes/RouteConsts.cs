namespace ModularMonolith.Routes;

/// <summary>Routes the host itself relies on (middleware, SPA hosting, health checks).</summary>
internal static class RouteConsts
{
    internal const string ApiPrefix = "/api";
    internal const string AuthPrefix = "/auth";
    internal const string Liveness = "/healthz";
    internal const string Readiness = "/health";

    // Unknown /api and /auth routes answer 404 instead of reaching the SPA (index.html or the React dev server).
    internal const string UnknownApiRoute = $"{ApiPrefix}/{{**path}}";
    internal const string UnknownAuthRoute = $"{AuthPrefix}/{{**path}}";
}
