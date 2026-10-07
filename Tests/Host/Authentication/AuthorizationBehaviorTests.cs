using System.Net;
using System.Text.Json;
using ModularMonolith.Authentication;
using ModularMonolith.Testing;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class AuthorizationBehaviorTests : IAsyncLifetime
{
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() => _host = await BffTestHost.StartAsync();

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task ApiRequest_WithoutSession_Returns401ProblemDetailsInsteadOfRedirect()
    {
        var response = await _host.SendAsync(HttpMethod.Get, "/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Null(response.Headers.Location);
        await AssertProblemDetailsAsync(response, 401);
    }

    [Fact]
    public async Task StateChangingApiRequest_WithoutSessionOrCsrfHeader_Returns401()
    {
        var response = await _host.SendAsync(HttpMethod.Post, BffTestHost.StateChangingRoute);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnknownApiRoute_WithoutSession_Returns401()
    {
        var response = await _host.SendAsync(HttpMethod.Get, "/api/does-not-exist");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedPage_WithoutSession_RedirectsToEntra()
    {
        var response = await _host.SendAsync(HttpMethod.Get, BffTestHost.ProtectedPageRoute);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith(BffTestHost.AuthorizationEndpoint, response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SupervisorEndpoint_AsAgent_Returns403ProblemDetails()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        var response = await _host.SendAsync(HttpMethod.Get, BffTestHost.SupervisorRoute, session);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(response.Headers.Location);
        await AssertProblemDetailsAsync(response, 403);
    }

    [Theory]
    [InlineData(BffTestHost.SupervisorRoute, AppRoles.Supervisor)]
    [InlineData(BffTestHost.AgentRoute, AppRoles.Agent)]
    [InlineData(BffTestHost.AgentRoute, AppRoles.Supervisor)]
    public async Task PolicyEndpoint_WithSatisfyingRole_Returns200(string route, string role)
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(role));

        var response = await _host.SendAsync(HttpMethod.Get, route, session);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(BffTestHost.SupervisorRoute)]
    [InlineData(BffTestHost.AgentRoute)]
    public async Task PolicyEndpoint_SignedInWithoutRole_Returns403(string route)
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles());

        var response = await _host.SendAsync(HttpMethod.Get, route, session);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StateChangingApiRequest_SignedInWithoutCsrfHeader_Returns400()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        var response = await _host.SendAsync(HttpMethod.Post, BffTestHost.StateChangingRoute, session);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertProblemDetailsAsync(response, 400);
    }

    [Fact]
    public async Task StateChangingApiRequest_SignedInWithCsrfHeader_Returns200()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        var response = await _host.SendAsync(HttpMethod.Delete, BffTestHost.StateChangingRoute, session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownApiRoute_SignedIn_Returns404InsteadOfSpa()
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(AppRoles.Agent));

        var response = await _host.SendAsync(HttpMethod.Get, "/api/does-not-exist", session);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Healthz_WithoutSession_Returns200()
    {
        var response = await _host.SendAsync(HttpMethod.Get, "/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task AssertProblemDetailsAsync(HttpResponseMessage response, int expectedStatus)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetInt32());
    }
}
