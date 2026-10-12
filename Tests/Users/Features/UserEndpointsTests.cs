using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClaimPartners.Users.Core;
using ClaimPartners.Users.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using ModularMonolith.Testing;
using Modules.Users.Features.Email;
using Modules.Users.Features.Features.Shared.Errors;
using Moq;
using Xunit;

namespace Modules.Users.Features.Tests.Features;

/// <summary>
/// The /api/users endpoints through the real BFF pipeline (session cookie, policies, CSRF, ProblemDetails), with the
/// package mocked.
/// </summary>
public sealed class UserEndpointsTests : IAsyncLifetime
{
    private const string SupervisorId = "33333333-3333-3333-3333-333333333333";
    private const string AgentId = "bbbbbbbb-0000-0000-0000-000000000002";
    private static readonly PartnerUser Agent = new(AgentId, "Ann Agent", "agent@example.com", TestRoles.Agent);

    private readonly Mock<IPartnerUserService> _partnerUsers = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private BffTestHost _host = null!;

    public async ValueTask InitializeAsync() =>
        _host = await BffTestHost.StartAsync(configureServices: services =>
        {
            services.AddSingleton(_partnerUsers.Object);
            services.AddSingleton(_emailSender.Object);
        });

    public async ValueTask DisposeAsync() => await _host.DisposeAsync();

    public static TheoryData<string, string> Endpoints => new()
    {
        { "GET", "/api/users" },
        { "POST", "/api/users" },
        { "DELETE", $"/api/users/{AgentId}" }
    };

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task UsersEndpoint_WithoutSession_Returns401(string method, string url)
    {
        var response = await SendAsync(method, url, session: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task UsersEndpoint_AsAgent_Returns403AndNeverCallsGraph(string method, string url)
    {
        var session = await _host.SignInAsync(BffTestHost.UserWithRoles(TestRoles.Agent));

        var response = await SendAsync(method, url, session);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListUsers_AsSupervisor_Returns200WithUsers()
    {
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([Agent]);
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Get, "/api/users", session);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        var user = Assert.Single(document.RootElement.EnumerateArray());
        Assert.Equal(AgentId, user.GetProperty("id").GetString());
        Assert.Equal("Ann Agent", user.GetProperty("displayName").GetString());
        Assert.Equal("agent@example.com", user.GetProperty("email").GetString());
        Assert.Equal(TestRoles.Agent, user.GetProperty("role").GetString());
    }

    [Fact]
    public async Task CreateUser_AsSupervisor_Returns201WithLocationAndUser()
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Agent);
        var session = await SignInAsSupervisorAsync();

        var response = await PostUserAsync(session, new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal($"/api/users/{AgentId}", response.Headers.Location?.OriginalString);
        using var document = await ReadJsonAsync(response);
        Assert.Equal(AgentId, document.RootElement.GetProperty("id").GetString());
        Assert.Equal(TestRoles.Agent, document.RootElement.GetProperty("role").GetString());
        Assert.Equal("Pending", document.RootElement.GetProperty("status").GetString());
        // Sent by the role assignment worker once the role is assigned.
        _emailSender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListUsers_AfterCreate_ListsTheNewUserAsPending()
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Agent);
        _partnerUsers.Setup(service => service.ListUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var session = await SignInAsSupervisorAsync();
        await PostUserAsync(session, new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent });

        var response = await _host.SendAsync(HttpMethod.Get, "/api/users", session);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        var user = Assert.Single(document.RootElement.EnumerateArray());
        Assert.Equal(AgentId, user.GetProperty("id").GetString());
        Assert.Equal("Pending", user.GetProperty("status").GetString());
    }

    [Fact]
    public async Task CreateUser_WithoutCsrfHeader_Returns400AndCreatesNothing()
    {
        var session = await SignInAsSupervisorAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/users")
        {
            Content = JsonContent.Create(new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent })
        };
        request.Headers.Add("Cookie", session);

