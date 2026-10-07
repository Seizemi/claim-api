using ModularMonolith.Routes;

namespace ModularMonolith.Authentication;

/// <summary>
/// Rejects state-changing /api/* requests that do not carry <c>X-CSRF: 1</c>. A custom header cannot be sent
/// cross-site without a CORS preflight, and CORS is disabled, so a forged request from another site never has it.
/// </summary>
internal sealed class CsrfHeaderMiddleware(RequestDelegate next)
{
    internal const string HeaderName = "X-CSRF";
    internal const string HeaderValue = "1";

    public Task InvokeAsync(HttpContext context)
    {
        if (RequiresCsrfHeader(context.Request) && !HasCsrfHeader(context.Request))
        {
            return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Missing CSRF header",
                    detail: $"State-changing API requests must include the header '{HeaderName}: {HeaderValue}'.")
                .ExecuteAsync(context);
        }

        return next(context);
    }

    private static bool RequiresCsrfHeader(HttpRequest request) =>
        request.Path.StartsWithSegments(RouteConsts.ApiPrefix)
        && (HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method)
            || HttpMethods.IsDelete(request.Method));

    private static bool HasCsrfHeader(HttpRequest request) =>
        request.Headers.TryGetValue(HeaderName, out var values)
        && values.Count == 1
        && values[0] == HeaderValue;
}
