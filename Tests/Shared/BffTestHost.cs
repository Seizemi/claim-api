using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ModularMonolith.Authentication;
using ModularMonolith.Spa;
using Modules.Common.Features;
using Modules.Common.Features.Authorization;
using Xunit;
using AuthenticationModule = Modules.Authentication.Features;
using UsersModule = Modules.Users.Features;

namespace ModularMonolith.Testing;

/// <summary>
/// An in-memory host running the real BFF authentication setup (cookie, OpenID Connect, policies, 401/403 handling,
/// CSRF middleware) and the host's endpoints, without the database Program.cs needs. Entra's metadata is replaced by
/// a static configuration so no network call is made, and the session cookie is issued by a test-only sign-in endpoint.
/// </summary>
public sealed class BffTestHost : IAsyncDisposable
{
    public const string AuthorizationEndpoint = "https://contoso.ciamlogin.com/test/oauth2/v2.0/authorize";
    public const string EndSessionEndpoint = "https://contoso.ciamlogin.com/test/oauth2/v2.0/logout";
    public const string SignInRoute = "/test/sign-in";
    public const string ProtectedPageRoute = "/test/protected-page";
    public const string SupervisorRoute = "/api/test/supervisor";
    public const string AgentRoute = "/api/test/agent";
    public const string StateChangingRoute = "/api/test/state";

    public static readonly IReadOnlyDictionary<string, string?> ValidSettings = new Dictionary<string, string?>
    {
        ["AzureAd:Authority"] = "https://contoso.ciamlogin.com/",
        ["AzureAd:TenantId"] = "11111111-1111-1111-1111-111111111111",
        ["AzureAd:ClientId"] = "22222222-2222-2222-2222-222222222222",
        ["AzureAd:ClientSecret"] = "test-client-secret",
        ["AzureAd:CallbackPath"] = "/signin-oidc",
        ["AzureAd:SignedOutCallbackPath"] = "/signout-callback-oidc",
        ["Session:LifetimeHours"] = "8"
    };

    private readonly WebApplication _app;

    private BffTestHost(WebApplication app, FakeTimeProvider time)
    {
        _app = app;
        Time = time;
        Client = app.GetTestClient();
    }

    public HttpClient Client { get; }

    public FakeTimeProvider Time { get; }

    public static async Task<BffTestHost> StartAsync(IReadOnlyDictionary<string, string?>? settings = null)
    {
        // Production environment: no user-secrets are loaded, and the SPA is served from (an empty) wwwroot.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = Environments.Production });
        builder.WebHost.UseTestServer(options => options.BaseAddress = new Uri("https://localhost/"));
        builder.Configuration.AddInMemoryCollection(settings ?? ValidSettings);

        var time = new FakeTimeProvider(DateTimeOffset.UtcNow);
        builder.Services.AddSingleton<TimeProvider>(time);
        builder.Services.AddProblemDetails();
        builder.Services.AddHealthChecks();
        builder.Services.AddBffAuthentication(builder.Configuration);
        builder.Services.AddSingleton<IDataProtectionProvider, EphemeralDataProtectionProvider>();
        builder.Services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            var configuration = new OpenIdConnectConfiguration
            {
                AuthorizationEndpoint = AuthorizationEndpoint,
                EndSessionEndpoint = EndSessionEndpoint
            };
            options.Configuration = configuration;
            options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
        });

        var app = builder.Build();
        app.UseRouting();
        app.UseBffAuthentication();
        app.MapEndpointModulesFromAssemblyContaining(typeof(AuthenticationModule.DependencyInjection));
        app.MapEndpointModulesFromAssemblyContaining(typeof(UsersModule.DependencyInjection));
        app.MapEndpointModulesFromAssemblyContaining(typeof(Program));
        MapTestEndpoints(app);
        app.MapSpa(app.Environment);

        try
        {
            await app.StartAsync(TestContext.Current.CancellationToken);
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }

        return new BffTestHost(app, time);
    }

    /// <summary>Signs in through the real cookie scheme and returns the session cookie ("name=value").</summary>
    public async Task<string> SignInAsync(params TestClaim[] claims)
    {
        var response = await Client.PostAsJsonAsync(SignInRoute, claims, TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        return SessionCookie.FromResponse(response)
               ?? throw new InvalidOperationException("The sign-in did not issue a session cookie.");
    }

    public static TestClaim[] UserWithRoles(params string[] roles) =>
    [
        new(ClaimConstants.ObjectId, "33333333-3333-3333-3333-333333333333"),
        new(ClaimConstants.Name, "Test User"),
        new("email", "test.user@example.com"),
        .. roles.Select(role => new TestClaim(ClaimConstants.Roles, role))
    ];

    public Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? sessionCookie = null, bool withCsrfHeader = false)
    {
        var request = new HttpRequestMessage(method, url);
        if (sessionCookie is not null)
        {
            request.Headers.Add("Cookie", sessionCookie);
        }

        if (withCsrfHeader)
        {
            request.Headers.Add(CsrfHeaderMiddleware.HeaderName, CsrfHeaderMiddleware.HeaderValue);
        }

        return Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }

    private static void MapTestEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(SignInRoute, (TestClaim[] claims) =>
            {
                var identity = new ClaimsIdentity(
                    claims.Select(claim => new Claim(claim.Type, claim.Value)),
                    authenticationType: "Test",
                    nameType: ClaimConstants.Name,
                    roleType: ClaimConstants.Roles);

                return Results.SignIn(new ClaimsPrincipal(identity), new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
            })
            .AllowAnonymous();

        app.MapGet(ProtectedPageRoute, () => Results.Ok());
        app.MapGet(SupervisorRoute, () => Results.Ok()).RequireAuthorization(AuthPolicies.Supervisor);
        app.MapGet(AgentRoute, () => Results.Ok()).RequireAuthorization(AuthPolicies.Agent);
        app.MapMethods(StateChangingRoute, [HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Patch, HttpMethods.Delete], () => Results.Ok());
    }
}
