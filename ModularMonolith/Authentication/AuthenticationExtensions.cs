using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
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
    private const string DataProtectionApplicationName = "ClaimApi";

    internal static IServiceCollection AddBffAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<AzureAdSettings>, AzureAdSettingsValidator>();
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

        // The keys encrypt the session cookie: losing them signs every user out. With DataProtection:KeysDirectory
        // (the container's mounted volume) they survive restarts; otherwise the default local key store is used.
        // Phase 3: persist keys to Azure Blob Storage, protected with Key Vault.
        var dataProtection = services.AddDataProtection().SetApplicationName(DataProtectionApplicationName);
        var keysDirectory = configuration[DataProtectionKeysDirectoryKey];
        if (!string.IsNullOrWhiteSpace(keysDirectory))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));
        }

        return services;
    }

    internal static IApplicationBuilder UseBffAuthentication(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<CsrfHeaderMiddleware>();

        return app;
    }
}
