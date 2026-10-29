using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Claims.Infrastructure.Database;
using Npgsql;

namespace Modules.Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddClaimsInfrastructure(this IServiceCollection services, bool enableSensitiveDataLogging)
    {
        // The data source (connection string, Entra token in Azure) is registered by the host.
        services.AddDbContext<ClaimsDbContext>((serviceProvider, x) =>
        {
            x.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>(), npgsqlOptions =>
                    npgsqlOptions.MigrationsHistoryTable(DbConsts.MigrationHistoryTableName, DbConsts.ClaimsSchemaName))
                .UseSnakeCaseNamingConvention();

            if (enableSensitiveDataLogging)
            {
                x.EnableSensitiveDataLogging();
            }
        });

        services.AddHealthChecks()
            .AddDbContextCheck<ClaimsDbContext>();

        return services;
    }
}
