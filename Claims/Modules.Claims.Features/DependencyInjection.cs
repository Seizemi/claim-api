using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Features.Extensions;
using Modules.Claims.Infrastructure;

namespace Modules.Claims.Features;

public static class DependencyInjection
{
    public static IServiceCollection AddClaimsModule(this IServiceCollection services, IConfiguration configuration, bool enableSensitiveDataLogging)
    {
        services.AddClaimsInfrastructure(configuration, enableSensitiveDataLogging);
        services.RegisterHandlersFromAssemblyContaining(typeof(DependencyInjection));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
