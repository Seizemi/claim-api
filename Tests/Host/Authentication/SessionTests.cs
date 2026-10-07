using System.Net;
using System.Net.Http.Json;
using ModularMonolith.Authentication;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class SessionTests : IAsyncLifetime
{
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() => _host = await BffTestHost.StartAsync();

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task SignIn_IssuesHostPrefixedHttpOnlySecureLaxSessionCookie()
    {
        var response = await _host.Client.PostAsJsonAsync(
            BffTestHost.SignInRoute,
            BffTestHost.UserWithRoles(AppRoles.Agent),
            TestContext.Current.CancellationToken);

        var setCookie = SessionCookie.SetCookieHeader(response);

        Assert.NotNull(setCookie);
        var attributes = setCookie.Split(';').Skip(1).Select(attribute => attribute.Trim().ToLowerInvariant()).ToList();
        Assert.Contains("path=/", attributes);
        Assert.Contains("secure", attributes);
        Assert.Contains("httponly", attributes);
        Assert.Contains("samesite=lax", attributes);
        Assert.DoesNotContain(attributes, attribute => attribute.StartsWith("domain=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Session_UsedUntilJustBeforeLifetime_IsNotExtended()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        _host.Time.Advance(TimeSpan.FromHours(4));
        var midSessionResponse = await _host.SendAsync(HttpMethod.Get, "/api/me", session);
        _host.Time.Advance(TimeSpan.FromHours(4) - TimeSpan.FromMinutes(1));
        var lastMinuteResponse = await _host.SendAsync(HttpMethod.Get, "/api/me", session);

        Assert.Equal(HttpStatusCode.OK, midSessionResponse.StatusCode);
        Assert.Null(SessionCookie.SetCookieHeader(midSessionResponse));
        Assert.Equal(HttpStatusCode.OK, lastMinuteResponse.StatusCode);
        Assert.Null(SessionCookie.SetCookieHeader(lastMinuteResponse));
    }

    [Fact]
    public async Task Session_AfterLifetime_Returns401EvenForActiveUser()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        for (var hour = 0; hour < 8; hour++)
        {
            _host.Time.Advance(TimeSpan.FromHours(1) - TimeSpan.FromSeconds(1));
            await _host.SendAsync(HttpMethod.Get, "/api/me", session);
        }

        _host.Time.Advance(TimeSpan.FromMinutes(1));
        var response = await _host.SendAsync(HttpMethod.Get, "/api/me", session);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
