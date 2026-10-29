using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using ModularMonolith.KeyVault;

namespace ModularMonolith.Authentication;

/// <summary>
/// Azure: the authorization code is redeemed with a client assertion signed by the web app registration's Key Vault
/// certificate, instead of a client secret (Microsoft.Identity.Web only sends a secret when it doesn't acquire tokens).
/// </summary>
internal static class CertificateCodeRedemption
{
    internal static void UseClientAssertion(OpenIdConnectOptions options)
    {
        var previous = options.Events.OnAuthorizationCodeReceived;
        options.Events.OnAuthorizationCodeReceived = async context =>
        {
            await previous(context);
            await AddClientAssertionAsync(context);
        };
    }

    internal static async Task AddClientAssertionAsync(AuthorizationCodeReceivedContext context)
    {
        if (context.TokenEndpointRequest is null || context.HandledCodeRedemption)
        {
            return;
        }

        var services = context.HttpContext.RequestServices;
        var certificateName = services.GetRequiredService<IOptions<KeyVaultSettings>>().Value.WebCertificateName!;
        var cancellationToken = context.HttpContext.RequestAborted;

        // The audience is the token endpoint the handler posts the code to (from the tenant's metadata, cached).
        var configuration = await context.Options.ConfigurationManager!.GetConfigurationAsync(cancellationToken);

        context.TokenEndpointRequest.ClientSecret = null;
        context.TokenEndpointRequest.ClientAssertionType = ClientAssertions.AssertionType;
        context.TokenEndpointRequest.ClientAssertion = await services.GetRequiredService<ClientAssertions>()
            .CreateAsync(context.Options.ClientId!, certificateName, configuration.TokenEndpoint, cancellationToken);
    }
}
