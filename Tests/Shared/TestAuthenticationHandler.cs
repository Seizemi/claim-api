using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace ModularMonolith.Testing;

/// <summary>
/// Replaces the session cookie in tests that run the whole application (Program.cs): the user comes from headers set
/// by <see cref="TestAuthentication.SignInAs"/>, with the same claim types Entra issues (oid, name, roles). A request
/// without these headers stays anonymous. The real cookie and OpenID Connect setup is covered by <see cref="BffTestHost"/>.
/// </summary>
public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    internal const string UserIdHeader = "X-Test-User-Id";
    internal const string UserNameHeader = "X-Test-User-Name";
    internal const string RolesHeader = "X-Test-User-Roles";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers[UserIdHeader].ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var roles = Request.Headers[RolesHeader].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Claim[] claims =
        [
            new(ClaimConstants.ObjectId, userId),
            new(ClaimConstants.Name, Request.Headers[UserNameHeader].ToString()),
            .. roles.Select(role => new Claim(ClaimConstants.Roles, role))
        ];
        var identity = new ClaimsIdentity(claims, SchemeName, ClaimConstants.Name, ClaimConstants.Roles);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
