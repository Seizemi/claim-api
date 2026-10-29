using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModularMonolith.ManagedIdentity;

internal static class ManagedIdentityExtensions
{
    /// <summary>
    /// The web app's system-assigned managed identity, for the Azure resources reached without a secret
    /// (PostgreSQL, the Data Protection blob and Key Vault key). Created once: the credential caches its tokens.
    /// </summary>
    internal static IServiceCollection AddManagedIdentityCredential(this IServiceCollection services)
    {
        services.TryAddSingleton<TokenCredential>(_ => new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned));

        return services;
    }
}
