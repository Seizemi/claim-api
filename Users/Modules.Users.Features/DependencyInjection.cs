using ClaimPartners.Users.DependencyInjection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Modules.Users.Features.Configuration;
using Modules.Users.Features.Email;
using Modules.Users.Features.Extensions;
using Modules.Users.Features.RoleAssignment;
using Modules.Users.Infrastructure;

namespace Modules.Users.Features;

/// <summary>
/// Entry point of the Users module. The host also uses it as the marker that maps this module's endpoints
/// (MapEndpointModulesFromAssemblyContaining).
/// </summary>
public static class DependencyInjection
{
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="graphClientAssertion">
    /// Signs the Graph app registration's client assertions (Azure: certificate in Key Vault). Null: the
    /// <c>PartnerUsers:GraphClientSecret</c> is used (local, Docker).
    /// </param>
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IServiceProvider, GraphClientAssertionCallback>? graphClientAssertion = null)
    {
        services.AddUsersInfrastructure();
        if (graphClientAssertion is null)
        {
            services.AddPartnerUserManagement(configuration);
        }
        else
        {
            services.AddPartnerUserManagement(configuration, serviceProvider =>
                new GraphClientAssertionProvider(graphClientAssertion(serviceProvider)));
        }
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IValidateOptions<UserManagementSettings>, UserManagementSettingsValidator>();
        services.AddOptions<UserManagementSettings>()
            .BindConfiguration(UserManagementSettings.SectionName)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AppSettings>, AppSettingsValidator>();
        services.AddOptions<AppSettings>()
            .BindConfiguration(AppSettings.SectionName)
            .ValidateOnStart();

        // Until an email provider is chosen (spec D2).
        services.AddSingleton<IEmailSender, LoggingEmailSender>();

        // Roles of new users are assigned in the background (spec 5.8).
        services.AddSingleton<RoleAssignmentSignal>();
        services.AddScoped<IRoleAssignmentProcessor, RoleAssignmentProcessor>();
        services.AddHostedService<RoleAssignmentWorker>();

        services.RegisterHandlersFromAssemblyContaining(typeof(DependencyInjection));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
