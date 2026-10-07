using System.Net;
using ModularMonolith.Testing;
using Xunit;

namespace Modules.Authentication.Features.Tests.Features.Login;

public sealed class LoginTests : IAsyncLifetime
{
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() => _host = await BffTestHost.StartAsync();

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    [Theory]
    [InlineData("/claims/42")]
    [InlineData("https://evil.example.com")]
    public async Task Login_WithoutSession_RedirectsToEntraAuthorizeEndpoint(string returnUrl)
    {
        var response = await _host.SendAsync(HttpMethod.Get, $"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location!.ToString();
        Assert.StartsWith(BffTestHost.AuthorizationEndpoint, location);
        Assert.Contains("response_type=code", location);
        Assert.Contains("code_challenge=", location);
        Assert.Contains($"redirect_uri={Uri.EscapeDataString("https://localhost/signin-oidc")}", location);
    }

    [Theory]
    [InlineData("/claims/42/information?tab=history", "/claims/42/information?tab=history")]
    [InlineData("https://evil.example.com/phish", "/")]
    [InlineData("//evil.example.com", "/")]
    [InlineData(null, "/")]
    public async Task Login_WithSession_RedirectsToLocalReturnUrlOnly(string? returnUrl, string expectedLocation)
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(TestRoles.Agent));
        var url = returnUrl is null ? "/auth/login" : $"/auth/login?returnUrl={Uri.EscapeDataString(returnUrl)}";

        var response = await _host.SendAsync(HttpMethod.Get, url, session);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location!.OriginalString);
    }
}
