using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Identity.Web;
using Modules.Common.Features;
using Modules.Users.Features.Features.Shared.Responses;
using Modules.Users.Features.Features.Shared.Routes;

namespace Modules.Users.Features.Features.GetCurrentUser;

internal sealed class GetCurrentUserEndpoint : IEndpointModule
{
    // External ID may emit the address in any of these claims depending on the token configuration.
    private static readonly string[] EmailClaimTypes = ["email", "emails", ClaimConstants.PreferredUserName, ClaimTypes.Email];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(RouteConsts.CurrentUser, Handle)
            .RequireAuthorization()
            .Produces<CurrentUserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static IResult Handle(ClaimsPrincipal user) => Results.Ok(ToCurrentUserResponse(user));

    internal static CurrentUserResponse ToCurrentUserResponse(ClaimsPrincipal user) =>
        new(
            Id: user.GetObjectId() ?? string.Empty,
            DisplayName: user.FindFirstValue(ClaimConstants.Name) ?? string.Empty,
            Email: EmailClaimTypes.Select(user.FindFirstValue).FirstOrDefault(value => !string.IsNullOrEmpty(value)) ?? string.Empty,
            Roles: user.FindAll(ClaimConstants.Roles).Select(claim => claim.Value).Distinct().ToList());
}
