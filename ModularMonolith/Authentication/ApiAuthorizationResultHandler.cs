using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using ModularMonolith.Routes;

namespace ModularMonolith.Authentication;

/// <summary>
/// For /api/* requests, answers a failed authorization with a 401 or 403 ProblemDetails instead of letting the
/// challenge redirect to Entra (a fetch call cannot follow that redirect). Other paths keep the default behavior.
/// </summary>
internal sealed class ApiAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded || !context.Request.Path.StartsWithSegments(RouteConsts.ApiPrefix))
        {
            return _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        var problem = authorizeResult.Forbidden
            ? Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "You do not have permission to access this resource.")
            : Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Sign in to access this resource.");

        return problem.ExecuteAsync(context);
    }
}
