using System.Net;
using ModularMonolith.Testing;
using Xunit;

namespace Modules.Authentication.Features.Tests.Features.Logout;

public sealed class LogoutTests : IAsyncLifetime
{
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() => _host = await BffTestHost.StartAsync();

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Logout_DeletesSessionCookieAndRedirectsToEntraEndSession()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(TestRoles.Agent));

        var response = await _host.SendAsync(HttpMethod.Post, "/auth/logout", session);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(BffTestHost.EndSessionEndpoint, response.Headers.Location!.ToString());
        var setCookie = SessionCookie.SetCookieHeader(response);
        Assert.NotNull(setCookie);
        Assert.Contains("expires=Thu, 01 Jan 1970", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Logout_WithoutSession_StillRedirectsToEntraEndSession()
    {
        var response = await _host.SendAsync(HttpMethod.Post, "/auth/logout");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(BffTestHost.EndSessionEndpoint, response.Headers.Location!.ToString());
    }
}
