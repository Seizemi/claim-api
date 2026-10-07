using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Authentication.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Authentication.Features.Features.Login;

internal sealed class LoginEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(RouteConsts.Login, Handle)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    private static IResult Handle(string? returnUrl, HttpContext context)
    {
        var safeReturnUrl = ReturnUrlValidator.GetSafeReturnUrl(returnUrl);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            return Results.LocalRedirect(safeReturnUrl);
        }

        return Results.Challenge(
            new AuthenticationProperties { RedirectUri = safeReturnUrl },
            [OpenIdConnectDefaults.AuthenticationScheme]);
    }
}