        var response = await _host.Client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("CSRF", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUser_WithSupervisorRole_Returns400AndCreatesNothing()
    {
        var session = await SignInAsSupervisorAsync();

        var response = await PostUserAsync(session, new { email = "boss@example.com", displayName = "Boss", role = TestRoles.Supervisor });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        Assert.True(document.RootElement.GetProperty("errors").TryGetProperty("Role", out _));
        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_Returns400ValidationProblem()
    {
        var session = await SignInAsSupervisorAsync();

        var response = await PostUserAsync(session, new { email = "nope", displayName = "Ann Agent", role = TestRoles.Agent });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var document = await ReadJsonAsync(response);
        Assert.True(document.RootElement.GetProperty("errors").TryGetProperty("Email", out _));
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_Returns409Problem()
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.UserAlreadyExists, "exists"));
        var session = await SignInAsSupervisorAsync();

        var response = await PostUserAsync(session, new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var document = await ReadJsonAsync(response);
        Assert.True(document.RootElement.GetProperty("errors").TryGetProperty(UserErrorCodes.AlreadyExists, out _));
        _emailSender.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUser_WhenGraphFails_Returns502()
    {
        _partnerUsers.Setup(service => service.CreateUserAsync(It.IsAny<CreatePartnerUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PartnerUserException(PartnerUserErrorCode.GraphError, "Graph internals"));
        var session = await SignInAsSupervisorAsync();

        var response = await PostUserAsync(session, new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.DoesNotContain("Graph internals", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DeleteUser_AsSupervisor_Returns204()
    {
        _partnerUsers.Setup(service => service.GetUserAsync(AgentId, It.IsAny<CancellationToken>())).ReturnsAsync(Agent);
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Delete, $"/api/users/{AgentId}", session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        _partnerUsers.Verify(service => service.DeleteUserAsync(AgentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_Self_Returns400()
    {
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Delete, $"/api/users/{SupervisorId}", session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains(UserErrorCodes.CannotDeleteSelf, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        _partnerUsers.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteUser_AnotherSupervisor_Returns400()
    {
        const string otherSupervisorId = "cccccccc-0000-0000-0000-000000000003";
        _partnerUsers.Setup(service => service.GetUserAsync(otherSupervisorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PartnerUser(otherSupervisorId, "Other", "other@example.com", TestRoles.Supervisor));
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Delete, $"/api/users/{otherSupervisorId}", session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _partnerUsers.Verify(service => service.DeleteUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_Unknown_Returns404()
    {
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Delete, $"/api/users/{AgentId}", session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithNonGuidId_Returns404WithoutCallingGraph()
    {
        var session = await SignInAsSupervisorAsync();

        var response = await _host.SendAsync(HttpMethod.Delete, "/api/users/not-a-guid", session, withCsrfHeader: true);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        _partnerUsers.VerifyNoOtherCalls();
    }

    private Task<string> SignInAsSupervisorAsync() =>
        _host.SignInAsync(
            new TestClaim(ClaimConstants.ObjectId, SupervisorId),
            new TestClaim(ClaimConstants.Name, "Sam Supervisor"),
            new TestClaim(ClaimConstants.Roles, TestRoles.Supervisor));

    private Task<HttpResponseMessage> PostUserAsync(string session, object body) =>
        SendAsync(HttpMethod.Post.Method, "/api/users", session, body);

    // A POST without a JSON body doesn't match the endpoint at all, so the request always carries one, as the
    // React app's would.
    private Task<HttpResponseMessage> SendAsync(string method, string url, string? session, object? body = null)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        if (method == HttpMethod.Post.Method)
        {
            request.Content = JsonContent.Create(body ?? new { email = "agent@example.com", displayName = "Ann Agent", role = TestRoles.Agent });
        }

        if (session is not null)
        {
            request.Headers.Add("Cookie", session);
        }

        request.Headers.Add(TestCsrf.HeaderName, TestCsrf.HeaderValue);

        return _host.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
}
