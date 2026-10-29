using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Modules.Users.Features;

namespace ModularMonolith.KeyVault;

internal static class KeyVaultExtensions
{
    /// <summary>
    /// When <c>KeyVault:Uri</c> is set (Azure), registers the certificates and the client assertions signed with them,
    /// read with the managed identity. Otherwise registers nothing: client secrets are used (local, Docker).
    /// </summary>
    internal static IServiceCollection AddKeyVaultCertificates(this IServiceCollection services, IConfiguration configuration)
    {
        if (!KeyVaultSettings.IsConfigured(configuration))
        {
            return services;
        }

        services.AddSingleton<IValidateOptions<KeyVaultSettings>, KeyVaultSettingsValidator>();
        services.AddOptions<KeyVaultSettings>()
            .BindConfiguration(KeyVaultSettings.SectionName)
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(serviceProvider => new SecretClient(
            new Uri(serviceProvider.GetRequiredService<IOptions<KeyVaultSettings>>().Value.Uri!),
            serviceProvider.GetRequiredService<TokenCredential>()));
        services.AddSingleton<KeyVaultCertificates>();
        services.AddSingleton<ClientAssertions>();

        return services;
    }

    /// <summary>Signs the Graph app registration's client assertions with its Key Vault certificate.</summary>
    internal static GraphClientAssertionCallback GraphClientAssertion(IServiceProvider serviceProvider)
    {
        var assertions = serviceProvider.GetRequiredService<ClientAssertions>();
        var certificateName = serviceProvider.GetRequiredService<IOptions<KeyVaultSettings>>().Value.GraphCertificateName!;

        return (clientId, audience, cancellationToken) => assertions.CreateAsync(clientId, certificateName, audience, cancellationToken);
    }
}
