using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Authentication.Features.Features.Shared.Routes;
using Modules.Common.Features;

namespace Modules.Authentication.Features.Features.Logout;

internal sealed class LogoutEndpoint : IEndpointModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Submitted by an HTML form (browser navigation, not fetch). Anonymous so that a user whose session already
        // expired still ends their Entra session instead of being sent to sign in first. Cross-site posts carry no
        // session cookie (SameSite=Lax).
        app.MapPost(RouteConsts.Logout, Handle)
            .AllowAnonymous()
            .ExcludeFromDescription();
    }

    private static IResult Handle() =>
        Results.SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
}
