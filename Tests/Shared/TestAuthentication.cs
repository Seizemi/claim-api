using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Testing;

/// <summary>Signs test clients in when the whole application (Program.cs) runs in a WebApplicationFactory.</summary>
public static class TestAuthentication
{
    /// <summary>
    /// Placeholder tenant values, so startup validation passes without the developer's user-secrets (e.g. in CI),
    /// and <see cref="TestAuthenticationHandler"/> as the way requests are authenticated.
    /// </summary>
    public static IWebHostBuilder UseTestAuthentication(this IWebHostBuilder builder)
    {
        foreach (var (key, value) in BffTestHost.ValidSettings)
        {
            builder.UseSetting(key, value);
        }

        return builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
                options.DefaultAuthenticateScheme = TestAuthenticationHandler.SchemeName);
        });
    }

    /// <summary>Sends the CSRF header on every request, like the React app.</summary>
    public static HttpClient WithCsrfHeader(this HttpClient client)
    {
        client.DefaultRequestHeaders.Add(TestCsrf.HeaderName, TestCsrf.HeaderValue);

        return client;
    }

    /// <summary>Authenticates every request of <paramref name="client"/> as <paramref name="user"/>.</summary>
    public static HttpClient SignInAs(this HttpClient client, TestUser user)
    {
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserIdHeader, user.Id.ToString());
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.UserNameHeader, user.Name);
        client.DefaultRequestHeaders.Add(TestAuthenticationHandler.RolesHeader, string.Join(',', user.Roles));

        return client;
    }
}
