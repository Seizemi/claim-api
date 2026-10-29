using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ModularMonolith.Authentication;
using ModularMonolith.KeyVault;
using ModularMonolith.Tests.KeyVault;
using Xunit;

namespace ModularMonolith.Tests.Authentication;

public sealed class CertificateCodeRedemptionTests
{
    private const string ClientId = "22222222-2222-2222-2222-222222222222";
    private const string TokenEndpoint = "https://contoso.ciamlogin.com/11111111-1111-1111-1111-111111111111/oauth2/v2.0/token";

    [Fact]
    public async Task OnAuthorizationCodeReceived_ReplacesTheSecretWithAClientAssertionForTheTokenEndpoint()
    {
        var context = CreateContext();
        context.TokenEndpointRequest = new OpenIdConnectMessage { ClientId = ClientId, ClientSecret = "<set-in-user-secrets>", Code = "code" };

        await context.Options.Events.AuthorizationCodeReceived(context);

        Assert.Null(context.TokenEndpointRequest.ClientSecret);
        Assert.Equal("urn:ietf:params:oauth:client-assertion-type:jwt-bearer", context.TokenEndpointRequest.ClientAssertionType);
        var assertion = new JsonWebToken(context.TokenEndpointRequest.ClientAssertion);
        Assert.Equal([TokenEndpoint], assertion.Audiences);
        Assert.Equal(ClientId, assertion.Issuer);
        Assert.Equal("code", context.TokenEndpointRequest.Code);
    }

    [Fact]
    public async Task OnAuthorizationCodeReceived_RunsTheEventAlreadyRegisteredFirst()
    {
        var context = CreateContext(previous: received =>
        {
            received.HandleCodeRedemption();
            return Task.CompletedTask;
        });
        context.TokenEndpointRequest = new OpenIdConnectMessage { ClientId = ClientId, Code = "code" };

        await context.Options.Events.AuthorizationCodeReceived(context);

        // The code was redeemed by the earlier handler: nothing to sign.
        Assert.Null(context.TokenEndpointRequest.ClientAssertion);
    }

    private static AuthorizationCodeReceivedContext CreateContext(Func<AuthorizationCodeReceivedContext, Task>? previous = null)
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var certificate = TestCertificates.Create();
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(new KeyVaultSettings { Uri = "https://kv.vault.azure.net/", WebCertificateName = "partner-portal-web", GraphCertificateName = "partner-portal-graph" }))
            .AddSingleton(new ClientAssertions(new KeyVaultCertificates(TestCertificates.SecretClientServing(() => certificate).Object, timeProvider, NullLogger<KeyVaultCertificates>.Instance), timeProvider))
            .BuildServiceProvider();

        var options = new OpenIdConnectOptions
        {
            ClientId = ClientId,
            ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(new OpenIdConnectConfiguration { TokenEndpoint = TokenEndpoint })
        };
        if (previous is not null)
        {
            options.Events.OnAuthorizationCodeReceived = previous;
        }

        CertificateCodeRedemption.UseClientAssertion(options);

        var httpContext = new DefaultHttpContext { RequestServices = services };
        var scheme = new AuthenticationScheme(OpenIdConnectDefaults.AuthenticationScheme, null, typeof(OpenIdConnectHandler));

        return new AuthorizationCodeReceivedContext(httpContext, scheme, options, new AuthenticationProperties());
    }
}
