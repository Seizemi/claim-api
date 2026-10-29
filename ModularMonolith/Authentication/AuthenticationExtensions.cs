using Azure.Core;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using ModularMonolith.KeyVault;
using Modules.Common.Features.Authorization;

namespace ModularMonolith.Authentication;

/// <summary>
/// Backend-for-Frontend authentication: OpenID Connect sign-in against Entra External ID (confidential client)
/// and an encrypted session cookie. No token is kept: the cookie only holds the ID token's claims.
/// </summary>
internal static class AuthenticationExtensions
{
    internal const string SessionCookieName = "__Host-PartnerPortal";
    internal const string DataProtectionKeysDirectoryKey = "DataProtection:KeysDirectory";
    internal const string DataProtectionBlobUriKey = "DataProtection:BlobUri";
    internal const string DataProtectionKeyVaultKeyKey = "DataProtection:KeyVaultKey";
    private const string DataProtectionApplicationName = "ClaimApi";

    internal static IServiceCollection AddBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var usesCertificate = KeyVaultSettings.IsConfigured(configuration);

        services.AddSingleton<IValidateOptions<AzureAdSettings>>(new AzureAdSettingsValidator(usesCertificate));
        services.AddOptions<AzureAdSettings>()
            .BindConfiguration(AzureAdSettings.SectionName)
            .ValidateOnStart();

        services.AddOptions<SessionSettings>()
            .BindConfiguration(SessionSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApp(identityOptions =>
            {
                configuration.GetSection(AzureAdSettings.SectionName).Bind(identityOptions);
                identityOptions.MapInboundClaims = false;
                identityOptions.TokenValidationParameters.RoleClaimType = ClaimConstants.Roles;
            });

        // PostConfigure runs after Microsoft.Identity.Web's own configuration, so these values always win.
        services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            options.SaveTokens = false;
            options.UseTokenLifetime = false;

            if (usesCertificate)
            {
                CertificateCodeRedemption.UseClientAssertion(options);
            }
        });

        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<SessionSettings>>((options, sessionSettings) =>
            {
                options.Cookie.Name = SessionCookieName;
                options.Cookie.Path = "/";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.ExpireTimeSpan = TimeSpan.FromHours(sessionSettings.Value.LifetimeHours);
                options.SlidingExpiration = false;
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthPolicies.Supervisor, policy => policy.RequireRole(AppRoles.Supervisor))
            .AddPolicy(AuthPolicies.Agent, policy => policy.RequireRole(AppRoles.Agent, AppRoles.Supervisor))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ApiAuthorizationResultHandler>();

        services.AddSessionDataProtection(configuration);

        return services;
    }

    /// <summary>
    /// The keys encrypt and sign the session cookie: losing them signs every user out, stealing them allows
    /// forging a session. Where they live:
    /// - Azure (DataProtection:BlobUri + DataProtection:KeyVaultKey): one blob shared by every instance and kept
    ///   across deployments, each key wrapped by the Key Vault key. Both reached with the managed identity.
    /// - Docker (DataProtection:KeysDirectory): the container's mounted volume, so they survive restarts.
    /// - Otherwise: the default local key store (dotnet run).
    /// </summary>
    private static void AddSessionDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var dataProtection = services.AddDataProtection().SetApplicationName(DataProtectionApplicationName);

        var blobUri = configuration[DataProtectionBlobUriKey];
        var keyVaultKey = configuration[DataProtectionKeyVaultKeyKey];
        var keysDirectory = configuration[DataProtectionKeysDirectoryKey];

        var hasBlobUri = !string.IsNullOrWhiteSpace(blobUri);
        var hasKeyVaultKey = !string.IsNullOrWhiteSpace(keyVaultKey);
        if (hasBlobUri != hasKeyVaultKey)
        {
            // Keys in the blob without Key Vault would be readable by anyone who can read the blob.
            throw new InvalidOperationException(
                $"'{DataProtectionBlobUriKey}' and '{DataProtectionKeyVaultKeyKey}' must be set together.");
        }

        if (hasBlobUri && !string.IsNullOrWhiteSpace(keysDirectory))
        {
            throw new InvalidOperationException(
                $"'{DataProtectionKeysDirectoryKey}' can't be combined with '{DataProtectionBlobUriKey}': choose one key store.");
        }

        if (hasBlobUri)
        {
            dataProtection
                .PersistKeysToAzureBlobStorage(serviceProvider =>
                    new BlobClient(new Uri(blobUri!), serviceProvider.GetRequiredService<TokenCredential>()))
                .ProtectKeysWithAzureKeyVault(new Uri(keyVaultKey!), serviceProvider =>
                    serviceProvider.GetRequiredService<TokenCredential>());
        }
        else if (!string.IsNullOrWhiteSpace(keysDirectory))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));
        }
    }

    internal static IApplicationBuilder UseBffAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<CsrfHeaderMiddleware>();

        return app;
    }
}
