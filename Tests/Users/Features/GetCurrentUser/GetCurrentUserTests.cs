using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Identity.Web;
using ModularMonolith.Testing;
using Modules.Users.Features.Features.GetCurrentUser;
using Xunit;

namespace Modules.Users.Features.Tests.Features.GetCurrentUser;

public sealed class GetCurrentUserTests : IAsyncLifetime
{
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() => _host = await BffTestHost.StartAsync();

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task GetCurrentUser_WithSession_ReturnsIdentityAndRolesFromClaims()
    {
        var session = await _host.SignInAsync(
            new TestClaim(ClaimConstants.ObjectId, "33333333-3333-3333-3333-333333333333"),
            new TestClaim(ClaimConstants.Name, "Alice Martin"),
            new TestClaim("email", "alice@example.com"),
            new TestClaim(ClaimConstants.Roles, TestRoles.Supervisor));

        var response = await _host.SendAsync(HttpMethod.Get, "/api/me", session);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var root = document.RootElement;
        Assert.Equal("33333333-3333-3333-3333-333333333333", root.GetProperty("id").GetString());
        Assert.Equal("Alice Martin", root.GetProperty("displayName").GetString());
        Assert.Equal("alice@example.com", root.GetProperty("email").GetString());
        Assert.Equal([TestRoles.Supervisor], root.GetProperty("roles").EnumerateArray().Select(role => role.GetString()));
    }

    [Fact]
    public async Task GetCurrentUser_WithoutSession_Returns401()
    {
        var response = await _host.SendAsync(HttpMethod.Get, "/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("email")]
    [InlineData("emails")]
    [InlineData(ClaimConstants.PreferredUserName)]
    [InlineData(ClaimTypes.Email)]
    public void ToCurrentUserResponse_WithEmailInAnySupportedClaim_ReturnsIt(string emailClaimType)
    {
        var user = Principal(new Claim(emailClaimType, "bob@example.com"));

        var response = GetCurrentUserEndpoint.ToCurrentUserResponse(user);

        Assert.Equal("bob@example.com", response.Email);
    }

    [Fact]
    public void ToCurrentUserResponse_WithEmailAndPreferredUsername_PrefersEmail()
    {
        var user = Principal(
            new Claim(ClaimConstants.PreferredUserName, "bob-upn@example.com"),
            new Claim("email", "bob@example.com"));

        var response = GetCurrentUserEndpoint.ToCurrentUserResponse(user);

        Assert.Equal("bob@example.com", response.Email);
    }

    [Fact]
    public void ToCurrentUserResponse_WithMappedObjectIdClaim_ReturnsIt()
    {
        var user = Principal(new Claim(ClaimConstants.Oid, "44444444-4444-4444-4444-444444444444"));

        var response = GetCurrentUserEndpoint.ToCurrentUserResponse(user);

        Assert.Equal("44444444-4444-4444-4444-444444444444", response.Id);
    }

    [Fact]
    public void ToCurrentUserResponse_WithoutOptionalClaims_ReturnsEmptyValues()
    {
        var response = GetCurrentUserEndpoint.ToCurrentUserResponse(Principal());

        Assert.Equal(string.Empty, response.Id);
        Assert.Equal(string.Empty, response.DisplayName);
        Assert.Equal(string.Empty, response.Email);
        Assert.Empty(response.Roles);
    }

    [Fact]
    public void ToCurrentUserResponse_WithDuplicateRoles_ReturnsEachOnce()
    {
        var user = Principal(
            new Claim(ClaimConstants.Roles, TestRoles.Agent),
            new Claim(ClaimConstants.Roles, TestRoles.Agent));

        var response = GetCurrentUserEndpoint.ToCurrentUserResponse(user);

        Assert.Equal([TestRoles.Agent], response.Roles);
    }

    private static ClaimsPrincipal Principal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test", ClaimConstants.Name, ClaimConstants.Roles));
}
